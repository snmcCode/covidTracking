 package ca.snmc.scanner.screens.settings

import android.os.Bundle
import android.util.Log
import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.widget.ArrayAdapter
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

     private var isSuccess = true

     override fun onCreateView(
         inflater: LayoutInflater, container: ViewGroup?,
         savedInstanceState: Bundle?
     ): View? {
         (activity as MainActivity).windowedMode()

         binding = SettingsFragmentBinding.inflate(inflater, container, false)

         // Set LifecycleOwner on Binding object
         binding.lifecycleOwner = this

         binding.scanButton.setOnClickListener {
             handleScanButtonClick()
         }

         // ViewModel
         viewModel = ViewModelProvider(this, settingsViewModelFactory).get(SettingsViewModel::class.java)

         // Get Necessary Data from Local DB
         viewLifecycleOwner.lifecycleScope.launch {
             viewModel.initialize()
         }

         viewModel.getVisitInfo().observe(viewLifecycleOwner, Observer {
             if (it != null) {
                 navigate()
             }
         })

         // Return the View at the Root of the Binding object
         return binding.root
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
             viewModel.saveVisitInfo(selectedDoor, selectedDirection)
             navigate()
         }

     }

     override fun onViewCreated(view: View, savedInstanceState: Bundle?) {
         super.onViewCreated(view, savedInstanceState)

//         loadSavedData()
         loadViewModelData()
         getDoors()

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

     // TODO: Figure out how to write this to support two-way binding
     private fun loadSavedData() {
         viewLifecycleOwner.lifecycleScope.launch {
             viewModel.getVisitInfo().observe(viewLifecycleOwner, Observer {
                 if (it != null) {
                     if (it.doorName != null && it.direction != null) {
                         setDoorAndDirectionFromPreviousData(it.doorName!!, it.direction!!)
                         coroutineContext.cancel()
                     }
                 }
             })
         }
     }

     // TODO: Figure out how to do this with two-way data-binding or an adapter
     private fun setDoorAndDirectionFromPreviousData(selectedDoor: String, selectedDirection: String) {
     }

     private fun loadViewModelData() {
         viewLifecycleOwner.lifecycleScope.launch {
             viewModel.getMergedData().observe(viewLifecycleOwner, Observer {
                 if (it != null) {
                     if (it.id != null && it.authorization != null) {
                         if (!viewModel.areOrganizationDoorsFetched()) {
                             handleFetchOrganizationDoors()
                         } else {
                             onDataLoaded()
                             coroutineContext.cancel()
                         }
                     } else {
                         onStarted()
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
                     setSpinnerData(organizationDoors)
                     onDataLoaded()
                     coroutineContext.cancel()
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
         enableUiForFailure()
         setError(error)
         Log.e("Error Message", "${error.code}: ${error.message}")
         isSuccess = false
     }

     private fun disableUi() {
         settings_progress_indicator.show()
         scan_button.disable()
     }

     private fun enableUi() {
         settings_progress_indicator.hide()
         scan_button.enable()
     }

     private fun enableUiForFailure() {
         settings_progress_indicator.hide()
         scan_button.disable()
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

     private fun navigate() {
         val action = SettingsFragmentDirections.actionSettingsFragmentToScannerFragment()
         this.findNavController().navigate(action)
     }

}