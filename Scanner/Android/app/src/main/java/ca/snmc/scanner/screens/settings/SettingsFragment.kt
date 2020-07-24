 package ca.snmc.scanner.screens.settings

import android.os.Bundle
import android.util.Log
import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.widget.ArrayAdapter
import android.widget.SpinnerAdapter
import androidx.fragment.app.Fragment
import androidx.lifecycle.Observer
import androidx.lifecycle.ViewModelProvider
import ca.snmc.scanner.MainActivity
import ca.snmc.scanner.R
import ca.snmc.scanner.data.db.entities.OrganizationDoorEntity
import ca.snmc.scanner.databinding.SettingsFragmentBinding
import ca.snmc.scanner.models.Error
import ca.snmc.scanner.utils.*
import kotlinx.android.synthetic.main.login_fragment.*
import kotlinx.android.synthetic.main.settings_fragment.*
import org.kodein.di.KodeinAware
import org.kodein.di.android.x.kodein
import org.kodein.di.generic.instance

 class SettingsFragment : Fragment(), SettingsListener, KodeinAware {

     override val kodein by kodein()
     private val settingsViewModelFactory : SettingsViewModelFactory by instance()

     override fun onActivityCreated(savedInstanceState: Bundle?) {
         super.onActivityCreated(savedInstanceState)
         (activity as MainActivity).showNavBar()
     }

     override fun onCreateView(
         inflater: LayoutInflater, container: ViewGroup?,
         savedInstanceState: Bundle?
     ): View? {

         val binding : SettingsFragmentBinding = SettingsFragmentBinding.inflate(inflater, container, false)

         // ViewModel
         val viewModel = ViewModelProvider(this, settingsViewModelFactory).get(SettingsViewModel::class.java)

         // Wait for data to load
         Coroutines.main {
             onStarted()
             viewModel.organization.await()
             viewModel.authentication.await()
         }.invokeOnCompletion {
             // Grab organization doors once the organization and authentication objects are loaded
             Coroutines.main {
                 val organizationDoors = viewModel.organizationDoors.await()
                 organizationDoors.observe(viewLifecycleOwner, Observer {
                     setSpinnerData(it)
                     onDataLoaded()
                 })
             }
         }

         // Set ViewModel on Binding object
         binding.viewmodel = viewModel

         // Set LifecycleOwner on Binding object
         binding.lifecycleOwner = this

         // Fragment implements methods defined in LoginListener which are called by ViewModel
         viewModel.settingsListener = this

         // Return the View at the Root of the Binding object
         return binding.root
    }

     override fun onStarted() {
         disableUi()
         removeError()
     }

     override fun onDataLoaded() {
         enableUi()
     }

     override fun onFailure(error: Error) {
         enableUi()
         setError(error)
         Log.e("Error Message", "${error.code}: ${error.message}")
     }

     private fun disableUi() {
         settings_progress_indicator.show()
         scan_button.disable()
     }

     private fun enableUi() {
         settings_progress_indicator.hide()
         scan_button.enable()
     }

     private fun setError(error: Error) {
         var showErrorMessage: Boolean = false

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
         organizationSpinner.adapter = arrayAdapter
     }

}