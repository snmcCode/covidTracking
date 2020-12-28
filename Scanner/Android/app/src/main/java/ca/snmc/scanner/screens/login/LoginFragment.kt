package ca.snmc.scanner.screens.login

import android.Manifest
import android.content.pm.PackageManager
import android.os.Bundle
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

    private lateinit var username: String
    private lateinit var password: String

    private var isSuccess = true
    private var isPermissionGranted = false
    private val permissionsRequestCode = 1001

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
                disableUi()
                navigateToSettingsPage()
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

        username = binding.username.text.toString().trim()
        password = binding.password.text.toString().trim()
        validateLoginFields(username, password)

        if (isSuccess) {
            viewLifecycleOwner.lifecycleScope.launch {
                activity?.let {
                    if (permissionGranted()) {
                        onPermissionGranted()
                    } else {
                        requestPermissions(arrayOf(Manifest.permission.ACCESS_FINE_LOCATION), permissionsRequestCode)
                    }
                }
            }
        }

    }

    private fun onPermissionGranted() {
        // This is called in a Coroutine in order to allow for exception handling
        viewLifecycleOwner.lifecycleScope.launch {
            try {
                viewModel.scannerLoginAndAuthenticate(username, password)
            } catch (e: ApiException) {
                val error = mapErrorStringToError(e.message!!)
                logError(
                    exception = e,
                    functionName = "onPermissionGranted",
                    errorMessage = error.message!!,
                    issue = "API returned error code during login attempt."
                )
                onFailure(error)
            } catch (e: NoInternetException) {
                val error = mapErrorStringToError(e.message!!)
                logError(
                    exception = e,
                    functionName = "onPermissionGranted",
                    errorMessage = error.message!!,
                    issue = "No internet connection during login attempt."
                )
                onFailure(error)
                viewModel.writeInternetIsNotAvailable()
            } catch (e: ConnectionTimeoutException) {
                val error = mapErrorStringToError(e.message!!)
                logError(
                    exception = e,
                    functionName = "onPermissionGranted",
                    errorMessage = error.message!!,
                    issue = "Connection timed out or connection error occurred during login attempt."
                )
                onFailure(error)
            } catch (e: AuthenticationException) {
                val error = mapErrorStringToError(e.message!!)
                logError(
                    exception = e,
                    functionName = "onPermissionGranted",
                    errorMessage = error.message!!,
                    issue = "Error occurred during authentication attempt."
                )
                onFailure(error)
            }
        }
    }

    override fun onRequestPermissionsResult(
        requestCode: Int,
        permissions: Array<String>,
        grantResults: IntArray
    ) {
        if (requestCode == permissionsRequestCode) {
            if ((permissions[0] == Manifest.permission.ACCESS_FINE_LOCATION && grantResults[0] == PackageManager.PERMISSION_GRANTED)) {
                // Permission Granted
                onPermissionGranted()
            } else {
                // Permission Denied
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
        Manifest.permission.ACCESS_FINE_LOCATION
    )

    private fun permissionGranted() = ContextCompat.checkSelfPermission(
        requireActivity(),
        Manifest.permission.ACCESS_FINE_LOCATION
    ) == PackageManager.PERMISSION_GRANTED

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

    private fun onPermissionsFailure(error: Error) {
        enableUi()
        setError(error)
//         Log.e("Error Message", "${error.code}: ${error.message}")
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
            AppErrorCodes.PERMISSIONS_NOT_GRANTED.code -> {
                showErrorMessage = true
                errorMessageText = AppErrorCodes.PERMISSIONS_NOT_GRANTED.message
            }
            AppErrorCodes.PERMISSIONS_NOT_GRANTED_NEVER_ASK_AGAIN.code -> {
                showErrorMessage = true
                errorMessageText = AppErrorCodes.PERMISSIONS_NOT_GRANTED_NEVER_ASK_AGAIN.message
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

    private fun navigateToSettingsPage() {
        val action = LoginFragmentDirections.actionLoginFragmentToSettingsFragment()
        this.findNavController().navigate(action)
    }

    @Suppress("SameParameterValue")
    private fun logError(exception: Exception, functionName: String, errorMessage: String, issue: String) {
        (requireActivity() as MainActivity).logError(
            exception = exception,
            properties = mapOf(
                Pair("Device ID", viewModel.getDeviceId()),
                Pair("Filename", "LoginFragment.kt"),
                Pair("Function Name", functionName),
                Pair("Error Message", errorMessage),
                Pair("Issue", issue)
            ),
            attachments = null
        )
    }

}