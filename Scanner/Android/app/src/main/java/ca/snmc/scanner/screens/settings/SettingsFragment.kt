 package ca.snmc.scanner.screens.settings

import android.Manifest
import android.content.pm.PackageManager
import android.os.Bundle
import android.util.Log
import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.widget.ArrayAdapter
import androidx.appcompat.app.AlertDialog
import androidx.core.app.ActivityCompat
import androidx.core.content.ContextCompat
import androidx.fragment.app.Fragment
import androidx.lifecycle.Observer
import androidx.lifecycle.ViewModelProvider
import androidx.lifecycle.lifecycleScope
import androidx.navigation.fragment.findNavController
import ca.snmc.scanner.MainActivity
import ca.snmc.scanner.R
import ca.snmc.scanner.data.db.entities.OrganizationDoorEntity
import ca.snmc.scanner.databinding.SettingsFragmentBinding
import ca.snmc.scanner.models.Error
import ca.snmc.scanner.utils.*
import kotlinx.android.synthetic.main.settings_fragment.*
import kotlinx.coroutines.cancel
import kotlinx.coroutines.launch
import org.kodein.di.KodeinAware
import org.kodein.di.android.x.kodein
import org.kodein.di.generic.instance

 class SettingsFragment : Fragment(), KodeinAware {

     override val kodein by kodein()
     private val settingsViewModelFactory : SettingsViewModelFactory by instance()

     private lateinit var binding : SettingsFragmentBinding
     private lateinit var viewModel : SettingsViewModel

     private lateinit var savedOrganizationDoors: List<OrganizationDoorEntity>

     private var isSuccess = true
     private val permissionsRequestCode = 1000

     override fun onCreateView(
         inflater: LayoutInflater, container: ViewGroup?,
         savedInstanceState: Bundle?
     ): View? {
         (activity as MainActivity).windowedMode()

         binding = SettingsFragmentBinding.inflate(inflater, container, false)

         // Set LifecycleOwner on Binding object
         binding.lifecycleOwner = this

         binding.logoutButton.setOnClickListener {
             handleLoginButtonClick()
         }

         binding.scanButton.setOnClickListener {
             handleScanButtonClick()
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
         getDoors()

     }

     private fun handleLoginButtonClick() {
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

     private fun loadData() {
         viewLifecycleOwner.lifecycleScope.launch {
             onStarted()
             viewModel.getMergedData().observe(viewLifecycleOwner, Observer {
                 if (it?.id != null && it.authorization != null) {
                     if (!viewModel.areOrganizationDoorsFetched()) {
                         handleFetchOrganizationDoors()
                     } else {
                         // TODO: What causes this to be hit after OrganizationDoors are fetched? This must be hit for the view to work
                         loadVisitSettings()
                         onDataLoaded()
                         coroutineContext.cancel()
                     }
                 } else {
                     onStarted()
                 }
             })
         }
     }

     private fun getDoors() {
         // Wait for OrganizationDoors to load, this is done in a coroutine
         viewLifecycleOwner.lifecycleScope.launch {
             onStarted()
             viewModel.getOrganizationDoors().observe(viewLifecycleOwner, Observer { organizationDoors ->
                 if (organizationDoors != null && organizationDoors.isNotEmpty()) {
                     savedOrganizationDoors = organizationDoors
                     setSpinnerData(organizationDoors)
                     onDataLoaded()
                 }
             })
         }
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

     private fun permissionNotGranted() = ContextCompat.checkSelfPermission(
         requireActivity(),
         Manifest.permission.CAMERA
     ) == PackageManager.PERMISSION_DENIED

     private fun handleLogout() {
         viewLifecycleOwner.lifecycleScope.launch {
             onStarted()
             viewModel.deleteAllData()
             viewModel.clearPrefs()
             navigateToLoginPage()
         }
     }

     private fun handleFetchOrganizationDoors() {
         // Reset success flag
         isSuccess = true

         viewLifecycleOwner.lifecycleScope.launch {
             try {
                 viewModel.fetchOrganizationDoors()
             } catch (e: ApiException) {
                 val error = mapErrorStringToError(e.message!!)
                 onFailure(error)
             } catch (e: NoInternetException) {
                 val error = mapErrorStringToError(e.message!!)
                 onFailure(error)
                 viewModel.writeInternetIsNotAvailable()
             } catch (e: AppException) {
                 val error = mapErrorStringToError(e.message!!)
                 onFailure(error)
             }
         }
     }

     private fun loadVisitSettings() {
         viewLifecycleOwner.lifecycleScope.launch {
             onStarted()
             viewModel.getSavedVisitSettingsDirectly().observe(viewLifecycleOwner, Observer {
                 if (it?.doorName != null && it.direction != null) {
                     setDoorAndDirectionFromPreviousData(it.doorName!!, it.direction!!)
                     onDataLoaded()
                     coroutineContext.cancel()
                 } else {
                     onStarted()
                 }
             })
         }
     }

     private fun setDoorAndDirectionFromPreviousData(selectedDoor: String, selectedDirection: String) {

         if (
             (selectedDirection == direction_switch.textOn.toString() && !direction_switch.isChecked)
             || (selectedDirection == direction_switch.textOff.toString() && direction_switch.isChecked)
         ) {
             direction_switch.toggle()
         }

         if (this::savedOrganizationDoors.isInitialized && savedOrganizationDoors.isNotEmpty()) {

             val index = savedOrganizationDoors.indexOfFirst { it.doorName == selectedDoor  }
             if (index != -1) {
                 organization_spinner.setSelection(index)
             }

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
         Log.e("Error Message", "${error.code}: ${error.message}")
         isSuccess = false
     }

     private fun onPermissionsFailure(error: Error) {
         enableUi()
         setError(error)
         Log.e("Error Message", "${error.code}: ${error.message}")
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
                 Log.e("Error Message", "${error.code}: ${error.message}")
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