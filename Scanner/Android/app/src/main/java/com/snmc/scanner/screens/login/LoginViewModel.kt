package com.snmc.scanner.screens.login

import android.app.Application
import android.util.Log
import android.view.View
import androidx.lifecycle.AndroidViewModel
import com.snmc.scanner.R
import com.snmc.scanner.data.repositories.AuthenticateRepository
import com.snmc.scanner.data.repositories.LoginRepository
import com.snmc.scanner.models.AuthenticateInfo
import com.snmc.scanner.models.LoginInfo
import com.snmc.scanner.utils.*

// TODO: Implement Interceptor to handle case of NoInternet --> Resume from Video 9

class LoginViewModel(
    application: Application,
    private val loginRepository: LoginRepository,
    private val authenticateRepository: AuthenticateRepository
) : AndroidViewModel(application) {

    // Fields connected to layout
    // TODO: Can these be abstracted?
    var username : String? = null
    var password : String? = null

    // Initialize LoginListener
    var loginListener : LoginListener? = null

    fun getSavedOrganization() = loginRepository.getSavedOrganization()

    // onClick called by layout, which calls the methods that the LoginFragment has implemented from LoginListener
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

                    // Save Organization
                    val organization = mapLoginToOrganization(loginResponse, loginInfo)
                    loginRepository.saveOrganization(organization)
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

                        // Save Authentication
                        val authentication = mapAuthenticationResponseToAuthentication(authenticateResponse)
                        authenticateRepository.saveAuthentication(authentication)
                        loginListener?.onAuthenticateSuccess(authentication)

                    } else {
                        loginListener?.onFailure(AppErrorCodes.NULL_AUTHENTICATION_RESPONSE)
                    }

                } else {
                    loginListener?.onFailure(AppErrorCodes.NULL_LOGIN_RESPONSE)
                }

            } catch (e: ApiException) {
                val error = mapErrorStringToError(e.message!!)
                loginListener?.onFailure(error)
            }
        }

        // TODO: Add Room database to store retrieved info
    }

    private fun getGrantType() : String {
        return getApplication<Application>().resources.getString(R.string.grant_type)
    }

    private fun getScopeSuffix() : String {
        return getApplication<Application>().resources.getString(R.string.scope_suffix)
    }

    private fun getScope(clientId: String) : String {
        val scopeSuffix : String = getScopeSuffix()

        return "$clientId/$scopeSuffix"
    }

}