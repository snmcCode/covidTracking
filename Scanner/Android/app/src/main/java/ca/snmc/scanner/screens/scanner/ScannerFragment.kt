package ca.snmc.scanner.screens.scanner

import android.Manifest
import android.content.pm.PackageManager
import android.media.MediaPlayer
import android.os.Bundle
import android.os.Handler
import android.os.Looper
import android.util.SparseArray
import android.view.LayoutInflater
import android.view.SurfaceHolder
import android.view.View
import android.view.ViewGroup
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
import ca.snmc.scanner.utils.observers.LifecycleBoundLocationManager
import com.google.android.gms.location.FusedLocationProviderClient
import com.google.android.gms.location.LocationCallback
import com.google.android.gms.location.LocationResult
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

private const val SUCCESS_NOTIFICATION_TIMEOUT = 1000.toLong()
private const val OFFLINE_SUCCESS_NOTIFICATION_TIMEOUT = 2000.toLong()
private const val VISIT_LOG_UPLOAD_TIMEOUT_NOTIFICATION_TIMEOUT = 7000.toLong()
private const val FAILURE_NOTIFICATION_TIMEOUT = 4000.toLong()
private const val WARNING_NOTIFICATION_TIMEOUT = 4000.toLong()
private const val INFECTED_VISITOR_NOTIFICATION_TIMEOUT = 4000.toLong()
private const val STARTUP_FAILURE_NOTIFICATION_TIMEOUT = 7000.toLong()
private const val SCAN_HISTORY_MAX_SIZE = 10
class ScannerFragment : Fragment(), KodeinAware {

    override val kodein by kodein()
    private val scannerViewModelFactory : ScannerViewModelFactory by instance()

    private val fusedLocationProviderClient: FusedLocationProviderClient by instance()

    // Needed for FusedLocationProviderClient to work properly
    private val locationCallback = object : LocationCallback() {
        override fun onLocationResult(p0: LocationResult?) {
            super.onLocationResult(p0)
        }
    }

    private lateinit var binding : ScannerFragmentBinding
    private lateinit var viewModel: ScannerViewModel

    private var isSuccess = true

    private lateinit var cameraSource: CameraSource
    private lateinit var detector: BarcodeDetector

    private lateinit var savedSurfaceHolder: SurfaceHolder

    // Used to prevent duplication
    private var scanComplete: Boolean = false

    // Used to indicate that upload saved logs is done
    private var isUploadingSavedVisitLogsComplete: Boolean = false

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

        bindLocationManager()

        // Return the View at the Root of the Binding object
        return binding.root
    }

    override fun onViewCreated(view: View, savedInstanceState: Bundle?) {
        super.onViewCreated(view, savedInstanceState)


        viewLifecycleOwner.lifecycleScope.launch {
            initRecyclerView()
            loadData()
            loadVisitSettings()
            setupScanner()
            setupSounds()
            loadSavedDeviceSettings()
        }

    }

    override fun onResume() {
        super.onResume()

        (activity as MainActivity).fullscreenMode()
    }

    private fun bindLocationManager() {
        LifecycleBoundLocationManager(
            this,
            fusedLocationProviderClient,
            locationCallback
        )
    }

    private suspend fun initRecyclerView() {

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

    private fun updateRecyclerView(text: String?, backgroundResource: Int) {

        if (text != null) {
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

    }

    private suspend fun loadData() {
        viewLifecycleOwner.lifecycleScope.launch {
            onStarted()
            viewModel.getMergedData().observe(viewLifecycleOwner, Observer {
                if (it?.authorization != null && it.username != null && it.password != null) {
                    viewLifecycleOwner.lifecycleScope.launch {
                        loadVisitSettings()
                        manageSavedVisitLogs()
                    }
                    onDataLoaded()
                    coroutineContext.cancel()
                } else {
                    onStarted()
                }
            })
        }
    }

    private suspend fun setupScanner() {
        detector = BarcodeDetector
            .Builder(requireActivity())
            .setBarcodeFormats(Barcode.QR_CODE).build()
        cameraSource = CameraSource.Builder(requireActivity(), detector)
            .setAutoFocusEnabled(true)
            .build()
        camera_surface_view.holder.addCallback(surfaceCallback)
        detector.setProcessor(processor)
    }

    private suspend fun setupSounds() {
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
                                viewModel.writeInternetIsAvailable()
                            } catch (e: ApiException) {
                                isSuccess = false
                                val error = mapErrorStringToError(e.message!!)
                                processApiFailureType(error)
                            } catch (e: NoInternetException) {
                                isSuccess = false
                                withContext(Dispatchers.IO) { viewModel.logVisitLocal() }
                                onOfflineSuccess()
                                viewModel.writeInternetIsNotAvailable()
                            } catch (e: ConnectionTimeoutException) {
                                isSuccess = false
                                withContext(Dispatchers.IO) { viewModel.logVisitLocal() }
                                onOfflineSuccess()
                                viewModel.writeInternetIsNotAvailable()
                            } catch (e: LocationPermissionNotGrantedException) {
                                isSuccess = false
                                val error = mapErrorStringToError(e.message!!)
                                onFailure(error)
                            } catch (e: LocationServicesDisabledException) {
                                isSuccess = false
                                val error = mapErrorStringToError(e.message!!)
                                onFailure(error)
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

    private suspend fun loadVisitSettings() {
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

    private suspend fun loadSavedDeviceSettings() {
        onStarted()
        viewLifecycleOwner.lifecycleScope.launch {
            try {
                viewModel.getDeviceInformationOnStartupAndSet()
                onDataLoaded()
            } catch (e: LocationPermissionNotGrantedException) {
                isSuccess = false
                val error = mapErrorStringToError(e.message!!)
                onStartupFailure(error)
            } catch (e: LocationServicesDisabledException) {
                isSuccess = false
                val error = mapErrorStringToError(e.message!!)
                onStartupFailure(error)
            }
        }
    }

    private suspend fun manageSavedVisitLogs() {
        onManageSavedVisitLogsStarted()
        viewLifecycleOwner.lifecycleScope.launch {
            viewModel.getVisitLogUploadProgressBarProgressObservable().observe(viewLifecycleOwner, Observer {
                if (it.progress != scanner_progress_indicator_determinate.progress) {
                    scanner_progress_indicator_determinate.progress = it.progress
                    scanner_progress_indicator_determinate_message_percentage.text = getString(R.string.scanner_progress_indicator_determinate_message_percentage_value, it.progress)
                }
                // This is the first field to change
                if (it.totalItems != 0) {
                    scanner_progress_indicator_determinate_message_items_remaining.text = getString(R.string.scanner_progress_indicator_determinate_message_items_remaining_value, 0, it.totalItems)
                }
                if (it.uploadedItems != 0) {
                    scanner_progress_indicator_determinate_message_items_remaining.text = getString(R.string.scanner_progress_indicator_determinate_message_items_remaining_value, it.uploadedItems, it.totalItems)
                }
                if (it.progress == 100) {
                    onManageSavedVisitLogsFinishedSuccessfully()
                    if (it.timeout) {
                        onVisitLogUploadTimeout()
                    }
                    viewModel.resetVisitLogUploadProgressIndicatorObservable()
                }
            })
            try {
                withContext(Dispatchers.IO) { viewModel.uploadVisitLogs() }
                viewModel.resetVisitLogUploadProgressIndicatorObservable()
            } catch (e: Exception) {
//                Log.e("Exception", "Exception Occurred", e)
                onManageSavedVisitLogsFinishedSuccessfully()
                viewModel.resetVisitLogUploadProgressIndicatorObservable()
            }
        }
    }

    private fun onManageSavedVisitLogsStarted() {
        setScanComplete() // Disable Scanning
        disableUiForVisitLogUpload()
    }

    private fun onManageSavedVisitLogsFinishedSuccessfully() {
        // We assume that upload is complete (even if it failed) when this function is called. This will allow the UI to function again
        isUploadingSavedVisitLogsComplete = true
        clearScanComplete() // Re-Enable Scanning
        enableUiAfterVisitLogUpload()
    }

    private fun onManageSavedVisitLogsFinishedUnsuccessfully() {
        // We assume that upload is complete (even if it failed) when this function is called. This will allow the UI to function again
        isUploadingSavedVisitLogsComplete = true
        clearScanComplete() // Re-Enable Scanning
        enableUiAfterVisitLogUpload()
    }

    private fun onStarted() {
        setScanComplete() // Disable Scanning
        disableUi()
    }

    private fun onDataLoaded() {
        if (isUploadingSavedVisitLogsComplete) {
            clearScanComplete() // Re-Enable Scanning
            enableUi()
        } else {
            enableUiDuringVisitLogUpload()
        }
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

    private fun onStartupFailure(error: Error) {
        var message = "Critical Failure. Please Restart App"
        when (error.code) {
            AppErrorCodes.LOCATION_SERVICES_DISABLED.code -> {
                message = AppErrorCodes.LOCATION_SERVICES_DISABLED.message!!
            }
            AppErrorCodes.PERMISSIONS_NOT_GRANTED.code -> {
                message = AppErrorCodes.PERMISSIONS_NOT_GRANTED.message!! + ": LOCATION"
            }
        }
        scanner_critical_error_message.text = message
        scanner_critical_error_message.show()
        settings_button.disable()
        failureNotification?.start()

        updateRecyclerView(getErrorMessage(error.code!!), R.drawable.error_notification_bubble)

        // Re-enable UI afterwards
        Handler(Looper.getMainLooper()).postDelayed({
            settings_button.enable()
            scanner_critical_error_message.hide()
        }, STARTUP_FAILURE_NOTIFICATION_TIMEOUT)
    }

    private fun onVisitLogUploadTimeout() {
        scanner_visit_log_upload_timeout_message.show()
        settings_button.disable()
        updateRecyclerView(getString(R.string.visit_log_upload_timeout_message), R.drawable.visit_log_upload_timeout_notification_bubble)

        // Re-enable UI afterwards
        Handler(Looper.getMainLooper()).postDelayed({
            settings_button.enable()
            scanner_visit_log_upload_timeout_message.hide()
        }, VISIT_LOG_UPLOAD_TIMEOUT_NOTIFICATION_TIMEOUT)
    }

    private fun onSuccess() {
        showSuccess()
        successNotification?.start()
        updateRecyclerView("Success", R.drawable.success_notification_bubble)
    }

    private fun onOfflineSuccess() {
        setOfflineSuccess()
        showOfflineSuccess()
        successNotification?.start()
        updateRecyclerView(getString(R.string.scan_recorded_offline_message), R.drawable.offline_success_notification_bubble)
    }

    private fun onFailure(error: Error) {
        showFailure()
        setError(error)
//        Log.e("Error Message", "${error.code}: ${error.message}")
        failureNotification?.start()
        updateRecyclerView(getErrorMessage(error.code!!), R.drawable.error_notification_bubble)
    }

    private fun onWarning(error: Error) {
        showWarning()
        setWarning(error)
        updateRecyclerView(getErrorMessage(error.code!!), R.drawable.warning_notification_bubble)
//        Log.e("Warning Message", "${error.code}: ${error.message}")
    }

    private fun onInfectedVisitor(error: Error) {
        showInfectedVisitor()
        setError(error)
//        Log.e("Error Message", "${error.code}: ${error.message}")
        infectedNotification?.start()
        updateRecyclerView(getErrorMessage(error.code!!), R.drawable.error_notification_bubble)
    }

    // Used to indicate work happening
    private fun disableUiForVisitLogUpload() {
        scanner_indicator_square_critical.show()
        showDeterminateProgressIndicator()
        removeOfflineSuccess()
        removeError()
        removeWarning()
        scanner_error_indicator.hide()
        scanner_success_indicator.hide()
        scanner_offline_success_indicator.hide()
        scanner_warning_indicator.hide()
        scanner_infected_visitor_indicator.hide()
        settings_button.disable()
    }

    // Used to indicate work happening
    private fun disableUi() {
        scanner_indicator_square.show()
        showProgressIndicator()
        removeOfflineSuccess()
        removeError()
        removeWarning()
        scanner_error_indicator.hide()
        scanner_success_indicator.hide()
        scanner_offline_success_indicator.hide()
        scanner_warning_indicator.hide()
        scanner_infected_visitor_indicator.hide()
        settings_button.disable()
    }

    // Used to indicate work happening
    private fun enableUiAfterVisitLogUpload() {
        scanner_indicator_square_critical.hide()
        hideDeterminateProgressIndicator()
        removeOfflineSuccess()
        removeError()
        removeWarning()
        scanner_error_indicator.hide()
        scanner_success_indicator.hide()
        scanner_offline_success_indicator.hide()
        scanner_warning_indicator.hide()
        scanner_infected_visitor_indicator.hide()
        settings_button.enable()
    }

    // Used to re-enable UI after work is complete
    private fun enableUi() {
        scanner_indicator_square.hide()
        hideProgressIndicator()
        removeOfflineSuccess()
        removeError()
        removeWarning()
        scanner_error_indicator.hide()
        scanner_success_indicator.hide()
        scanner_offline_success_indicator.hide()
        scanner_warning_indicator.hide()
        scanner_infected_visitor_indicator.hide()
        settings_button.enable()
    }

    // Used to re-enable UI after work is complete
    private fun enableUiDuringVisitLogUpload() {
        scanner_indicator_square.hide()
        hideProgressIndicator()
        removeOfflineSuccess()
        removeError()
        removeWarning()
        scanner_error_indicator.hide()
        scanner_success_indicator.hide()
        scanner_offline_success_indicator.hide()
        scanner_warning_indicator.hide()
        scanner_infected_visitor_indicator.hide()
        settings_button.disable()
    }

    private fun showFailure() {
        scanner_indicator_square.show()
        hideProgressIndicator()
        scanner_error_indicator.show()
        scanner_success_indicator.hide()
        scanner_offline_success_indicator.hide()
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
        scanner_offline_success_indicator.hide()
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

    private fun showOfflineSuccess() {
        setScanComplete()
        scanner_indicator_square.show()
        hideProgressIndicator()
        scanner_error_indicator.hide()
        scanner_success_indicator.hide()
        scanner_offline_success_indicator.show()
        scanner_warning_indicator.hide()
        scanner_infected_visitor_indicator.hide()
        settings_button.disable()

        // Re-enable UI afterwards
        Handler(Looper.getMainLooper()).postDelayed({
            enableUi()
            clearScanComplete()
            viewModel.recentScanCode = null
        }, OFFLINE_SUCCESS_NOTIFICATION_TIMEOUT)
    }

    private fun showWarning() {
        scanner_indicator_square.show()
        hideProgressIndicator()
        scanner_error_indicator.hide()
        scanner_success_indicator.hide()
        scanner_offline_success_indicator.hide()
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
        scanner_offline_success_indicator.hide()
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

        when (error.code) {
            AppErrorCodes.NULL_LOGIN_RESPONSE.code -> {
                showErrorMessage = true
                errorMessageText = AppErrorCodes.NULL_LOGIN_RESPONSE.message
            }
            AppErrorCodes.NULL_AUTHENTICATION_RESPONSE.code -> {
                showErrorMessage = true
                errorMessageText = AppErrorCodes.NULL_AUTHENTICATION_RESPONSE.message
            }
            AppErrorCodes.NO_INTERNET.code -> {
                showErrorMessage = true
                errorMessageText = AppErrorCodes.NO_INTERNET.message
            }
            AppErrorCodes.CONNECTION_TIMEOUT.code -> {
                showErrorMessage = true
                errorMessageText = AppErrorCodes.CONNECTION_TIMEOUT.message
            }
            AppErrorCodes.LOCATION_SERVICES_DISABLED.code -> {
                showErrorMessage = true
                errorMessageText = AppErrorCodes.LOCATION_SERVICES_DISABLED.message
            }
            AppErrorCodes.PERMISSIONS_NOT_GRANTED.code -> {
                showErrorMessage = true
                errorMessageText = AppErrorCodes.PERMISSIONS_NOT_GRANTED.message + ": LOCATION"
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

    private fun setOfflineSuccess() {
        scanner_offline_success_message.show()
    }

    private fun removeOfflineSuccess() {
        scanner_offline_success_message.hide()
    }

    private fun showProgressIndicator() {
        scanner_progress_indicator_container.show()
        scanner_progress_indicator.show()
    }

    private fun hideProgressIndicator() {
        scanner_progress_indicator_container.hide()
        scanner_progress_indicator.hide()
    }

    private fun showDeterminateProgressIndicator() {
        scanner_determinate_progress_indicator_container.show()
        scanner_progress_indicator_determinate.show()
        scanner_progress_indicator_determinate_number_container.show()
        scanner_progress_indicator_determinate_message.show()
    }

    private fun hideDeterminateProgressIndicator() {
        scanner_determinate_progress_indicator_container.hide()
        scanner_progress_indicator_determinate.hide()
        scanner_progress_indicator_determinate_number_container.hide()
        scanner_progress_indicator_determinate_message.hide()
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

    private fun getErrorMessage(code: Int) : String? {

        // TODO: Find a better way to do this
        when (code) {
            AppErrorCodes.NULL_LOGIN_RESPONSE.code -> {
                return AppErrorCodes.NULL_LOGIN_RESPONSE.message!!
            }
            AppErrorCodes.NULL_AUTHENTICATION_RESPONSE.code -> {
                return AppErrorCodes.NULL_AUTHENTICATION_RESPONSE.message!!
            }
            AppErrorCodes.NO_INTERNET.code -> {
                return AppErrorCodes.NO_INTERNET.message!!
            }
            AppErrorCodes.CONNECTION_TIMEOUT.code -> {
                return AppErrorCodes.CONNECTION_TIMEOUT.message!!
            }
            AppErrorCodes.CAMERA_ERROR.code -> {
                return AppErrorCodes.CAMERA_ERROR.message!!
            }
            AppErrorCodes.INVALID_QR_CODE.code -> {
                return AppErrorCodes.INVALID_QR_CODE.message!!
            }
            AppErrorCodes.MULTIPLE_CODES_SCANNED.code -> {
                return AppErrorCodes.MULTIPLE_CODES_SCANNED.message!!
            }
            ApiErrorCodes.UNAUTHORIZED.code -> {
                return ApiErrorCodes.UNAUTHORIZED.message!!
            }
            ApiErrorCodes.UNVERIFIED_VISITOR.code -> {
                return ApiErrorCodes.UNVERIFIED_VISITOR.message!!
            }
            ApiErrorCodes.VISITOR_NOT_FOUND_IN_SQL_DATABASE.code -> {
                return ApiErrorCodes.VISITOR_NOT_FOUND_IN_SQL_DATABASE.message!!
            }
            ApiErrorCodes.INFECTED_VISITOR.code -> {
                return ApiErrorCodes.INFECTED_VISITOR.message!!
            }
            ApiErrorCodes.GENERAL_ERROR.code -> {
                return ApiErrorCodes.GENERAL_ERROR.message!!
            }
        }

        return null

    }

}