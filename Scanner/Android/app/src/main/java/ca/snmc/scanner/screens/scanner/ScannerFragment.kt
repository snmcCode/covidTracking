package ca.snmc.scanner.screens.scanner

import android.Manifest
import android.content.pm.PackageManager
import android.os.Bundle
import android.os.Handler
import android.os.Looper
import android.util.Log
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
import ca.snmc.scanner.MainActivity
import ca.snmc.scanner.databinding.ScannerFragmentBinding
import ca.snmc.scanner.models.Error
import ca.snmc.scanner.utils.*
import com.google.android.gms.vision.CameraSource
import com.google.android.gms.vision.Detector
import com.google.android.gms.vision.barcode.Barcode
import com.google.android.gms.vision.barcode.BarcodeDetector
import kotlinx.android.synthetic.main.scanner_fragment.*
import kotlinx.coroutines.cancel
import kotlinx.coroutines.launch
import org.kodein.di.KodeinAware
import org.kodein.di.android.x.kodein
import org.kodein.di.generic.instance
import java.io.IOException
import java.util.*

private const val NOTIFICATION_TIMEOUT = 3000.toLong()
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

        loadViewModelData()
        setupScanner()

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

                if (qrCodes.size() > 1) {
                    // Prevent Scanning Multiple Codes at one time

                    // UI Task
                    viewLifecycleOwner.lifecycleScope.launch {
                        onFailure(AppErrorCodes.INVALID_VISITOR_ID)
                    }
                }

                val code = qrCodes.valueAt(0)

                try {
                    setScanComplete()
                    viewModel.visit.visitorId = UUID.fromString(code.displayValue)
                    Log.d("Scanned Value", code.displayValue)

                    // UI Task
                    viewLifecycleOwner.lifecycleScope.launch {
                        // TODO: Add API Call here, follow similar flow from LoginFragment
                        onSuccess()
                    }

                } catch (e: RuntimeException) {
                    setScanComplete()

                    // UI Task
                    viewLifecycleOwner.lifecycleScope.launch {
                        onFailure(AppErrorCodes.INVALID_VISITOR_ID)
                    }

                }
            }
        }

    }

    private fun startCamera() {
        if (ActivityCompat.checkSelfPermission(
                requireActivity(),
                Manifest.permission.CAMERA
            ) == PackageManager.PERMISSION_GRANTED
        ) {
            cameraSource.start(savedSurfaceHolder)
        }
    }

    private fun stopCamera() {
        cameraSource.stop()
    }

    private fun navigate() {
        val action = ScannerFragmentDirections.actionScannerFragmentToSettingsFragment()
        this.findNavController().navigate(action)
    }

    private fun loadViewModelData() {
        viewLifecycleOwner.lifecycleScope.launch {
            viewModel.getVisitInfo().observe(viewLifecycleOwner, Observer {
                if (it != null) {
                    if (it.doorName != null && it.direction != null) {
                        viewModel.visit.organization = it.organizationName
                        viewModel.visit.door = it.doorName
                        viewModel.visit.direction = it.direction
                        onDataLoaded()
                        coroutineContext.cancel()
                    } else {
                        onStarted()
                    }
                } else {
                    onStarted()
                }
            })
        }
    }

    private fun onStarted() {
        disableUi()
    }

    private fun onDataLoaded() {
        enableUi()
        removeError()
    }

    private fun onFailure(error: Error) {
        showFailure()
        setError(error)
        Log.e("Error Message", "${error.code}: ${error.message}")
        isSuccess = false
    }

    private fun onSuccess() {
        showSuccess()
    }

    private fun onWarning() {
        showWarning()
    }

    // Used to indicate work happening
    private fun disableUi() {
        scanner_indicator_square.show()
        showProgressIndicator()
        scanner_error_message.hide()
        scanner_error_indicator.hide()
        scanner_success_indicator.hide()
        scanner_warning_indicator.hide()
        settings_button.disable()
    }

    // Used to re-enable UI after work is complete
    private fun enableUi() {
        scanner_indicator_square.hide()
        hideProgressIndicator()
        scanner_error_message.hide()
        scanner_error_indicator.hide()
        scanner_success_indicator.hide()
        scanner_warning_indicator.hide()
        settings_button.enable()
    }

    private fun showFailure() {
        scanner_indicator_square.show()
        hideProgressIndicator()
        scanner_error_indicator.show()
        scanner_success_indicator.hide()
        scanner_warning_indicator.hide()
        settings_button.enable()

        // Re-enable UI afterwards
        Handler(Looper.getMainLooper()).postDelayed({
            enableUi()
            clearScanComplete()
        }, NOTIFICATION_TIMEOUT)
    }

    private fun showSuccess() {
        scanner_indicator_square.show()
        hideProgressIndicator()
        scanner_error_indicator.hide()
        scanner_success_indicator.show()
        scanner_warning_indicator.hide()
        settings_button.enable()

        // Re-enable UI afterwards
        Handler(Looper.getMainLooper()).postDelayed({
            enableUi()
            clearScanComplete()
        }, NOTIFICATION_TIMEOUT)
    }

    private fun showWarning() {
        scanner_indicator_square.show()
        hideProgressIndicator()
        scanner_error_indicator.hide()
        scanner_success_indicator.hide()
        scanner_warning_indicator.show()
        settings_button.enable()

        // Re-enable UI afterwards
        Handler(Looper.getMainLooper()).postDelayed({
            enableUi()
            clearScanComplete()
        }, NOTIFICATION_TIMEOUT)
    }

    private fun setError(error: Error) {
        var showErrorMessage = false

        var errorMessageText: String? = null

        // TODO: Keep only the error codes relevant to this fragment
        // TODO: Add UI element to show error messages
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
            AppErrorCodes.INVALID_VISITOR_ID.code -> {
                showErrorMessage = true
                errorMessageText = AppErrorCodes.INVALID_VISITOR_ID.message
            }
            AppErrorCodes.MULTIPLE_CODES_SCANNED.code -> {
                showErrorMessage = true
                errorMessageText = AppErrorCodes.MULTIPLE_CODES_SCANNED.message
            }
            ApiErrorCodes.UNAUTHORIZED.code -> {
                showErrorMessage = true
                errorMessageText = ApiErrorCodes.UNAUTHORIZED.message
            }
            ApiErrorCodes.NOT_FOUND_IN_SQL_DATABASE.code -> {
                showErrorMessage = true
                errorMessageText = ApiErrorCodes.NOT_FOUND_IN_SQL_DATABASE.message
            }
            ApiErrorCodes.GENERAL_ERROR.code -> {
                showErrorMessage = true
                errorMessageText = ApiErrorCodes.GENERAL_ERROR.message
            }
            else -> {
                // This state means the error is unaccounted for
                showErrorMessage = false
                Log.e("Error Message", "${error.code}: ${error.message}")
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

}