package ca.snmc.scanner.screens.login

import android.app.Application
import android.util.Log
import android.view.View
import androidx.lifecycle.AndroidViewModel
import ca.snmc.scanner.data.preferences.PreferenceProvider
import ca.snmc.scanner.data.repositories.AuthenticateRepository
import ca.snmc.scanner.data.repositories.LoginRepository
import ca.snmc.scanner.models.AuthenticateInfo
import ca.snmc.scanner.models.LoginInfo
import ca.snmc.scanner.utils.*
import ca.snmc.scanner.utils.AuthApiUtils.getGrantType
import ca.snmc.scanner.utils.AuthApiUtils.getScope

class LoginViewModel(
    application: Application,
    private val loginRepository: LoginRepository,
    private val authenticateRepository: AuthenticateRepository,
    private val prefs: PreferenceProvider
) : AndroidViewModel(application) {

    // Fields connected to layout
    // TODO: Can these be made private with Getter and Setter?
    var username : String? = null
    var password : String? = null

    // Initialize LoginListener
    var loginListener : LoginListener? = null

    fun getSavedOrganization() = loginRepository.getSavedOrganization()

    fun getSavedAuthentication() = authenticateRepository.getSavedAuthentication()

    // onClick called by layout, which also calls the methods that the LoginFragment has implemented from LoginListener
    fun onLoginButtonClick(view: View) {
        loginListener?.onStarted()
        Log.d("Username", username.toString())
        Log.d("Password", password.toString())
        if (username.isNullOrEmpty() || password.isNullOrEmpty()) {
            if (username.isNullOrEmpty()) {
                loginListener?.onFailure(AppErrorCodes.EMPTY_USERNAME)
            }
            if (password.isNullOrEmpty()) {
                loginListener?.onFailure(AppErrorCodes.EMPTY_PASSWORD)
            }
            return
        }

        // Create Object for Logging In
        val loginInfo = LoginInfo(username!!, password!!)

        // Invoke Async API Call as a Coroutine
        Coroutines.main {
            // Call the API from the Repository, Always Pass in the Base URL from the ViewModel
            try {

                // Call LoginApi
                val loginResponse = loginRepository.scannerLogin(loginInfo = loginInfo)

                // Process LoginApi Response
                if (loginResponse.isNotNull()) {

                    // Map OrganizationResponse to OrganizationEntity
                    val organization = mapLoginToOrganizationEntity(loginResponse, loginInfo)
                    // Store OrganizationEntity in DB
                    loginRepository.saveOrganization(organization)
                    // Set User is Logged In Flag to True SharedPrefs
                    prefs.writeUserIsLoggedIn()
                    // Set Is Internet Available Flag to True in SharedPrefs
                    prefs.writeInternetIsAvailable()
                    // Indicate User Login to UI
                    loginListener?.onLoginSuccess(organization)

                    // Create Object for Authenticating
                    val authenticateInfo = AuthenticateInfo(
                        grantType = getGrantType(),
                        clientId = loginResponse.clientId!!,
                        clientSecret = loginResponse.clientSecret!!,
                        scope = getScope(loginResponse.clientId)
                    )

                    // Call AuthenticateAPI
                    val authenticateResponse = authenticateRepository.scannerAuthenticate(authenticateInfo = authenticateInfo)

                    // Process AuthenticateAPI Response
                    if (authenticateResponse.isNotNull()) {

                        // Map AuthenticationResponse to AuthenticationEntity
                        val authentication = mapAuthenticationResponseToAuthenticationEntity(authenticateResponse)
                        // Store AuthenticationEntity in DB
                        authenticateRepository.saveAuthentication(authentication)
                        // Set Token Expiry Time in SharedPrefs
                        prefs.writeAuthTokenExpiryTime(getAccessTokenExpiryTime(authentication.expiresIn!!))
                        // Set Is Internet Available Flag to True in SharedPrefs
                        prefs.writeInternetIsAvailable()
                        // Indicate Authentication Success to UI
                        loginListener?.onAuthenticateSuccess(authentication)

                    } else {
                        // Notify UI of Login Failure
                        loginListener?.onFailure(AppErrorCodes.NULL_AUTHENTICATION_RESPONSE)
                    }

                } else {
                    // Notify UI of Login Failure
                    loginListener?.onFailure(AppErrorCodes.NULL_LOGIN_RESPONSE)
                }

            } catch (e: ApiException) {
                // Map Error
                val error = mapErrorStringToError(e.message!!)
                // Notify UI of Login Failure
                loginListener?.onFailure(error)
            } catch (e: NoInternetException) {
                // Map Error
                val error = mapErrorStringToError(e.message!!)
                // Notify UI of Login Failure
                loginListener?.onFailure(error)
                // Set Is Internet Available Flag to False in SharedPrefs
                prefs.writeInternetIsNotAvailable()
            }
        }
    }

}