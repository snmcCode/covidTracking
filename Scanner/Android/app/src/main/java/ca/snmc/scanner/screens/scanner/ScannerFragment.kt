package ca.snmc.scanner.screens.scanner

import android.Manifest
import android.content.pm.PackageManager
import android.os.Bundle
import android.util.Log
import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import androidx.core.app.ActivityCompat
import androidx.core.content.ContextCompat
import androidx.fragment.app.Fragment
import androidx.lifecycle.Observer
import androidx.lifecycle.ViewModelProvider
import androidx.lifecycle.lifecycleScope
import androidx.navigation.fragment.findNavController
import ca.snmc.scanner.MainActivity
import ca.snmc.scanner.databinding.ScannerFragmentBinding
import ca.snmc.scanner.models.Error
import ca.snmc.scanner.utils.*
import kotlinx.android.synthetic.main.scanner_fragment.*
import kotlinx.coroutines.cancel
import kotlinx.coroutines.launch
import org.kodein.di.KodeinAware
import org.kodein.di.android.x.kodein
import org.kodein.di.generic.instance

class ScannerFragment : Fragment(), KodeinAware {

    override val kodein by kodein()
    private val scannerViewModelFactory : ScannerViewModelFactory by instance()

    private lateinit var binding : ScannerFragmentBinding
    private lateinit var viewModel: ScannerViewModel

    private var isSuccess = true

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

    private fun navigate() {
        val action = ScannerFragmentDirections.actionScannerFragmentToSettingsFragment()
        this.findNavController().navigate(action)
    }

    override fun onViewCreated(view: View, savedInstanceState: Bundle?) {
        super.onViewCreated(view, savedInstanceState)

        loadViewModelData()
        // TODO: Implement Library for QR code scanning

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
        enableUiForFailure()
        setError(error)
        Log.e("Error Message", "${error.code}: ${error.message}")
        isSuccess = false
    }

    private fun disableUi() {
        scanner_indicator_square.show()
        scanner_indicator_inner_square.show()
        scanner_progress_indicator.show()
        settings_button.disable()
    }

    private fun enableUi() {
        scanner_indicator_square.hide()
        scanner_indicator_inner_square.hide()
        scanner_progress_indicator.hide()
        settings_button.enable()
    }

    private fun enableUiForFailure() {
        scanner_indicator_square.hide()
        scanner_indicator_inner_square.hide()
        scanner_progress_indicator.hide()
        settings_button.disable()
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
            scanner_error_indicator.showError(errorMessageText)
        }
    }

    private fun removeError() {
        scanner_error_indicator.hideError()
    }

}