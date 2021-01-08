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

private const val SUCCESS_NOTIFICATION_TIMEOUT = 2000.toLong()
private const val OFFLINE_SUCCESS_NOTIFICATION_TIMEOUT = 2000.toLong()
private const val VISIT_LOG_UPLOAD_TIMEOUT_NOTIFICATION_TIMEOUT = 7000.toLong()
private const val FAILURE_NOTIFICATION_TIMEOUT = 4000.toLong()
private const val WARNING_NOTIFICATION_TIMEOUT = 4000.toLong()
private const val INFECTED_VISITOR_NOTIFICATION_TIMEOUT = 4000.toLong()
private const val NOT_BOOKED_NOTIFICATION_TIMEOUT = 5000.toLong()
private const val CAPACITY_REACHED_NOTIFICATION_TIMEOUT = 5000.toLong()
private const val STARTUP_FAILURE_NOTIFICATION_TIMEOUT = 7000.toLong()
private const val SCAN_RESULT_HISTORY_MAX_SIZE = 10
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
    private var duplicateScanNotification : MediaPlayer? = null
    private var notBookedNotification : MediaPlayer? = null
    private var capacityReachedNotification : MediaPlayer? = null

    private var isDataAlreadyLoaded : Boolean = false

    private lateinit var scanHistoryAdapter : ScanHistoryRecyclerViewAdapter

    private val handler : Handler = Handler(Looper.getMainLooper())

    override fun onCreateView(
        inflater: LayoutInflater, container: ViewGroup?,
        savedInstanceState: Bundle?
    ): View? {
        (activity as MainActivity).fullscreenMode()

        binding = ScannerFragmentBinding.inflate(inflater, container, false)

        // Set LifecycleOwner on Binding object
        binding.lifecycleOwner = this

        binding.settingsButton.setOnClickListener {
            setScanComplete() // Disable Scanning
            disableUi() // Disable UI
            navigate()
        }

        binding.scanHistoryButton.setOnClickListener {
            handleScanHistoryDrawer()
        }

        binding.scannerNotBookedOverrideButton.setOnClickListener {
            handleBookingOverride()
        }

        binding.scannerCapacityReachedOverrideButton.setOnClickListener {
            handleCapacityReachedOverride()
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
            setupScanner()
            setupSounds()
            loadSavedDeviceSettings()
            setupLogVisitSuccessHandler()
        }

    }

    override fun onResume() {
        super.onResume()

        (activity as MainActivity).fullscreenMode()
    }

    override fun onDestroy() {
        handler.removeCallbacksAndMessages(null)
        super.onDestroy()
    }

    private fun bindLocationManager() {
        LifecycleBoundLocationManager(
            this,
            fusedLocationProviderClient,
            locationCallback
        )
    }

    private fun initRecyclerView() {

        viewLifecycleOwner.lifecycleScope.launch {
            scanHistoryAdapter = ScanHistoryRecyclerViewAdapter()
            scan_history_recycler_view.apply {
                layoutManager = LinearLayoutManager(requireActivity())
                adapter = scanHistoryAdapter
            }

            // Observe the observable
            viewModel.scanResultHistoryObservable.observe(viewLifecycleOwner, Observer {
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
                if (viewModel.scanResultHistory.count() == SCAN_RESULT_HISTORY_MAX_SIZE) {
                    viewModel.scanResultHistory = viewModel.scanResultHistory.dropLast(1) as ArrayList<ScanHistoryItem>
                }

                // Add the latest item to the top
                viewModel.scanResultHistory.add(0, ScanHistoryItem(text, backgroundResource))

//                Log.e("Scan History", "\nStarting\n")
//                for (scanHistoryItem in viewModel.scanHistory) {
//                    Log.e("ScanHistoryItem", "${scanHistoryItem.visitInfo.visitorId}, ${scanHistoryItem.visitInfo.door}, ${scanHistoryItem.visitInfo.direction}")
//                }
//                Log.e("Scan History", "\nComplete\n")

                // Update the observable
                viewModel.scanResultHistoryObservable.postValue(viewModel.scanResultHistory)
            }
        }

    }

    private fun generateRecyclerViewMessageWithVisitorId(baseString: String) : String {
        return "${baseString}\n\nVisitor ID: ${viewModel.visitInfo.visitorId}"
    }

    private suspend fun loadData() {
        viewLifecycleOwner.lifecycleScope.launch {
//            Log.e("LoadData", "Calling OnStarted")
            onStarted()
            viewModel.getMergedData().observe(viewLifecycleOwner, Observer {
                if (!isDataAlreadyLoaded) {
                    // This if check solves the duplication bug, since this function would be called
                    // whenever authentication is updated

                    if (it?.authorization != null && it.username != null && it.password != null) {
                        isDataAlreadyLoaded = true
                        viewLifecycleOwner.lifecycleScope.launch {
                            loadVisitSettings()
                            manageSavedVisitLogs()
                        }

                        // Load Selected Event
                        viewModel.getSelectedEvent().observe(viewLifecycleOwner, Observer { selectedEventEntity ->
                            if (selectedEventEntity?.eventId != null) {
                                viewModel.visitInfo.eventId = selectedEventEntity.eventId
                            } else {
                                viewModel.visitInfo.eventId = null
                            }
                        })

                        onDataLoaded()
                        coroutineContext.cancel()
                    } else {
//                        Log.e("LoadData Else", "Calling OnStarted")
                        onStarted()
                    }

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
        duplicateScanNotification = MediaPlayer.create(requireActivity(), R.raw.duplicate_scan_notification)
        notBookedNotification = MediaPlayer.create(requireActivity(), R.raw.not_booked_notification)
        capacityReachedNotification = MediaPlayer.create(requireActivity(), R.raw.capacity_reached_notification)
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
                    logError(
                        exception = e,
                        functionName = "surfaceCreated",
                        errorMessage = AppErrorCodes.CAMERA_ERROR.message!!,
                        issue = "Error occurred when trying to open the camera."
                    )
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

                setScanComplete()

                if (qrCodes.size() == 1) { // Prevent Scanning Multiple Codes at one time

                    val code = qrCodes.valueAt(0)

                    try {
                        viewModel.visitInfo.visitorId = UUID.fromString(code.displayValue)
                        viewModel.visitInfo.anti_duplication_timestamp = System.currentTimeMillis()
//                        Log.d("Scanned Value", code.displayValue)

                        // TODO: Check capacity reached and throw and catch relevant exception

                        // UI Task
                        viewLifecycleOwner.lifecycleScope.launch {
                            try {
//                                Log.e("receiveDetections", "Calling OnStarted")
                                onStarted()

                                if (viewModel.visitInfo.eventId != null) {
                                    val isEventFull = withContext(Dispatchers.IO) {
                                        return@withContext viewModel.isEventFull()
                                    }
                                    if (isEventFull) {
                                        val errorMessage = "${AppErrorCodes.CAPACITY_REACHED.code}: ${AppErrorCodes.CAPACITY_REACHED.message}"
                                        throw CapacityReachedException(errorMessage)
                                    }
                                }

                                withContext(Dispatchers.IO) { viewModel.logVisit() }
                                viewModel.writeInternetIsAvailable()
                            } catch (e: ApiException) {
                                viewModel.isLogVisitApiCallSuccessful.postValue(false)
                                val error = mapErrorStringToError(e.message!!)
                                logError(
                                    exception = e,
                                    functionName = "receiveDetections",
                                    errorMessage = error.message!!,
                                    issue = "API returned error code during attempt to log visit."
                                )
                                processApiFailureType(error)
                            } catch (e: NoInternetException) {
                                viewModel.isLogVisitApiCallSuccessful.postValue(false)
                                logError(
                                    exception = e,
                                    functionName = "receiveDetections",
                                    errorMessage = e.message!!,
                                    issue = "No internet connection during attempt to log visit."
                                )
                                withContext(Dispatchers.IO) { viewModel.logVisitLocal() }
                                onOfflineSuccess()
                                viewModel.writeInternetIsNotAvailable()
                            } catch (e: ConnectionTimeoutException) {
                                viewModel.isLogVisitApiCallSuccessful.postValue(false)
                                logError(
                                    exception = e,
                                    functionName = "receiveDetections",
                                    errorMessage = e.message!!,
                                    issue = "Connection timed out or connection error occurred during attempt to log visit."
                                )
                                withContext(Dispatchers.IO) { viewModel.logVisitLocal() }
                                onOfflineSuccess()
                                viewModel.writeInternetIsNotAvailable()
                            } catch (e: LocationPermissionNotGrantedException) {
                                viewModel.isLogVisitApiCallSuccessful.postValue(false)
                                val error = mapErrorStringToError(e.message!!)
                                logError(
                                    exception = e,
                                    functionName = "receiveDetections",
                                    errorMessage = error.message!!,
                                    issue = "Location permission was not granted or was retracted so the log visit attempt failed."
                                )
                                onFailure(error)
                            } catch (e: LocationServicesDisabledException) {
                                viewModel.isLogVisitApiCallSuccessful.postValue(false)
                                val error = mapErrorStringToError(e.message!!)
                                logError(
                                    exception = e,
                                    functionName = "receiveDetections",
                                    errorMessage = error.message!!,
                                    issue = "Location services were disabled so the log visit attempt failed."
                                )
                                onFailure(error)
                            } catch (e: DuplicateScanException) {
                                viewModel.isLogVisitApiCallSuccessful.postValue(false)
                                val error = mapErrorStringToError(e.message!!)
                                onDuplicateScan(error)
                            } catch (e: AuthenticationException) {
                                viewModel.isLogVisitApiCallSuccessful.postValue(false)
                                val error = mapErrorStringToError(e.message!!)
                                logError(
                                    exception = e,
                                    functionName = "receiveDetections",
                                    errorMessage = error.message!!,
                                    issue = "Error occurred during authentication attempt."
                                )
                                onFailure(error)
                            } catch (e: CapacityReachedException) {
                                viewModel.isLogVisitApiCallSuccessful.postValue(false)
                                val error = mapErrorStringToError(e.message!!)
                                logError(
                                    exception = e,
                                    functionName = "receiveDetections",
                                    errorMessage = error.message!!,
                                    issue = "Event capacity reached."
                                )
                                onCapacityReached(error)
                            }
                        }
                    } catch (e: RuntimeException) {
//                        Log.e("Exception", e.message!!)
                        setScanComplete()

                        // UI Task
                        viewLifecycleOwner.lifecycleScope.launch {
                            logError(
                                exception = e,
                                functionName = "receiveDetections",
                                errorMessage = e.message!!,
                                issue = "An error occurred in the MobileVision API. It failed to scan the QR code successfully so the Log Visit attempt failed."
                            )
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
//        Log.e("LoadVisitSettings", "Calling OnStarted")
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
//        Log.e("LoadSavedDeviceSettings", "Calling OnStarted")
        onStarted()
        viewLifecycleOwner.lifecycleScope.launch {
            try {
                viewModel.getDeviceInformationOnStartupAndSet()
                onDataLoaded()
            } catch (e: LocationPermissionNotGrantedException) {
                viewModel.isLogVisitApiCallSuccessful.postValue(false)
                val error = mapErrorStringToError(e.message!!)
                logError(
                    exception = e,
                    functionName = "loadSavedDeviceSettings",
                    errorMessage = error.message!!,
                    issue = "Location permission was not granted or was retracted so the app was unable to get the device location."
                )
                onStartupFailure(error)
            } catch (e: LocationServicesDisabledException) {
                viewModel.isLogVisitApiCallSuccessful.postValue(false)
                val error = mapErrorStringToError(e.message!!)
                logError(
                    exception = e,
                    functionName = "loadSavedDeviceSettings",
                    errorMessage = error.message!!,
                    issue = "Location services were disabled so the app was unable to get the device location."
                )
                onStartupFailure(error)
            }
        }
    }

    private fun setupLogVisitSuccessHandler() {
        viewModel.isLogVisitApiCallSuccessful.observe(viewLifecycleOwner, Observer { isLogVisitApiCallSuccessful ->
            if (isLogVisitApiCallSuccessful) {
                onSuccess()
            }
        })
    }

    private suspend fun manageSavedVisitLogs() {
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

            viewModel.isLogVisitBulkApiCallRunning.observe(viewLifecycleOwner, Observer { isLogVisitBulkApiCallRunning ->
                if (isLogVisitBulkApiCallRunning) {
                    onManageSavedVisitLogsStarted()
                } else {
                    onManageSavedVisitLogsFinishedSuccessfully()
                }
            })

            try {
                withContext(Dispatchers.IO) { viewModel.uploadVisitLogs() }
                viewModel.resetVisitLogUploadProgressIndicatorObservable()
            } catch (e: Exception) {
//                Log.e("Exception", "Exception Occurred", e)
                logError(
                    exception = e,
                    functionName = "manageSavedVisitLogs",
                    errorMessage = e.message!!,
                    issue = "Unable to upload saved visit logs because an Exception occurred. Please see the error message for details."
                )
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

    private fun onStarted() {
//        Log.e("IsDataAlreadyLoaded", isDataAlreadyLoaded.toString())
        setScanComplete() // Disable Scanning
        disableUi()
    }

    private fun onDataLoaded() {
        if (isUploadingSavedVisitLogsComplete) {
            enableUi()
            clearScanComplete() // Re-Enable Scanning
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
            ApiErrorCodes.NOT_BOOKED.code -> {
                onNotBooked(error)
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
        handler.postDelayed({
            settings_button.enable()
            scanner_critical_error_message.hide()
        }, STARTUP_FAILURE_NOTIFICATION_TIMEOUT)
    }

    private fun onVisitLogUploadTimeout() {
        scanner_visit_log_upload_timeout_message.show()
        settings_button.disable()
        updateRecyclerView(getString(R.string.visit_log_upload_timeout_message), R.drawable.visit_log_upload_timeout_notification_bubble)

        // Re-enable UI afterwards
        handler.postDelayed({
            settings_button.enable()
            scanner_visit_log_upload_timeout_message.hide()
        }, VISIT_LOG_UPLOAD_TIMEOUT_NOTIFICATION_TIMEOUT)
    }

    private fun onSuccess() {
//        Log.e("On Success", "Called Now")
        // Update the list of successful scans
        viewModel.onSuccessfulScan()

        showSuccess()
        successNotification?.start()
        val recyclerViewMessageWithVisitorId: String = generateRecyclerViewMessageWithVisitorId("Success")
        updateRecyclerView(recyclerViewMessageWithVisitorId, R.drawable.success_notification_bubble)
    }

    private fun onOfflineSuccess() {
        // Update the list of successful scans
        viewModel.onSuccessfulScan()

        setOfflineSuccess()
        showOfflineSuccess()
        successNotification?.start()
        val recyclerViewMessageWithVisitorId: String = generateRecyclerViewMessageWithVisitorId(getString(R.string.scan_recorded_offline_message))
        updateRecyclerView(recyclerViewMessageWithVisitorId, R.drawable.offline_success_notification_bubble)
    }

    private fun onDuplicateScan(error: Error) {
        showFailure()
        setError(error)
//        Log.e("Error Message", "${error.code}: ${error.message}")
        duplicateScanNotification?.start()
        val recyclerViewMessageWithVisitorId: String = generateRecyclerViewMessageWithVisitorId(getErrorMessage(error.code!!)!!)
        updateRecyclerView(recyclerViewMessageWithVisitorId, R.drawable.error_notification_bubble)
    }


    private fun onFailure(error: Error) {
        showFailure()
        setError(error)
//        Log.e("Error Message", "${error.code}: ${error.message}")
        failureNotification?.start()
        val recyclerViewMessageWithVisitorId: String = generateRecyclerViewMessageWithVisitorId(getErrorMessage(error.code!!)!!)
        updateRecyclerView(recyclerViewMessageWithVisitorId, R.drawable.error_notification_bubble)
    }

    private fun onWarning(error: Error) {
        showWarning()
        setWarning(error)
        val recyclerViewMessageWithVisitorId: String = generateRecyclerViewMessageWithVisitorId(getErrorMessage(error.code!!)!!)
        updateRecyclerView(recyclerViewMessageWithVisitorId, R.drawable.warning_notification_bubble)
//        Log.e("Warning Message", "${error.code}: ${error.message}")
    }

    private fun onInfectedVisitor(error: Error) {
        showInfectedVisitor()
        setError(error)
//        Log.e("Error Message", "${error.code}: ${error.message}")
        infectedNotification?.start()
        val recyclerViewMessageWithVisitorId: String = generateRecyclerViewMessageWithVisitorId(getErrorMessage(error.code!!)!!)
        updateRecyclerView(recyclerViewMessageWithVisitorId, R.drawable.error_notification_bubble)
    }

    private fun onNotBooked(error: Error) {
        showNotBooked()
        setWarning(error)
        notBookedNotification?.start()
        val recyclerViewMessageWithVisitorId: String = generateRecyclerViewMessageWithVisitorId(getErrorMessage(error.code!!)!!)
        updateRecyclerView(recyclerViewMessageWithVisitorId, R.drawable.warning_notification_bubble)
    }

    private fun onCapacityReached(error: Error) {
        showCapacityReached()
        setError(error)
        capacityReachedNotification?.start()
        val recyclerViewMessageWithVisitorId: String = generateRecyclerViewMessageWithVisitorId(getErrorMessage(error.code!!)!!)
        updateRecyclerView(recyclerViewMessageWithVisitorId, R.drawable.error_notification_bubble)
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
        scanner_not_booked_indicator.hide()
        scanner_capacity_reached_indicator.hide()
        settings_button.disable()
    }

    // Used to indicate work happening
    private fun disableUi() {
//        Log.e("Disable", "Disable UI Called")
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
        scanner_not_booked_indicator.hide()
        scanner_capacity_reached_indicator.hide()
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
        scanner_not_booked_indicator.hide()
        scanner_capacity_reached_indicator.hide()
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
        scanner_not_booked_indicator.hide()
        scanner_capacity_reached_indicator.hide()
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
        scanner_not_booked_indicator.hide()
        scanner_capacity_reached_indicator.hide()
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
        scanner_not_booked_indicator.hide()
        scanner_capacity_reached_indicator.hide()
        settings_button.disable()

        // Re-enable UI afterwards
        handler.postDelayed({
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
        scanner_not_booked_indicator.hide()
        scanner_capacity_reached_indicator.hide()
        settings_button.disable()

        // Re-enable UI afterwards
        handler.postDelayed({
            enableUi()
            clearScanComplete()
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
        scanner_not_booked_indicator.hide()
        scanner_capacity_reached_indicator.hide()
        settings_button.disable()

        // Re-enable UI afterwards
        handler.postDelayed({
            enableUi()
            clearScanComplete()
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
        scanner_not_booked_indicator.hide()
        scanner_capacity_reached_indicator.hide()
        settings_button.disable()

        // Re-enable UI afterwards
        handler.postDelayed({
            enableUi()
            clearScanComplete()
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
        scanner_not_booked_indicator.hide()
        scanner_capacity_reached_indicator.hide()
        settings_button.disable()

        // Re-enable UI afterwards
        handler.postDelayed({
            enableUi()
            clearScanComplete()
        }, INFECTED_VISITOR_NOTIFICATION_TIMEOUT)
    }

    private fun showNotBooked() {
        scanner_indicator_square.show()
        hideProgressIndicator()
        scanner_error_indicator.hide()
        scanner_success_indicator.hide()
        scanner_offline_success_indicator.hide()
        scanner_warning_indicator.hide()
        scanner_infected_visitor_indicator.hide()
        scanner_not_booked_indicator.show()
        scanner_capacity_reached_indicator.hide()
        settings_button.disable()

        // Re-enable UI afterwards
        handler.postDelayed({
            enableUi()
            clearScanComplete()
        }, NOT_BOOKED_NOTIFICATION_TIMEOUT)
    }

    private fun showCapacityReached() {
        scanner_indicator_square.show()
        hideProgressIndicator()
        scanner_error_indicator.hide()
        scanner_success_indicator.hide()
        scanner_offline_success_indicator.hide()
        scanner_warning_indicator.hide()
        scanner_infected_visitor_indicator.hide()
        scanner_not_booked_indicator.hide()
        scanner_capacity_reached_indicator.show()
        settings_button.disable()

        // Re-enable UI afterwards
        handler.postDelayed({
            enableUi()
            clearScanComplete()
        }, CAPACITY_REACHED_NOTIFICATION_TIMEOUT)
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
            AppErrorCodes.DUPLICATE_SCAN.code -> {
                showErrorMessage = true
                errorMessageText = AppErrorCodes.DUPLICATE_SCAN.message
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
            AppErrorCodes.CAPACITY_REACHED.code -> {
                showErrorMessage = true
                errorMessageText = AppErrorCodes.CAPACITY_REACHED.message
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
            ApiErrorCodes.NOT_BOOKED.code -> {
                showWarningMessage = true
                warningMessageText = ApiErrorCodes.NOT_BOOKED.message
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

    private fun handleBookingOverride() { // Triggers logVisitOverride (used to override a non-booked visitor error) which doesn't throw a DuplicateScanException
        setScanComplete()

        // UI Task
        viewLifecycleOwner.lifecycleScope.launch {
            try {
//                Log.e("handleOverride", "Calling OnStarted")
                viewModel.visitInfo.bookingOverride = true
                val isEventFull = withContext(Dispatchers.IO) {
                    return@withContext viewModel.isEventFull()
                }
                if (isEventFull) {
                    viewModel.visitInfo.capacityOverride = true
                }
                onStarted()
                withContext(Dispatchers.IO) { viewModel.logVisitOverride() }
                viewModel.writeInternetIsAvailable()
                viewModel.visitInfo.bookingOverride = false
            } catch (e: ApiException) {
                viewModel.visitInfo.bookingOverride = false
                viewModel.visitInfo.capacityOverride = false
                viewModel.isLogVisitApiCallSuccessful.postValue(false)
                val error = mapErrorStringToError(e.message!!)
                logError(
                    exception = e,
                    functionName = "receiveDetections",
                    errorMessage = error.message!!,
                    issue = "API returned error code during attempt to log visit."
                )
                processApiFailureType(error)
            } catch (e: NoInternetException) {
                viewModel.isLogVisitApiCallSuccessful.postValue(false)
                logError(
                    exception = e,
                    functionName = "receiveDetections",
                    errorMessage = e.message!!,
                    issue = "No internet connection during attempt to log visit."
                )
                // Only Log Visit Locally if there is no selected event, otherwise, there is no local logging
                withContext(Dispatchers.IO) {
                    viewModel.logVisitLocal()
                    viewModel.visitInfo.capacityOverride = false
                }
                onOfflineSuccess()
                viewModel.writeInternetIsNotAvailable()
            } catch (e: ConnectionTimeoutException) {
                viewModel.isLogVisitApiCallSuccessful.postValue(false)
                logError(
                    exception = e,
                    functionName = "receiveDetections",
                    errorMessage = e.message!!,
                    issue = "Connection timed out or connection error occurred during attempt to log visit."
                )
                withContext(Dispatchers.IO) {
                    viewModel.logVisitLocal()
                    viewModel.visitInfo.capacityOverride = false
                }
                onOfflineSuccess()
                viewModel.writeInternetIsNotAvailable()
            } catch (e: LocationPermissionNotGrantedException) {
                viewModel.visitInfo.bookingOverride = false
                viewModel.visitInfo.capacityOverride = false
                viewModel.isLogVisitApiCallSuccessful.postValue(false)
                val error = mapErrorStringToError(e.message!!)
                logError(
                    exception = e,
                    functionName = "receiveDetections",
                    errorMessage = error.message!!,
                    issue = "Location permission was not granted or was retracted so the log visit attempt failed."
                )
                onFailure(error)
            } catch (e: LocationServicesDisabledException) {
                viewModel.visitInfo.bookingOverride = false
                viewModel.visitInfo.capacityOverride = false
                viewModel.isLogVisitApiCallSuccessful.postValue(false)
                val error = mapErrorStringToError(e.message!!)
                logError(
                    exception = e,
                    functionName = "receiveDetections",
                    errorMessage = error.message!!,
                    issue = "Location services were disabled so the log visit attempt failed."
                )
                onFailure(error)
            } catch (e: AuthenticationException) {
                viewModel.visitInfo.bookingOverride = false
                viewModel.visitInfo.capacityOverride = false
                viewModel.isLogVisitApiCallSuccessful.postValue(false)
                val error = mapErrorStringToError(e.message!!)
                logError(
                    exception = e,
                    functionName = "receiveDetections",
                    errorMessage = error.message!!,
                    issue = "Error occurred during authentication attempt."
                )
                onFailure(error)
            }
        }
    }

    private fun handleCapacityReachedOverride() { // Triggers logVisit (the normal one) which does throw a DuplicateScanException
        setScanComplete()

        // UI Task
        viewLifecycleOwner.lifecycleScope.launch {
            try {
//                Log.e("handleOverride", "Calling OnStarted")
                viewModel.visitInfo.capacityOverride = true
                onStarted()
                withContext(Dispatchers.IO) { viewModel.logVisit() }
                viewModel.writeInternetIsAvailable()
                viewModel.visitInfo.capacityOverride = false
            } catch (e: ApiException) {
                viewModel.visitInfo.capacityOverride = false
                viewModel.isLogVisitApiCallSuccessful.postValue(false)
                val error = mapErrorStringToError(e.message!!)
                logError(
                    exception = e,
                    functionName = "receiveDetections",
                    errorMessage = error.message!!,
                    issue = "API returned error code during attempt to log visit."
                )
                processApiFailureType(error)
            } catch (e: NoInternetException) {
                viewModel.isLogVisitApiCallSuccessful.postValue(false)
                logError(
                    exception = e,
                    functionName = "receiveDetections",
                    errorMessage = e.message!!,
                    issue = "No internet connection during attempt to log visit."
                )
                // Only Log Visit Locally if there is no selected event, otherwise, there is no local logging
                withContext(Dispatchers.IO) {
                    viewModel.logVisitLocal()
                    viewModel.visitInfo.capacityOverride = false
                }
                onOfflineSuccess()
                viewModel.writeInternetIsNotAvailable()
            } catch (e: ConnectionTimeoutException) {
                viewModel.visitInfo.capacityOverride = true
                viewModel.isLogVisitApiCallSuccessful.postValue(false)
                logError(
                    exception = e,
                    functionName = "receiveDetections",
                    errorMessage = e.message!!,
                    issue = "Connection timed out or connection error occurred during attempt to log visit."
                )
                withContext(Dispatchers.IO) {
                    viewModel.logVisitLocal()
                    viewModel.visitInfo.capacityOverride = false
                }
                onOfflineSuccess()
                viewModel.writeInternetIsNotAvailable()
            } catch (e: LocationPermissionNotGrantedException) {
                viewModel.visitInfo.capacityOverride = false
                viewModel.isLogVisitApiCallSuccessful.postValue(false)
                val error = mapErrorStringToError(e.message!!)
                logError(
                    exception = e,
                    functionName = "receiveDetections",
                    errorMessage = error.message!!,
                    issue = "Location permission was not granted or was retracted so the log visit attempt failed."
                )
                onFailure(error)
            } catch (e: LocationServicesDisabledException) {
                viewModel.visitInfo.capacityOverride = false
                viewModel.isLogVisitApiCallSuccessful.postValue(false)
                val error = mapErrorStringToError(e.message!!)
                logError(
                    exception = e,
                    functionName = "receiveDetections",
                    errorMessage = error.message!!,
                    issue = "Location services were disabled so the log visit attempt failed."
                )
                onFailure(error)
            } catch (e: DuplicateScanException) {
                viewModel.visitInfo.capacityOverride = false
                viewModel.isLogVisitApiCallSuccessful.postValue(false)
                val error = mapErrorStringToError(e.message!!)
                onDuplicateScan(error)
            } catch (e: AuthenticationException) {
                viewModel.visitInfo.capacityOverride = false
                viewModel.isLogVisitApiCallSuccessful.postValue(false)
                val error = mapErrorStringToError(e.message!!)
                logError(
                    exception = e,
                    functionName = "receiveDetections",
                    errorMessage = error.message!!,
                    issue = "Error occurred during authentication attempt."
                )
                onFailure(error)
            }
        }
    }

    // Errors must be written in here for scan history to parse them
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
            AppErrorCodes.LOCATION_SERVICES_DISABLED.code -> {
                return AppErrorCodes.LOCATION_SERVICES_DISABLED.message
            }
            AppErrorCodes.PERMISSIONS_NOT_GRANTED.code -> {
                return AppErrorCodes.PERMISSIONS_NOT_GRANTED.message + ": LOCATION"
            }
            AppErrorCodes.DUPLICATE_SCAN.code -> {
                return AppErrorCodes.DUPLICATE_SCAN.message
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
            ApiErrorCodes.NOT_BOOKED.code -> {
                return ApiErrorCodes.NOT_BOOKED.message!!
            }
            AppErrorCodes.CAPACITY_REACHED.code -> {
                return AppErrorCodes.CAPACITY_REACHED.message!!
            }
            ApiErrorCodes.GENERAL_ERROR.code -> {
                return ApiErrorCodes.GENERAL_ERROR.message!!
            }
        }

        return null

    }

    @Suppress("SameParameterValue")
    private fun logError(exception: Exception, functionName: String, errorMessage: String, issue: String) {
        (requireActivity() as MainActivity).logError(
            exception = exception,
            properties = mapOf(
                Pair("Device ID", viewModel.getDeviceId()),
                Pair("Organization", viewModel.visitInfo.organization!!),
                Pair("Door", viewModel.visitInfo.door!!),
                Pair("Direction", viewModel.visitInfo.direction!!),
                Pair("Filename", "LoginFragment.kt"),
                Pair("Function Name", functionName),
                Pair("Error Message", errorMessage),
                Pair("Issue", issue)
            ),
            attachments = null
        )
    }

}