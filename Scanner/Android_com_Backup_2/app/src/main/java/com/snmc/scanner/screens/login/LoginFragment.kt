package com.snmc.scanner.screens.login

import android.os.Bundle
import android.util.Log
import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import androidx.fragment.app.Fragment
import androidx.lifecycle.Observer
import androidx.lifecycle.ViewModelProvider
import androidx.navigation.fragment.findNavController
import com.snmc.scanner.data.db.entities.AuthenticationEntity
import com.snmc.scanner.data.db.entities.OrganizationEntity
import com.snmc.scanner.databinding.LoginFragmentBinding
import com.snmc.scanner.models.Error
import com.snmc.scanner.utils.*
import com.snmc.scanner.utils.ApiErrorCodes.GENERAL_ERROR
import com.snmc.scanner.utils.ApiErrorCodes.NOT_FOUND_IN_SQL_DATABASE
import com.snmc.scanner.utils.AppErrorCodes.EMPTY_PASSWORD
import com.snmc.scanner.utils.AppErrorCodes.EMPTY_USERNAME
import com.snmc.scanner.utils.AppErrorCodes.NO_INTERNET
import com.snmc.scanner.utils.AppErrorCodes.NULL_AUTHENTICATION_RESPONSE
import com.snmc.scanner.utils.AppErrorCodes.NULL_LOGIN_RESPONSE
import kotlinx.android.synthetic.main.login_fragment.*
import org.kodein.di.KodeinAware
import org.kodein.di.android.x.kodein
import org.kodein.di.generic.instance

class LoginFragment : Fragment(), LoginListener, KodeinAware {

    override val kodein by kodein()
    private val loginViewModelFactory : LoginViewModelFactory by instance()

    override fun onCreateView(inflater: LayoutInflater, container: ViewGroup?,
                              savedInstanceState: Bundle?): View? {

        // Binding object that connects to the layout
        val binding : LoginFragmentBinding = LoginFragmentBinding.inflate(inflater, container, false)

        // ViewModel
        val viewModel = ViewModelProvider(this, loginViewModelFactory).get(LoginViewModel::class.java)

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

    override fun onStarted() {
        disableUi()
        removeError()
    }

    override fun onLoginSuccess(organizationEntity: OrganizationEntity) {
        Log.d("Organization", "Organization Id: ${organizationEntity.id}, Organization Name: ${organizationEntity.name}, Username: ${organizationEntity.username}")
    }

    override fun onAuthenticateSuccess(authenticationEntity: AuthenticationEntity) {
        Log.d("Organization", "Token Type: ${authenticationEntity.tokenType}, Expires In: ${authenticationEntity.expiresIn}, Ext Expires In: ${authenticationEntity.extExpiresIn}")
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
            NO_INTERNET.code -> {
                showErrorMessage = true
                errorMessageText = NO_INTERNET.message
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