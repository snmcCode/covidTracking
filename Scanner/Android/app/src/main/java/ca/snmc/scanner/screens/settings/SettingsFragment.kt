 package ca.snmc.scanner.screens.settings

import android.Manifest
import android.content.ClipData
import android.content.ClipboardManager
import android.content.Context
import android.content.pm.PackageManager
import android.os.Bundle
import android.view.LayoutInflater
import android.view.MotionEvent
import android.view.View
import android.view.ViewGroup
import android.widget.ArrayAdapter
import androidx.appcompat.app.AlertDialog
import androidx.core.app.ActivityCompat
import androidx.core.content.ContextCompat
import androidx.core.content.ContextCompat.getColor
import androidx.fragment.app.Fragment
import androidx.lifecycle.Observer
import androidx.lifecycle.ViewModelProvider
import androidx.lifecycle.lifecycleScope
import androidx.navigation.fragment.findNavController
import androidx.recyclerview.widget.LinearLayoutManager
import ca.snmc.scanner.BuildConfig
import ca.snmc.scanner.MainActivity
import ca.snmc.scanner.R
import ca.snmc.scanner.data.db.entities.OrganizationDoorEntity
import ca.snmc.scanner.databinding.SettingsFragmentBinding
import ca.snmc.scanner.models.Error
import ca.snmc.scanner.models.EventListItem
import ca.snmc.scanner.utils.*
import ca.snmc.scanner.utils.adapters.EventRecyclerViewAdapter
import ca.snmc.scanner.utils.adapters.OnEventItemClickListener
import kotlinx.android.synthetic.main.settings_fragment.*
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.launch
import kotlinx.coroutines.withContext
import org.kodein.di.KodeinAware
import org.kodein.di.android.x.kodein
import org.kodein.di.generic.instance

 private const val SCANNER_VERSION_CLICK_THRESHOLD = 20
 // TODO: Add a testing switch that sets the refresh token breathing room 9 minutes and sets the door to North-West
 class SettingsFragment : Fragment(), KodeinAware, OnEventItemClickListener {

     override val kodein by kodein()
     private val settingsViewModelFactory : SettingsViewModelFactory by instance()

     private lateinit var binding : SettingsFragmentBinding
     private lateinit var viewModel : SettingsViewModel

     private var isSuccess = true
     private val permissionsRequestCode = 1000

     private var scannerVersionClickCount = 0

     private lateinit var eventAdapter : EventRecyclerViewAdapter
     private var selectedEvent : Int? = null

     override fun onCreateView(
         inflater: LayoutInflater, container: ViewGroup?,
         savedInstanceState: Bundle?
     ): View? {
         (activity as MainActivity).windowedMode()

         binding = SettingsFragmentBinding.inflate(inflater, container, false)

         // Set LifecycleOwner on Binding object
         binding.lifecycleOwner = this

         binding.logoutButton.setOnClickListener {
             handleLogOutButtonClick()
         }

         binding.scanButton.setOnClickListener {
             handleScanButtonClick()
         }

         binding.infoButton.setOnClickListener {
             handleInfoButtonClick()
         }

         binding.selectEventButton.setOnClickListener {
             handleSelectEventButtonClick()
         }

         binding.removeEventButton.setOnClickListener {
             handleRemoveEventButtonClick()
         }

         binding.scannerModeSelectionDialogButton.setOnClickListener {
             handleScannerModeSelectionDialogButtonClick()
         }

         binding.eventSelectionDialogButton.setOnClickListener {
             handleEventSelectionDialogButtonClick()
         }

         // ViewModel
         viewModel = ViewModelProvider(this, settingsViewModelFactory).get(SettingsViewModel::class.java)

         // Get Necessary Data from Local DB
         viewLifecycleOwner.lifecycleScope.launch {
             viewModel.initialize()
         }

         // Return the View at the Root of the Binding object
         return binding.root
    }

     override fun onViewCreated(view: View, savedInstanceState: Bundle?) {
         super.onViewCreated(view, savedInstanceState)

         viewLifecycleOwner.lifecycleScope.launch {
             loadData()
             setupSettingsDrawer()
             setupScannerModeSelectionDialog()
             initRecyclerView()
         }

     }

     override fun onResume() {
         super.onResume()

         (activity as MainActivity).windowedMode()
     }

     private fun initRecyclerView() {
         viewLifecycleOwner.lifecycleScope.launch {
             eventAdapter = EventRecyclerViewAdapter(this@SettingsFragment)
             event_recycler_view.apply {
                 layoutManager = LinearLayoutManager(requireActivity())
                 adapter = eventAdapter
             }
         }

         // Observe the observable
         viewModel.events.observe(viewLifecycleOwner, Observer {
             viewLifecycleOwner.lifecycleScope.launch {
                 if (viewModel.areEventsTodayFetched()) {
                     onEvents()
                     eventAdapter.submitList(mapEventEntityListToEventListItemList(it))
                 } else {
                     withContext(Dispatchers.IO) {
                         viewModel.deleteAllEvents()
                     }
                     onNoEvents()
                     eventAdapter.submitList(listOf())
                 }
                 eventAdapter.notifyDataSetChanged()
             }
         })
     }

     private fun onEvents() {
         no_events_message.visibility = View.GONE
         event_selection_body.visibility = View.VISIBLE
     }

     private fun onNoEvents() {
         event_selection_body.visibility = View.GONE
         no_events_message.visibility = View.VISIBLE
     }

     override fun onItemClick(item: EventListItem, position: Int) {
         eventAdapter.selectItem(position)
         selectedEvent = item.id
     }

     private fun handleLogOutButtonClick() {

         val alertDialog = AlertDialog.Builder(requireContext())
         alertDialog.setTitle(R.string.logout_dialog_title)
         alertDialog.setMessage(R.string.logout_dialog_message)
         alertDialog.setPositiveButton(R.string.logout_dialog_positive_button) { _, _ ->
             handleLogout()
         }
         alertDialog.setNegativeButton(R.string.logout_dialog_negative_button) { dialog, _ ->
             dialog.dismiss()
         }
         alertDialog.show()

     }

     private fun handleScanButtonClick() {

         val selectedDoor : String = door_spinner.selectedItem.toString()
         val selectedDirection : String = if (direction_switch.isChecked) {
             direction_switch.textOn.toString()
         } else {
             direction_switch.textOff.toString()
         }

         viewLifecycleOwner.lifecycleScope.launch {
             onStarted()
             viewModel.saveVisitSettings(selectedDoor, selectedDirection)
             activity?.let {
                 if (permissionGranted()) { // Permission Granted
                     disableUi()
                     navigateToScannerPage()
                 } else { // Request Permissions
                     requestPermissions(arrayOf(Manifest.permission.CAMERA, Manifest.permission.ACCESS_FINE_LOCATION), permissionsRequestCode)
                 }
             }
         }

     }

     private fun handleInfoButtonClick() {

         if (settings_drawer.visibility == View.GONE) {
             settings_drawer.visibility = View.VISIBLE
         } else {
             settings_drawer.visibility = View.GONE
         }

     }

     private fun handleSelectEventButtonClick() {
         event_selection_dialog.visibility = View.VISIBLE
     }

     private fun handleRemoveEventButtonClick() {
         if (selectedEvent != null) {
             onEventRemoved()
         }
     }

     private fun handleScannerModeSelectionDialogButtonClick() {
         val scannerMode : Int = viewModel.getScannerMode()
         val switchEnabled : Boolean = scanner_mode_selection_dialog_switch.isChecked
         val selectedScannerMode : Int = if (switchEnabled) { TESTING_MODE } else { PRODUCTION_MODE }

         if (selectedScannerMode == scannerMode) {
             // No modification

             if (scanner_mode_selection_dialog.visibility == View.VISIBLE) {
                 scanner_mode_selection_dialog.visibility = View.GONE
             }
             if (settings_drawer.visibility == View.VISIBLE) {
                 settings_drawer.visibility = View.GONE
             }

         } else {
             // Scanner Mode modified

             if (switchEnabled) {
                 onTestingMode()
             } else {
                 onProductionMode()
             }

         }
     }

     private fun handleEventSelectionDialogButtonClick() {
         event_selection_dialog.visibility = View.GONE

         if (selectedEvent != null) {
             onEventSelected()
         }
     }

     private fun onEventSelected() {
         viewLifecycleOwner.lifecycleScope.launch {
             onStarted()
             viewModel.saveSelectedEvent(selectedEvent!!)
             onDataLoaded()
         }
     }

     private fun onEventRemoved() {
         selectedEvent = null
         eventAdapter.selectItem(-1)
         viewLifecycleOwner.lifecycleScope.launch {
             onStarted()
             viewModel.deleteSelectedEvent()
         }
     }

     private fun loadData() {
         viewLifecycleOwner.lifecycleScope.launch {
             onStarted()

             viewModel.getMergedOrgAuthData().observe(viewLifecycleOwner, Observer { combinedOrgAuthData ->

                 if (combinedOrgAuthData?.id != null && combinedOrgAuthData.authorization != null) {

                     if (!viewModel.areOrganizationDoorsFetched()) {
                         handleFetchOrganizationDoors()
                     }

//                     Log.e("Fetch Events Test", "About to Fetch Events")
                     handleFetchOrganizationEventsToday()

                     viewModel.getMergedDoorVisitData().observe(viewLifecycleOwner, Observer { combinedDoorVisitData ->
                         if (combinedDoorVisitData?.doors != null && combinedDoorVisitData.doors.isNotEmpty()) {
                             setDoorSpinnerData(combinedDoorVisitData.doors)

                             if (combinedDoorVisitData.organizationName != null) {
                                 setOrganizationName(combinedDoorVisitData.organizationName!!)
                             }

                             onDataLoaded()

                             if (combinedDoorVisitData.organizationName != null && combinedDoorVisitData.doorName != null && combinedDoorVisitData.direction != null) {
                                 setDoorAndDirectionFromPreviousData(
                                     combinedDoorVisitData.doorName!!,
                                     combinedDoorVisitData.direction!!,
                                     combinedDoorVisitData.doors
                                 )
                                 onDataLoaded()
                             }

                             viewModel.getSavedSelectedEventDirectly().observe(viewLifecycleOwner, Observer { selectedEventEntity ->
                                 if (selectedEventEntity != null && selectedEventEntity.eventId != null) {
                                     setEventSelectedMode()
                                     selectedEvent = selectedEventEntity.eventId
                                 } else {
                                     setNoEventSelectedMode()
                                 }
                                 onDataLoaded()
                             })

                         } else {
                             onStarted()
                         }
                     })

                 } else {
                     onStarted()
                 }

             })

         }
     }

     private fun handleFetchOrganizationDoors() {
         // Reset success flag
         isSuccess = true

         viewLifecycleOwner.lifecycleScope.launch {
             try {
                 onStarted()
                 withContext(Dispatchers.IO) { viewModel.fetchOrganizationDoors() }
             } catch (e: ApiException) {
                 val error = mapErrorStringToError(e.message!!)
                 logError(
                     exception = e,
                     functionName = "handleFetchOrganizationDoors",
                     errorMessage = error.message!!,
                     issue = "API returned error code during attempt to fetch organization doors."
                 )
                 onFailure(error)
             } catch (e: NoInternetException) {
                 val error = mapErrorStringToError(e.message!!)
                 logError(
                     exception = e,
                     functionName = "handleFetchOrganizationDoors",
                     errorMessage = error.message!!,
                     issue = "No internet connection during attempt to fetch organization doors."
                 )
                 onFailure(error)
                 viewModel.writeInternetIsNotAvailable()
             } catch (e: ConnectionTimeoutException) {
                 val error = mapErrorStringToError(e.message!!)
                 logError(
                     exception = e,
                     functionName = "handleFetchOrganizationDoors",
                     errorMessage = error.message!!,
                     issue = "Connection timed out or connection error occurred during attempt to fetch organization doors."
                 )
                 onFailure(error)
             } catch (e: AuthenticationException) {
                 val error = mapErrorStringToError(e.message!!)
                 logError(
                     exception = e,
                     functionName = "handleFetchOrganizationDoors",
                     errorMessage = error.message!!,
                     issue = "Error occurred during authentication attempt."
                 )
                 onFailure(error)
             } catch (e: EmptyResponseException) {
                 val error = mapErrorStringToError(e.message!!)
                 logError(
                     exception = e,
                     functionName = "handleFetchOrganizationDoors",
                     errorMessage = error.message!!,
                     issue = "Couldn't find any doors associated with organization."
                 )
                 onFailure(error)
             }
         }
     }

     private fun handleFetchOrganizationEventsToday() {
         // Reset success flag
         isSuccess = true

         viewLifecycleOwner.lifecycleScope.launch {
             try {
                 onStarted()
                 withContext(Dispatchers.IO) { viewModel.fetchOrganizationEventsToday() }
             } catch (e: ApiException) {
                 val error = mapErrorStringToError(e.message!!)
                 logError(
                     exception = e,
                     functionName = "handleFetchOrganizationEventsToday",
                     errorMessage = error.message!!,
                     issue = "API returned error code during attempt to fetch organization events for today."
                 )
             } catch (e: NoInternetException) {
                 val error = mapErrorStringToError(e.message!!)
                 logError(
                     exception = e,
                     functionName = "handleFetchOrganizationEventsToday",
                     errorMessage = error.message!!,
                     issue = "No internet connection during attempt to fetch organization events for today."
                 )
                 viewModel.writeInternetIsNotAvailable()
             } catch (e: ConnectionTimeoutException) {
                 val error = mapErrorStringToError(e.message!!)
                 logError(
                     exception = e,
                     functionName = "handleFetchOrganizationEventsToday",
                     errorMessage = error.message!!,
                     issue = "Connection timed out or connection error occurred during attempt to fetch organization events for today."
                 )
             } catch (e: AuthenticationException) {
                 val error = mapErrorStringToError(e.message!!)
                 logError(
                     exception = e,
                     functionName = "handleFetchOrganizationEventsToday",
                     errorMessage = error.message!!,
                     issue = "Error occurred during during authentication attempt."
                 )
             }
             catch (e: EmptyResponseException) {
                 val error = mapErrorStringToError(e.message!!)
//                 Log.e("Fetch Events Test", e.message)
                 logError(
                     exception = e,
                     functionName = "handleFetchOrganizationEventsToday",
                     errorMessage = error.message!!,
                     issue = "Couldn't find any events associated with organization for today."
                 )
             }
         }
     }

     private fun setOrganizationName(organizationName: String) {
         organization_name.text = organizationName
     }

     private fun setDoorAndDirectionFromPreviousData(
         selectedDoor: String,
         selectedDirection: String,
         doors: List<OrganizationDoorEntity>
     ) {

         if (
             (selectedDirection == direction_switch.textOn.toString() && !direction_switch.isChecked)
             || (selectedDirection == direction_switch.textOff.toString() && direction_switch.isChecked)
         ) {
             direction_switch.toggle()
         }

         if (doors.isNotEmpty()) {

             val index = doors.indexOfFirst { it.doorName == selectedDoor }
             if (index != -1) {
                 door_spinner.setSelection(index)
             }

         }

     }

     private fun setEventSelectedMode() {
         select_event_button.visibility = View.GONE
         remove_event_button.visibility = View.VISIBLE
     }

     private fun setNoEventSelectedMode() {
         remove_event_button.visibility = View.GONE
         select_event_button.visibility = View.VISIBLE
     }

     private fun setupSettingsDrawer() {
         settings_drawer_device_id_container.isClickable = true
         settings_drawer_scanner_version_container.isClickable = true
         settings_drawer_scanner_version_text.text = BuildConfig.VERSION_NAME
         settings_drawer_authentication_api_text.text = "1.0"
         settings_drawer_backend_api_text.text = "1.0"
         settings_drawer_device_id_text.text = viewModel.getDeviceId()
         // TODO: Try click listener or long click or this, get ripple animation working!!
         settings_drawer_device_id_container.setOnTouchListener(View.OnTouchListener { view, motionEvent ->
             when (motionEvent.action) {
                 MotionEvent.ACTION_DOWN -> {
                     settings_drawer_device_id_container.isPressed = true
                     view.performClick()
                 }
                 MotionEvent.ACTION_UP -> {
                     settings_drawer_device_id_container.isPressed = false
                     view.performClick()
                     handleDeviceIdClick()
                 }
             }
             return@OnTouchListener true
         })
         settings_drawer_scanner_version_container.setOnClickListener {
             handleScannerVersionClick()
         }
     }

     private fun setupScannerModeSelectionDialog() {

         val scannerMode : Int = viewModel.getScannerMode()

         if (scannerMode == TESTING_MODE) {
             scanner_mode_selection_dialog_state.text = getString(R.string.scanner_mode_selection_dialog_message_testing_mode)
             scanner_mode_selection_dialog_state.setTextColor(getColor(requireContext(), R.color.failureIndicator))
             scanner_mode_selection_dialog_switch.isChecked = true
         } else {
             scanner_mode_selection_dialog_state.text = getString(R.string.scanner_mode_selection_dialog_message_production_mode)
             scanner_mode_selection_dialog_state.setTextColor(getColor(requireContext(), R.color.successIndicator))
             scanner_mode_selection_dialog_switch.isChecked = false
         }

         scanner_mode_selection_dialog_switch.setOnCheckedChangeListener { _, isChecked ->
             if (isChecked) {
                 updateScannerModeSelectionDialogOnTestingMode()
             } else {
                 updateScannerModeSelectionDialogOnProductionMode()
             }
         }
     }

     private fun handleDeviceIdClick() {
         val deviceId = viewModel.getDeviceId()
         val clipboard = requireActivity().getSystemService(Context.CLIPBOARD_SERVICE) as ClipboardManager
         val deviceIdClip: ClipData = ClipData.newPlainText("Device ID", deviceId)
         clipboard.setPrimaryClip(deviceIdClip)
         requireActivity().toast("Device ID: $deviceId Copied!")
     }

     private fun handleScannerVersionClick() {
         scannerVersionClickCount += 1

         if (scannerVersionClickCount >= (SCANNER_VERSION_CLICK_THRESHOLD - 5) && scannerVersionClickCount < SCANNER_VERSION_CLICK_THRESHOLD) {
             requireActivity().toast("You are ${SCANNER_VERSION_CLICK_THRESHOLD - scannerVersionClickCount} steps away from accessing Scanner Mode Selection.")
         }

         if (scannerVersionClickCount == SCANNER_VERSION_CLICK_THRESHOLD) {
             requireActivity().toast("You now have access to Scanner Mode Selection.")
             scannerVersionClickCount = 0

             scanner_mode_selection_dialog.visibility = View.VISIBLE

         }
     }

     // TODO: Need to add some functionality to disable dialog UI and a progress indicator on top of the dialog UI
     private fun onTestingMode() {
         viewLifecycleOwner.lifecycleScope.launch {
             onStartSaveScannerMode()
             viewModel.saveScannerMode(TESTING_MODE)
         }.invokeOnCompletion {
             viewLifecycleOwner.lifecycleScope.launch {
                 (activity as MainActivity).updateTestingModeIndicator()
                 handleLogout()
             }
         }
     }

     private fun onProductionMode() {
         viewLifecycleOwner.lifecycleScope.launch {
             onStartSaveScannerMode()
             viewModel.saveScannerMode(PRODUCTION_MODE)
         }.invokeOnCompletion {
             viewLifecycleOwner.lifecycleScope.launch {
                 (activity as MainActivity).updateTestingModeIndicator()
                 handleLogout()
             }
         }
     }

     private fun onStartSaveScannerMode() {
         scanner_mode_selection_dialog_button.isEnabled = false
         scanner_mode_selection_progress_indicator_container.visibility = View.VISIBLE
     }

     private fun updateScannerModeSelectionDialogOnTestingMode() {
         scanner_mode_selection_dialog_state.setTextColor(getColor(requireContext(), R.color.failureIndicator))
         scanner_mode_selection_dialog_state.text = getString(R.string.scanner_mode_selection_dialog_message_testing_mode)
     }

     private fun updateScannerModeSelectionDialogOnProductionMode() {
         scanner_mode_selection_dialog_state.setTextColor(getColor(requireContext(), R.color.successIndicator))
         scanner_mode_selection_dialog_state.text = getString(R.string.scanner_mode_selection_dialog_message_production_mode)
     }

     override fun onRequestPermissionsResult(
         requestCode: Int,
         permissions: Array<String>,
         grantResults: IntArray
     ) {
         if (requestCode == permissionsRequestCode) {
             if ((permissions[0] == Manifest.permission.CAMERA && grantResults[0] == PackageManager.PERMISSION_GRANTED)
                 && (permissions[1] == Manifest.permission.ACCESS_FINE_LOCATION && grantResults[1] == PackageManager.PERMISSION_GRANTED)) { // Permission Granted
                 disableUi()
                 navigateToScannerPage()
             } else { // Permission Denied
                 if (!shouldShowRationale()) { // User Selected Do Not Ask Again
                     onPermissionsFailure(AppErrorCodes.PERMISSIONS_NOT_GRANTED_NEVER_ASK_AGAIN)
                 } else { // User Did Not Select Do Not Ask Again
                     onPermissionsFailure(AppErrorCodes.PERMISSIONS_NOT_GRANTED)
                 }
             }
         } else {
             onPermissionsFailure(AppErrorCodes.PERMISSIONS_NOT_GRANTED)
         }
     }

     private fun shouldShowRationale() = ActivityCompat.shouldShowRequestPermissionRationale(requireActivity(), Manifest.permission.CAMERA)
             || ActivityCompat.shouldShowRequestPermissionRationale(requireActivity(), Manifest.permission.ACCESS_FINE_LOCATION)

     private fun permissionGranted() =
         (ContextCompat.checkSelfPermission(requireActivity(), Manifest.permission.CAMERA) == PackageManager.PERMISSION_GRANTED)
                 && (ContextCompat.checkSelfPermission(requireActivity(), Manifest.permission.ACCESS_FINE_LOCATION) == PackageManager.PERMISSION_GRANTED)

     private fun handleLogout() {
         viewLifecycleOwner.lifecycleScope.launch {
             onStarted()
             viewModel.deleteAllData()
             viewModel.clearPrefs()
             disableUi()
             navigateToLoginPage()
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
         enableUiForFailure()
         setError(error)
//         Log.e("Error Message", "${error.code}: ${error.message}")
         isSuccess = false
     }

     private fun onPermissionsFailure(error: Error) {
         enableUi()
         setError(error)
//         Log.e("Error Message", "${error.code}: ${error.message}")
         isSuccess = false
     }

     private fun disableUi() {
         settings_progress_indicator.show()
         scan_button.disable()
         logout_button.disable()
         info_button.disable()
     }

     private fun enableUi() {
         settings_progress_indicator.hide()
         scan_button.enable()
         logout_button.enable()
         info_button.enable()
     }

     private fun enableUiForFailure() {
         settings_progress_indicator.hide()
         scan_button.disable()
         logout_button.enable()
         info_button.enable()
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
             AppErrorCodes.NULL_ORGANIZATION_DOORS_RESPONSE.code -> {
                 showErrorMessage = true
                 errorMessageText = AppErrorCodes.NULL_ORGANIZATION_DOORS_RESPONSE.message
             }
             AppErrorCodes.NO_INTERNET.code -> {
                 showErrorMessage = true
                 errorMessageText = AppErrorCodes.NO_INTERNET.message
             }
             AppErrorCodes.CONNECTION_TIMEOUT.code -> {
                 showErrorMessage = true
                 errorMessageText = AppErrorCodes.CONNECTION_TIMEOUT.message
             }
             AppErrorCodes.PERMISSIONS_NOT_GRANTED.code -> {
                 showErrorMessage = true
                 errorMessageText = AppErrorCodes.PERMISSIONS_NOT_GRANTED.message
             }
             AppErrorCodes.PERMISSIONS_NOT_GRANTED_NEVER_ASK_AGAIN.code -> {
                 showErrorMessage = true
                 errorMessageText = AppErrorCodes.PERMISSIONS_NOT_GRANTED_NEVER_ASK_AGAIN.message
             }
             ApiErrorCodes.UNAUTHORIZED.code -> {
                 showErrorMessage = true
                 errorMessageText = ApiErrorCodes.UNAUTHORIZED.message
             }
             ApiErrorCodes.ORGANIZATION_NOT_FOUND_IN_SQL_DATABASE.code -> {
                 showErrorMessage = true
                 errorMessageText = ApiErrorCodes.ORGANIZATION_NOT_FOUND_IN_SQL_DATABASE.message
             }
             ApiErrorCodes.GENERAL_ERROR.code -> {
                 showErrorMessage = true
                 errorMessageText = ApiErrorCodes.GENERAL_ERROR.message
             }
             else -> {
                 // This state means the error is unaccounted for
                 showErrorMessage = false
//                 Log.e("Error Message", "${error.code}: ${error.message}")
             }
         }

         if (showErrorMessage && errorMessageText != null) {
             settings_error_indicator.showError(errorMessageText)
         }
     }

     private fun removeError() {
         settings_error_indicator.hideError()
     }

     private fun setDoorSpinnerData(organizationDoors: List<OrganizationDoorEntity>) {
         val doorNames: List<String> = organizationDoors.map { it.doorName }
         val arrayAdapter = ArrayAdapter(requireContext(), R.layout.support_simple_spinner_dropdown_item, doorNames)
         arrayAdapter.setDropDownViewResource(R.layout.support_simple_spinner_dropdown_item)
         door_spinner.adapter = arrayAdapter
     }

     private fun navigateToScannerPage() {
         val action = SettingsFragmentDirections.actionSettingsFragmentToScannerFragment()
         this.findNavController().navigate(action)
     }

     private fun navigateToLoginPage() {
         val action = SettingsFragmentDirections.actionSettingsFragmentToLoginFragment()
         this.findNavController().navigate(action)
     }

     @Suppress("SameParameterValue")
     private fun logError(exception: Exception, functionName: String, errorMessage: String, issue: String) {
         (requireActivity() as MainActivity).logError(
             exception = exception,
             properties = mapOf(
                 Pair("Device ID", viewModel.getDeviceId()),
                 Pair("Filename", "SettingsFragment.kt"),
                 Pair("Function Name", functionName),
                 Pair("Error Message", errorMessage),
                 Pair("Issue", issue)
             ),
             attachments = null
         )
     }

}