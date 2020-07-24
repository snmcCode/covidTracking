package ca.snmc.scanner.screens.login

import android.os.Bundle
import android.util.Log
import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import androidx.fragment.app.Fragment
import androidx.lifecycle.Observer
import androidx.lifecycle.ViewModelProvider
import androidx.lifecycle.lifecycleScope
import androidx.navigation.fragment.findNavController
import ca.snmc.scanner.MainActivity
import ca.snmc.scanner.databinding.LoginFragmentBinding
import ca.snmc.scanner.models.Error
import ca.snmc.scanner.utils.*
import ca.snmc.scanner.utils.ApiErrorCodes.GENERAL_ERROR
import ca.snmc.scanner.utils.ApiErrorCodes.NOT_FOUND_IN_SQL_DATABASE
import ca.snmc.scanner.utils.AppErrorCodes.EMPTY_PASSWORD
import ca.snmc.scanner.utils.AppErrorCodes.EMPTY_USERNAME
import ca.snmc.scanner.utils.AppErrorCodes.NO_INTERNET
import ca.snmc.scanner.utils.AppErrorCodes.NULL_AUTHENTICATION_RESPONSE
import ca.snmc.scanner.utils.AppErrorCodes.NULL_LOGIN_RESPONSE
import kotlinx.android.synthetic.main.login_fragment.*
import kotlinx.coroutines.launch
import org.kodein.di.KodeinAware
import org.kodein.di.android.x.kodein
import org.kodein.di.generic.instance

class LoginFragment : Fragment(), KodeinAware {

    override val kodein by kodein()
    private val loginViewModelFactory : LoginViewModelFactory by instance()

    private lateinit var binding : LoginFragmentBinding
    private lateinit var viewModel : LoginViewModel

    private var isSuccess = true

    override fun onCreateView(inflater: LayoutInflater, container: ViewGroup?,
                              savedInstanceState: Bundle?): View? {

        (activity as MainActivity).windowedMode()

        // Binding object that connects to the layout
        binding = LoginFragmentBinding.inflate(inflater, container, false)

        // Set LifecycleOwner on Binding object
        binding.lifecycleOwner = this

        binding.loginButton.setOnClickListener {
            handleScannerLogin()
        }

        // ViewModel
        viewModel = ViewModelProvider(this, loginViewModelFactory).get(LoginViewModel::class.java)

        // Waiting until Authentication info is in DB before navigating
        viewModel.getSavedAuthentication().observe(viewLifecycleOwner, Observer { authentication ->
            if (authentication != null) {
                navigate()
            }
        })

        // Return the View at the Root of the Binding object
        return binding.root
    }

    private fun handleScannerLogin() {
        // Reset success flag
        isSuccess = true

        onStarted()

        val username: String = binding.username.text.toString().trim()
        val password: String = binding.password.text.toString().trim()
        validateLoginFields(username, password)

        if (isSuccess) {
            // This is called in a Coroutine in order to allow for exception handling
            viewLifecycleOwner.lifecycleScope.launch {
                try {
                    viewModel.scannerLoginAndAuthenticate(username, password)
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

    }

    private fun validateLoginFields(username: String, password: String) {
        if (username.isEmpty()) {
            onFailure(error = EMPTY_USERNAME)
        }
        if (password.isEmpty()) {
            onFailure(error = EMPTY_PASSWORD)
        }
    }

    private fun onStarted() {
        disableUi()
        removeError()
    }

    private fun onFailure(error: Error) {
        enableUi()
        setError(error)
        Log.e("Error Message", "${error.code}: ${error.message}")
        isSuccess = false
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
        var showUsernameError = false
        var showPasswordError = false
        var showErrorMessage = false

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
                showErrorMessage = false
                Log.e("Error Message", "${error.code}: ${error.message}")
            }
        }

        if (showUsernameError && usernameErrorMessage != null) {
            username_layout.showError(usernameErrorMessage)
        }
        if (showPasswordError && passwordErrorMessage != null) {
            password_field.showError(passwordErrorMessage)
        }
        if (showErrorMessage && errorMessageText != null) {
            login_error_indicator.showError(errorMessageText)
        }
    }

    private fun removeError() {
        username_layout.hideError()
        password_field.hideError()
        login_error_indicator.hideError()
    }

    private fun navigate() {
        val action = LoginFragmentDirections.actionLoginFragmentToSettingsFragment()
        this.findNavController().navigate(action)
    }
}