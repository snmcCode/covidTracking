 package ca.snmc.scanner.screens.settings

import android.Manifest
import android.content.pm.PackageManager
import android.os.Bundle
import android.view.LayoutInflater
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
import ca.snmc.scanner.BuildConfig
import ca.snmc.scanner.MainActivity
import ca.snmc.scanner.R
import ca.snmc.scanner.data.db.entities.OrganizationDoorEntity
import ca.snmc.scanner.databinding.SettingsFragmentBinding
import ca.snmc.scanner.models.Error
import ca.snmc.scanner.utils.*
import kotlinx.android.synthetic.main.settings_fragment.*
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.launch
import kotlinx.coroutines.withContext
import org.kodein.di.KodeinAware
import org.kodein.di.android.x.kodein
import org.kodein.di.generic.instance

 private const val SCANNER_VERSION_CLICK_THRESHOLD = 20
 // TODO: Add a testing switch that sets the refresh token breathing room 9 minutes and sets the door to North-West
 class SettingsFragment : Fragment(), KodeinAware {

     override val kodein by kodein()
     private val settingsViewModelFactory : SettingsViewModelFactory by instance()

     private lateinit var binding : SettingsFragmentBinding
     private lateinit var viewModel : SettingsViewModel

     private var isSuccess = true
     private val permissionsRequestCode = 1000

     private var scannerVersionClickCount = 0

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

         binding.scannerModeSelectionDialogButton.setOnClickListener {
             handleScannerModeSelectionDialogButtonClick()
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

         loadData()
         setupSettingsDrawer()
         setupScannerModeSelectionDialog()

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

         val selectedDoor : String = organization_spinner.selectedItem.toString()
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
                     navigateToScannerPage()
                 } else { // Request Permissions
                     requestPermissions(arrayOf(Manifest.permission.CAMERA), permissionsRequestCode)
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

     private fun loadData() {
         viewLifecycleOwner.lifecycleScope.launch {
             onStarted()

             viewModel.getMergedOrgAuthData().observe(viewLifecycleOwner, Observer { combinedOrgAuthData ->

                 if (combinedOrgAuthData?.id != null && combinedOrgAuthData.authorization != null) {

                     if (!viewModel.areOrganizationDoorsFetched()) {
                         handleFetchOrganizationDoors()
                     }

                     viewModel.getMergedDoorVisitData().observe(viewLifecycleOwner, Observer { combinedDoorVisitData ->
                         if (combinedDoorVisitData?.doors != null && combinedDoorVisitData.doors.isNotEmpty()) {
                             setSpinnerData(combinedDoorVisitData.doors)
                             onDataLoaded()

                             if (combinedDoorVisitData.organizationName != null) {
                                 setOrganizationName(combinedDoorVisitData.organizationName!!)
                             }

                             if (combinedDoorVisitData.organizationName != null && combinedDoorVisitData.doorName != null && combinedDoorVisitData.direction != null) {
                                 setDoorAndDirectionFromPreviousData(
                                     combinedDoorVisitData.doorName!!,
                                     combinedDoorVisitData.direction!!,
                                     combinedDoorVisitData.doors
                                 )
                                 onDataLoaded()
                             }
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
                 onFailure(error)
             } catch (e: NoInternetException) {
                 val error = mapErrorStringToError(e.message!!)
                 onFailure(error)
                 viewModel.writeInternetIsNotAvailable()
             } catch (e: ConnectionTimeoutException) {
                 val error = mapErrorStringToError(e.message!!)
                 onFailure(error)
             } catch (e: AppException) {
                 val error = mapErrorStringToError(e.message!!)
                 onFailure(error)
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
                 organization_spinner.setSelection(index)
             }

         }

     }

     private fun setupSettingsDrawer() {
         settings_drawer_scanner_version_container.isClickable = true
         settings_drawer_scanner_version_text.text = BuildConfig.VERSION_NAME
         settings_drawer_authentication_api_text.text = "1.0"
         settings_drawer_backend_api_text.text = "1.0"
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
             if ((permissions[0] == Manifest.permission.CAMERA && grantResults[0] == PackageManager.PERMISSION_GRANTED)) { // Permission Granted
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

     private fun shouldShowRationale() = ActivityCompat.shouldShowRequestPermissionRationale(
         requireActivity(),
         Manifest.permission.CAMERA
     )

     private fun permissionGranted() = ContextCompat.checkSelfPermission(
         requireActivity(),
         Manifest.permission.CAMERA
     ) == PackageManager.PERMISSION_GRANTED

     private fun handleLogout() {
         viewLifecycleOwner.lifecycleScope.launch {
             onStarted()
             viewModel.deleteAllData()
             viewModel.clearPrefs()
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
     }

     private fun enableUi() {
         settings_progress_indicator.hide()
         scan_button.enable()
         logout_button.enable()
     }

     private fun enableUiForFailure() {
         settings_progress_indicator.hide()
         scan_button.disable()
         logout_button.enable()
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

     private fun setSpinnerData(organizationDoors: List<OrganizationDoorEntity>) {
         val doorNames: List<String> = organizationDoors.map { it.doorName }
         val arrayAdapter = ArrayAdapter(requireContext(), R.layout.support_simple_spinner_dropdown_item, doorNames)
         arrayAdapter.setDropDownViewResource(R.layout.support_simple_spinner_dropdown_item)
         organization_spinner.adapter = arrayAdapter
     }

     private fun navigateToScannerPage() {
         val action = SettingsFragmentDirections.actionSettingsFragmentToScannerFragment()
         this.findNavController().navigate(action)
     }

     private fun navigateToLoginPage() {
         val action = SettingsFragmentDirections.actionSettingsFragmentToLoginFragment()
         this.findNavController().navigate(action)
     }

}