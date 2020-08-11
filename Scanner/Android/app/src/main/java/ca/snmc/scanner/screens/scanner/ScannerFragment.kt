package ca.snmc.scanner.screens.scanner

import android.Manifest
import android.content.pm.PackageManager
import android.media.MediaPlayer
import android.os.Bundle
import android.os.Handler
import android.os.Looper
import android.util.SparseArray
import android.view.*
import androidx.core.app.ActivityCompat
import androidx.core.util.isNotEmpty
import androidx.fragment.app.Fragment
import androidx.lifecycle.Observer
import androidx.lifecycle.ViewModelProvider
import androidx.lifecycle.lifecycleScope
import androidx.navigation.fragment.findNavController
import androidx.recyclerview.widget.LinearLayoutManager
import ca.snmc.scanner.MainActivity
import ca.snmc.scanner.R
import ca.snmc.scanner.databinding.ScannerFragmentBinding
import ca.snmc.scanner.models.Error
import ca.snmc.scanner.models.ScanHistoryItem
import ca.snmc.scanner.utils.*
import ca.snmc.scanner.utils.adapters.ScanHistoryRecyclerViewAdapter
import com.google.android.gms.vision.CameraSource
import com.google.android.gms.vision.Detector
import com.google.android.gms.vision.barcode.Barcode
import com.google.android.gms.vision.barcode.BarcodeDetector
import kotlinx.android.synthetic.main.scanner_fragment.*
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.cancel
import kotlinx.coroutines.launch
import kotlinx.coroutines.withContext
import org.kodein.di.KodeinAware
import org.kodein.di.android.x.kodein
import org.kodein.di.generic.instance
import java.io.IOException
import java.util.*
import kotlin.collections.ArrayList

private const val SUCCESS_NOTIFICATION_TIMEOUT = 3000.toLong()
private const val FAILURE_NOTIFICATION_TIMEOUT = 10000.toLong()
private const val WARNING_NOTIFICATION_TIMEOUT = 10000.toLong()
private const val INFECTED_VISITOR_NOTIFICATION_TIMEOUT = 10000.toLong()
private const val SCAN_HISTORY_MAX_SIZE = 10
class ScannerFragment : Fragment(), KodeinAware {

    override val kodein by kodein()
    private val scannerViewModelFactory : ScannerViewModelFactory by instance()

    private lateinit var binding : ScannerFragmentBinding
    private lateinit var viewModel: ScannerViewModel

    private var isSuccess = true

    private lateinit var cameraSource: CameraSource
    private lateinit var detector: BarcodeDetector

    private lateinit var savedSurfaceHolder: SurfaceHolder

    // Used to prevent duplication
    private var scanComplete: Boolean = false

    private var successNotification : MediaPlayer? = null
    private var failureNotification : MediaPlayer? = null
    private var unverifiedNotification : MediaPlayer? = null
    private var infectedNotification : MediaPlayer? = null

    private lateinit var scanHistoryAdapter : ScanHistoryRecyclerViewAdapter

    override fun onCreateView(
        inflater: LayoutInflater, container: ViewGroup?,
        savedInstanceState: Bundle?
    ): View? {
        (activity as MainActivity).fullscreenMode()

        binding = ScannerFragmentBinding.inflate(inflater, container, false)

        // Set LifecycleOwner on Binding object
        binding.lifecycleOwner = this

        binding.settingsButton.setOnClickListener {
            navigate()
        }

        binding.scanHistoryButton.setOnClickListener {
            handleScanHistoryDrawer()
        }

        // ViewModel
        viewModel = ViewModelProvider(this, scannerViewModelFactory).get(ScannerViewModel::class.java)

        // Get Necessary Data from Local DB
        viewLifecycleOwner.lifecycleScope.launch {
            viewModel.initialize()
        }

        // Return the View at the Root of the Binding object
        return binding.root
    }

    override fun onViewCreated(view: View, savedInstanceState: Bundle?) {
        super.onViewCreated(view, savedInstanceState)

        initRecyclerView()
        loadData()
        loadVisitSettings()
        setupScanner()
        setupSounds()

    }

    private fun initRecyclerView() {

        viewLifecycleOwner.lifecycleScope.launch {
            scanHistoryAdapter = ScanHistoryRecyclerViewAdapter()
            scan_history_recycler_view.apply {
                layoutManager = LinearLayoutManager(requireActivity())
                adapter = scanHistoryAdapter
            }

            // Observe the observable
            viewModel.scanHistoryObservable.observe(viewLifecycleOwner, Observer {
//                Log.e("Observing", it.toString())
                scanHistoryAdapter.submitList(it.toList())
                scanHistoryAdapter.notifyDataSetChanged()
            })
        }

    }

    private fun updateRecyclerView(text: String, backgroundResource: Int) {

        viewLifecycleOwner.lifecycleScope.launch {
//            Log.e("Updating", viewModel.scanHistory.toString())
            // Remove the last item if the list exceeds the max size
            if (viewModel.scanHistory.count() == SCAN_HISTORY_MAX_SIZE) {
                viewModel.scanHistory = viewModel.scanHistory.dropLast(1) as ArrayList<ScanHistoryItem>
            }

            // Add the latest item to the top
            viewModel.scanHistory.add(0, ScanHistoryItem(text, backgroundResource))

            // Update the observable
            viewModel.scanHistoryObservable.postValue(viewModel.scanHistory)
        }

    }

    private fun loadData() {
        viewLifecycleOwner.lifecycleScope.launch {
            onStarted()
            viewModel.getMergedData().observe(viewLifecycleOwner, Observer {
                if (it?.authorization != null && it.username != null && it.password != null) {
                    loadVisitSettings()
                    onDataLoaded()
                    coroutineContext.cancel()
                } else {
                    onStarted()
                }
            })
        }
    }

    private fun setupScanner() {
        detector = BarcodeDetector
            .Builder(requireActivity())
            .setBarcodeFormats(Barcode.QR_CODE).build()
        cameraSource = CameraSource.Builder(requireActivity(), detector)
            .setAutoFocusEnabled(true)
            .build()
        camera_surface_view.holder.addCallback(surfaceCallback)
        detector.setProcessor(processor)
    }

    private fun setupSounds() {
        successNotification = MediaPlayer.create(requireActivity(), R.raw.success_notification)
        failureNotification = MediaPlayer.create(requireActivity(), R.raw.failure_notification)
        unverifiedNotification = MediaPlayer.create(requireActivity(), R.raw.unverified_notification)
        infectedNotification = MediaPlayer.create(requireActivity(), R.raw.infected_notification)
    }

    private val surfaceCallback = object : SurfaceHolder.Callback {
        override fun surfaceChanged(p0: SurfaceHolder, p1: Int, p2: Int, p3: Int) {}

        override fun surfaceDestroyed(p0: SurfaceHolder) {
            cameraSource.stop()
        }

        override fun surfaceCreated(surfaceHolder: SurfaceHolder) {

            savedSurfaceHolder = surfaceHolder

            try {
                if (ActivityCompat.checkSelfPermission(requireActivity(), Manifest.permission.CAMERA)
                    == PackageManager.PERMISSION_GRANTED) { // Permission Check is not necessary but Android requests it, it won't slow down scanning, only the building on this view slightly, the if should always be true
                    cameraSource.start(surfaceHolder)
                }
            } catch (e: IOException) {
                viewLifecycleOwner.lifecycleScope.launch {
                    onFailure(AppErrorCodes.CAMERA_ERROR)
                }
            }
        }

    }

    private val processor = object : Detector.Processor<Barcode> {
        override fun release() {}

        override fun receiveDetections(detections: Detector.Detections<Barcode>?) {
            // This method runs inside a different thread,
            // so any UI tasks should be called using viewLifecycleOwner.lifecycleScope.launch

            if (!scanComplete && detections != null && detections.detectedItems.isNotEmpty()) {
                val qrCodes : SparseArray<Barcode> = detections.detectedItems

                if (qrCodes.size() == 1) { // Prevent Scanning Multiple Codes at one time

                    val code = qrCodes.valueAt(0)

                    try {
                        setScanComplete()
                        viewModel.visitInfo.visitorId = UUID.fromString(code.displayValue)
//                        Log.d("Scanned Value", code.displayValue)

                        // UI Task
                        viewLifecycleOwner.lifecycleScope.launch {
                            try {
                                onStarted()
                                withContext(Dispatchers.IO) { viewModel.logVisit() }
                                isSuccess = true
                            } catch (e: ApiException) {
                                isSuccess = false
                                val error = mapErrorStringToError(e.message!!)
                                processApiFailureType(error)
                            } catch (e: NoInternetException) {
                                isSuccess = false
                                val error = mapErrorStringToError(e.message!!)
                                onFailure(error)
                                viewModel.writeInternetIsNotAvailable()
                            } catch (e: AppException) {
                                isSuccess = false
                                val error = mapErrorStringToError(e.message!!)
                                onFailure(error)
                            }
                        }.invokeOnCompletion {
                            if (isSuccess) {
                                viewLifecycleOwner.lifecycleScope.launch { onSuccess() }
                            }
                        }

                    } catch (e: RuntimeException) {
//                        Log.e("Exception", e.message!!)
                        setScanComplete()

                        // UI Task
                        viewLifecycleOwner.lifecycleScope.launch {
                            onFailure(AppErrorCodes.INVALID_QR_CODE)
                        }

                    }
                } else { // Multiple QR Codes on Screen
                    setScanComplete()

                    // UI Task
                    viewLifecycleOwner.lifecycleScope.launch {
                        onFailure(AppErrorCodes.MULTIPLE_CODES_SCANNED)
                    }

                }
            }
        }

    }

    private fun navigate() {
        val action = ScannerFragmentDirections.actionScannerFragmentToSettingsFragment()
        this.findNavController().navigate(action)
    }

    private fun loadVisitSettings() {
        onStarted()
        viewLifecycleOwner.lifecycleScope.launch {
            viewModel.getSavedVisitSettingsDirectly().observe(viewLifecycleOwner, Observer {
                if (it?.organizationName != null && it.doorName != null && it.direction != null) {
                    viewModel.visitInfo.organization = it.organizationName
                    viewModel.visitInfo.door = it.doorName
                    viewModel.visitInfo.direction = it.direction
                    viewModel.visitInfo.scannerVersion = it.scannerVersion
                    onDataLoaded()
                    coroutineContext.cancel()
                }
            })
        }
    }

    private fun onStarted() {
        setScanComplete() // Disable Scanning
        disableUi()
    }

    private fun onDataLoaded() {
        clearScanComplete() // Re-Enable Scanning
        enableUi()
        removeError()
        removeWarning()
    }

    private fun processApiFailureType(error: Error) {
        when (error.code) {
            ApiErrorCodes.UNVERIFIED_VISITOR.code -> {
                onWarning(error)
                unverifiedNotification?.start()
            }
            ApiErrorCodes.INFECTED_VISITOR.code -> {
                onInfectedVisitor(error)
            }
            else -> {
                onFailure(error)
            }
        }
    }

    private fun onSuccess() {
        showSuccess()
        successNotification?.start()
        updateRecyclerView("Success", R.drawable.success_notification_bubble)
    }

    private fun onFailure(error: Error) {
        showFailure()
        setError(error)
//        Log.e("Error Message", "${error.code}: ${error.message}")
        failureNotification?.start()
        updateRecyclerView(error.message!!, R.drawable.error_notification_bubble)
    }

    private fun onWarning(error: Error) {
        showWarning()
        setWarning(error)
        updateRecyclerView(error.message!!, R.drawable.warning_notification_bubble)
//        Log.e("Warning Message", "${error.code}: ${error.message}")
    }

    private fun onInfectedVisitor(error: Error) {
        showInfectedVisitor()
        setError(error)
//        Log.e("Error Message", "${error.code}: ${error.message}")
        infectedNotification?.start()
        updateRecyclerView(error.message!!, R.drawable.error_notification_bubble)
    }

    // Used to indicate work happening
    private fun disableUi() {
        scanner_indicator_square.show()
        showProgressIndicator()
        removeError()
        removeWarning()
        scanner_error_indicator.hide()
        scanner_success_indicator.hide()
        scanner_warning_indicator.hide()
        scanner_infected_visitor_indicator.hide()
        settings_button.disable()
    }

    // Used to re-enable UI after work is complete
    private fun enableUi() {
        scanner_indicator_square.hide()
        hideProgressIndicator()
        removeError()
        removeWarning()
        scanner_error_indicator.hide()
        scanner_success_indicator.hide()
        scanner_warning_indicator.hide()
        scanner_infected_visitor_indicator.hide()
        settings_button.enable()
    }

    private fun showFailure() {
        scanner_indicator_square.show()
        hideProgressIndicator()
        scanner_error_indicator.show()
        scanner_success_indicator.hide()
        scanner_warning_indicator.hide()
        scanner_infected_visitor_indicator.hide()
        settings_button.disable()

        // Re-enable UI afterwards
        Handler(Looper.getMainLooper()).postDelayed({
            enableUi()
            clearScanComplete()
        }, FAILURE_NOTIFICATION_TIMEOUT)
    }

    private fun showSuccess() {
        setScanComplete()
        scanner_indicator_square.show()
        hideProgressIndicator()
        scanner_error_indicator.hide()
        scanner_success_indicator.show()
        scanner_warning_indicator.hide()
        scanner_infected_visitor_indicator.hide()
        settings_button.disable()

        // Re-enable UI afterwards
        Handler(Looper.getMainLooper()).postDelayed({
            enableUi()
            clearScanComplete()
            viewModel.recentScanCode = null
        }, SUCCESS_NOTIFICATION_TIMEOUT)
    }

    private fun showWarning() {
        scanner_indicator_square.show()
        hideProgressIndicator()
        scanner_error_indicator.hide()
        scanner_success_indicator.hide()
        scanner_warning_indicator.show()
        scanner_infected_visitor_indicator.hide()
        settings_button.disable()

        // Re-enable UI afterwards
        Handler(Looper.getMainLooper()).postDelayed({
            enableUi()
            clearScanComplete()
            viewModel.recentScanCode = null
        }, WARNING_NOTIFICATION_TIMEOUT)
    }

    private fun showInfectedVisitor() {
        scanner_indicator_square.show()
        hideProgressIndicator()
        scanner_error_indicator.hide()
        scanner_success_indicator.hide()
        scanner_warning_indicator.hide()
        scanner_infected_visitor_indicator.show()
        settings_button.disable()

        // Re-enable UI afterwards
        Handler(Looper.getMainLooper()).postDelayed({
            enableUi()
            clearScanComplete()
            viewModel.recentScanCode = null
        }, INFECTED_VISITOR_NOTIFICATION_TIMEOUT)
    }

    private fun setError(error: Error) {
        var showErrorMessage = false

        var errorMessageText: String? = null

        // TODO: Keep only the error codes relevant to this fragment
        when (error.code) {
            AppErrorCodes.NULL_LOGIN_RESPONSE.code -> {
                showErrorMessage = true
                errorMessageText = AppErrorCodes.NULL_LOGIN_RESPONSE.message
            }
            AppErrorCodes.NULL_AUTHENTICATION_RESPONSE.code -> {
                showErrorMessage = true
                errorMessageText = AppErrorCodes.NULL_AUTHENTICATION_RESPONSE.message
            }
            AppErrorCodes.NULL_ORGANIZATION_DOORS_RESPONSE.code -> {
                showErrorMessage = true
                errorMessageText = AppErrorCodes.NULL_ORGANIZATION_DOORS_RESPONSE.message
            }
            AppErrorCodes.NO_INTERNET.code -> {
                showErrorMessage = true
                errorMessageText = AppErrorCodes.NO_INTERNET.message
            }
            AppErrorCodes.CAMERA_ERROR.code -> {
                showErrorMessage = true
                errorMessageText = AppErrorCodes.CAMERA_ERROR.message
            }
            AppErrorCodes.INVALID_QR_CODE.code -> {
                showErrorMessage = true
                errorMessageText = AppErrorCodes.INVALID_QR_CODE.message
            }
            AppErrorCodes.MULTIPLE_CODES_SCANNED.code -> {
                showErrorMessage = true
                errorMessageText = AppErrorCodes.MULTIPLE_CODES_SCANNED.message
            }
            ApiErrorCodes.UNAUTHORIZED.code -> {
                showErrorMessage = true
                errorMessageText = ApiErrorCodes.UNAUTHORIZED.message
            }
            ApiErrorCodes.VISITOR_NOT_FOUND_IN_SQL_DATABASE.code -> {
                showErrorMessage = true
                errorMessageText = ApiErrorCodes.VISITOR_NOT_FOUND_IN_SQL_DATABASE.message
            }
            ApiErrorCodes.INFECTED_VISITOR.code -> {
                showErrorMessage = true
                errorMessageText = ApiErrorCodes.INFECTED_VISITOR.message
            }
            ApiErrorCodes.GENERAL_ERROR.code -> {
                showErrorMessage = true
                errorMessageText = ApiErrorCodes.GENERAL_ERROR.message
            }
            else -> {
                // This state means the error is unaccounted for
                showErrorMessage = false
//                Log.e("Unaccounted Error", "${error.code}: ${error.message}")
            }
        }

        if (showErrorMessage && errorMessageText != null) {
            scanner_error_message.show()
            scanner_error_message.text = errorMessageText
        }
    }

    private fun removeError() {
        scanner_error_message.hide()
    }

    private fun setWarning(error: Error) {
        var showWarningMessage = false

        var warningMessageText: String? = null

        // TODO: Keep only the error codes relevant to this fragment
        // TODO: Add UI element to show warning messages
        when (error.code) {
            ApiErrorCodes.UNVERIFIED_VISITOR.code -> {
                showWarningMessage = true
                warningMessageText = ApiErrorCodes.UNVERIFIED_VISITOR.message
            }
        }

        if (showWarningMessage && warningMessageText != null) {
            scanner_warning_message.show()
            scanner_warning_message.text = warningMessageText
        }
    }

    private fun removeWarning() {
        scanner_warning_message.hide()
    }

    private fun showProgressIndicator() {
        scanner_progress_indicator_container.show()
        scanner_progress_indicator.show()
    }

    private fun hideProgressIndicator() {
        scanner_progress_indicator_container.hide()
        scanner_progress_indicator.hide()
    }

    private fun setScanComplete() {
        scanComplete = true
    }

    private fun clearScanComplete() {
        scanComplete = false
    }

    private fun handleScanHistoryDrawer() {

        if (scan_history_body.visibility == View.GONE) {
            scan_history_body.visibility = View.VISIBLE
            scan_history_button_icon.setBackgroundResource(R.drawable.ic_collapse_indicator)
        } else {
            scan_history_body.visibility = View.GONE
            scan_history_button_icon.setBackgroundResource(R.drawable.ic_expand_indicator)
        }

    }

}