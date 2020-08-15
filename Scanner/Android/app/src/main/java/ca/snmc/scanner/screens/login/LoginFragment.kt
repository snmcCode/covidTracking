package ca.snmc.scanner.screens.login

import android.os.Bundle
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

    override fun onResume() {
        super.onResume()

        (activity as MainActivity).windowedMode()
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
                } catch (e: ConnectionTimeoutException) {
                    val error = mapErrorStringToError(e.message!!)
                    onFailure(error)
                } catch (e: AppException) {
                    val error = mapErrorStringToError(e.message!!)
                    onFailure(error)
                }
            }
        }

    }

    private fun validateLoginFields(username: String, password: String) {
        if (username.isEmpty()) {
            onFailure(error = AppErrorCodes.EMPTY_USERNAME)
        }
        if (password.isEmpty()) {
            onFailure(error = AppErrorCodes.EMPTY_PASSWORD)
        }
    }

    private fun onStarted() {
        disableUi()
        removeError()
    }

    private fun onFailure(error: Error) {
        enableUi()
        setError(error)
//        Log.e("Error Message", "${error.code}: ${error.message}")
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
            AppErrorCodes.EMPTY_USERNAME.code -> {
                showUsernameError = true
                usernameErrorMessage = AppErrorCodes.EMPTY_USERNAME.message
            }
            AppErrorCodes.EMPTY_PASSWORD.code -> {
                showPasswordError = true
                passwordErrorMessage = AppErrorCodes.EMPTY_PASSWORD.message
            }
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
            ApiErrorCodes.USER_NOT_FOUND_IN_SQL_DATABASE.code -> {
                showErrorMessage = true
                errorMessageText = ApiErrorCodes.USER_NOT_FOUND_IN_SQL_DATABASE.message
            }
            ApiErrorCodes.GENERAL_ERROR.code -> {
                showErrorMessage = true
                errorMessageText = ApiErrorCodes.GENERAL_ERROR.message
            }
            else -> {
                // This state means the error is unaccounted for
                showErrorMessage = false
//                Log.e("Error Message", "${error.code}: ${error.message}")
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