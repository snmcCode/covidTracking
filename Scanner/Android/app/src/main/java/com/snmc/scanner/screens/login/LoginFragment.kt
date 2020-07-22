package com.snmc.scanner.screens.login

import android.os.Bundle
import android.util.Log
import androidx.fragment.app.Fragment
import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import androidx.lifecycle.Observer
import androidx.lifecycle.ViewModelProvider
import androidx.navigation.fragment.findNavController
import com.snmc.scanner.R
import com.snmc.scanner.data.db.AppDatabase
import com.snmc.scanner.data.db.entities.Authentication
import com.snmc.scanner.data.db.entities.Organization
import com.snmc.scanner.data.network.AuthenticateApi
import com.snmc.scanner.data.network.LoginApi
import com.snmc.scanner.data.repositories.AuthenticateRepository
import com.snmc.scanner.data.repositories.LoginRepository
import com.snmc.scanner.databinding.LoginFragmentBinding
import com.snmc.scanner.models.Error
import com.snmc.scanner.utils.*
import com.snmc.scanner.utils.ApiErrorCodes.GENERAL_ERROR
import com.snmc.scanner.utils.ApiErrorCodes.NOT_FOUND_IN_SQL_DATABASE
import com.snmc.scanner.utils.AppErrorCodes.EMPTY_USERNAME
import com.snmc.scanner.utils.AppErrorCodes.EMPTY_PASSWORD
import com.snmc.scanner.utils.AppErrorCodes.NULL_AUTHENTICATION_RESPONSE
import com.snmc.scanner.utils.AppErrorCodes.NULL_LOGIN_RESPONSE
import kotlinx.android.synthetic.main.login_fragment.*

class LoginFragment : Fragment(), LoginListener {

    // Initialize ViewModel
    private lateinit var viewModel: LoginViewModel

    override fun onCreateView(inflater: LayoutInflater, container: ViewGroup?,
                              savedInstanceState: Bundle?): View? {

        // Initialize APIs, DB, and Repositories
        val loginApi = LoginApi(baseUrl = getLoginBaseUrl())
        val authenticateApi = AuthenticateApi(baseUrl = getAuthenticateBaseUrl())
        val db = AppDatabase(requireActivity())
        val loginRepository = LoginRepository(loginApi, db)
        val authenticateRepository = AuthenticateRepository(authenticateApi, db)
        val loginViewModelFactory = LoginViewModelFactory(requireActivity().application, loginRepository, authenticateRepository)

        // Binding object that connects to the layout
        val binding : LoginFragmentBinding = LoginFragmentBinding.inflate(inflater, container, false)

        // ViewModel
        viewModel = ViewModelProvider(this, loginViewModelFactory).get(LoginViewModel::class.java)

        // Set ViewModel on Binding object
        binding.viewmodel = viewModel

        // Set LifecycleOwner on Binding object
        binding.lifecycleOwner = this

        // Fragment implements methods defined in LoginListener which are called by ViewModel
        viewModel.loginListener = this

        // Check if an Organization is already logged in
        viewModel.getSavedOrganization().observe(viewLifecycleOwner, Observer { organization ->
            if (organization != null) {
                navigate()
            }
        })

        // Return the View at the Root of the Binding object
        return binding.root
    }

    // Needed for Init
    private fun getLoginBaseUrl(): String {
        return "${getString(R.string.login_base_url)}/"
    }

    // Needed for Init
    private fun getAuthenticateBaseUrl(): String {
        val tenantId : String = getTenantId()

        return "${getString(R.string.authentication_base_url)}/$tenantId/"
    }

    // Needed for Init
    private fun getTenantId() : String {
        return getString(R.string.tenant_id)
    }

    override fun onStarted() {
        disableUi()
        removeError()
    }

    override fun onLoginSuccess(organization: Organization) {
        Log.d("Organization", "Organization Id: ${organization.id}, Organization Name: ${organization.name}, Username: ${organization.username}")
    }

    override fun onAuthenticateSuccess(authentication: Authentication) {
        Log.d("Organization", "Token Type: ${authentication.tokenType}, Expires In: ${authentication.expiresIn}, Ext Expires In: ${authentication.extExpiresIn}")
    }

    override fun onFailure(error: Error) {
        enableUi()
        setError(error)
        Log.e("Error Message", "${error.code}: ${error.message}")
    }

    private fun disableUi() {
        login_progress_indicator.show()
        login_button.disable()
    }

    private fun enableUi() {
        login_progress_indicator.hide()
        login_button.enable()
    }

    private fun setError(error: Error) {
        var showUsernameError: Boolean = false
        var showPasswordError: Boolean = false
        var showErrorMessage: Boolean = false

        var usernameErrorMessage: String? = null
        var passwordErrorMessage: String? = null
        var errorMessageText: String? = null

        when (error.code) {
            EMPTY_USERNAME.code -> {
                showUsernameError = true
                usernameErrorMessage = EMPTY_USERNAME.message
            }
            EMPTY_PASSWORD.code -> {
                showPasswordError = true
                passwordErrorMessage = EMPTY_PASSWORD.message
            }
            NULL_LOGIN_RESPONSE.code -> {
                showErrorMessage = true
                errorMessageText = NULL_LOGIN_RESPONSE.message
            }
            NULL_AUTHENTICATION_RESPONSE.code -> {
                showErrorMessage = true
                errorMessageText = NULL_AUTHENTICATION_RESPONSE.message
            }
            NOT_FOUND_IN_SQL_DATABASE.code -> {
                showErrorMessage = true
                errorMessageText = NOT_FOUND_IN_SQL_DATABASE.message
            }
            GENERAL_ERROR.code -> {
                showErrorMessage = true
                errorMessageText = GENERAL_ERROR.message
            }
            else -> {
                // This state means the error is unaccounted for
                showUsernameError = false
                Log.e("Error Message", "${error.code}: ${error.message}")
            }
        }

        if (showUsernameError && usernameErrorMessage != null) {
            username.showError(usernameErrorMessage)
        }
        if (showPasswordError && passwordErrorMessage != null) {
            password.showError(passwordErrorMessage)
        }
        if (showErrorMessage && errorMessageText != null) {
            login_error_indicator.showError(errorMessageText)
        }
    }

    private fun removeError() {
        username.hideError()
        password.hideError()
        login_error_indicator.hideError()
    }

    private fun navigate() {
        val action = LoginFragmentDirections.actionLoginFragmentToSettingsFragment()
        this.findNavController().navigate(action)
    }
}