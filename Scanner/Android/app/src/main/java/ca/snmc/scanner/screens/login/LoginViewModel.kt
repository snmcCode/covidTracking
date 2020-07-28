package ca.snmc.scanner.screens.login

import android.app.Application
import androidx.lifecycle.AndroidViewModel
import ca.snmc.scanner.R
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

    suspend fun scannerLoginAndAuthenticate(username: String, password: String) {

        val loginInfo = LoginInfo(username, password)
        val loginResponse = loginRepository.scannerLogin(loginInfo)
        if (loginResponse.isNotNull()) {
            // Map LoginResponse to OrganizationEntity
            val organization = mapLoginToOrganizationEntity(loginResponse, loginInfo)
            // Store OrganizationEntity in DB
            loginRepository.saveOrganization(organization)
            // Set User is Logged In Flag to True SharedPrefs
            prefs.writeUserIsLoggedIn()
            // Set Is Internet Available Flag to True in SharedPrefs Due to Successful API Call
            prefs.writeInternetIsAvailable()

            val authenticateInfo = AuthenticateInfo(
                grantType = getGrantType(),
                clientId = loginResponse.clientId!!,
                clientSecret = loginResponse.clientSecret!!,
                scope = getScope(scopePrefix = getScopePrefix())
            )
            val authenticateResponse = authenticateRepository.scannerAuthenticate(authenticateInfo = authenticateInfo)
            if (authenticateResponse.isNotNull()) {
                // Map AuthenticationResponse to AuthenticationEntity
                val authentication = mapAuthenticationResponseToAuthenticationEntity(authenticateResponse)
                // Store AuthenticationEntity in DB
                authenticateRepository.saveAuthentication(authentication)
                // Set Token Expiry Time in SharedPrefs
                prefs.writeAuthTokenExpiryTime(getAccessTokenExpiryTime(authentication.expiresIn!!))
                // Set Is Internet Available Flag to True in SharedPrefs Due to Successful API Call
                prefs.writeInternetIsAvailable()
            } else {
                val errorMessage = "${AppErrorCodes.NULL_AUTHENTICATION_RESPONSE.code}: ${AppErrorCodes.NULL_AUTHENTICATION_RESPONSE.message}"
                throw AppException(errorMessage)
            }

        } else {
            val errorMessage = "${AppErrorCodes.NULL_LOGIN_RESPONSE.code}: ${AppErrorCodes.NULL_LOGIN_RESPONSE.message}"
            throw AppException(errorMessage)
        }

    }

    fun writeInternetIsNotAvailable() = prefs.writeInternetIsNotAvailable()

    // Expose AuthenticationObject to UI for observing
    fun getSavedAuthentication() = authenticateRepository.getSavedAuthentication()

    private fun getScopePrefix() : String = getApplication<Application>().applicationContext.getString(R.string.backend_base_url)

}