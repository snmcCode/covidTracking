package ca.snmc.scanner.screens.login

import android.app.Application
import androidx.lifecycle.AndroidViewModel
import ca.snmc.scanner.R
import ca.snmc.scanner.data.network.responses.AuthenticateResponse
import ca.snmc.scanner.data.network.responses.LoginResponse
import ca.snmc.scanner.data.providers.PreferenceProvider
import ca.snmc.scanner.data.repositories.AuthenticateRepository
import ca.snmc.scanner.data.repositories.DeviceInformationRepository
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
    private val deviceInformationRepository: DeviceInformationRepository,
    private val prefs: PreferenceProvider
) : AndroidViewModel(application) {

    suspend fun scannerLoginAndAuthenticate(username: String, password: String) {

        val scannerMode = prefs.readScannerMode()

        val loginInfo = LoginInfo(username, password)

        // Selection based on Scanner Mode
        val loginResponse : LoginResponse = if (scannerMode == TESTING_MODE) {
            loginRepository.scannerLoginTesting(loginInfo)
        } else {
            loginRepository.scannerLoginProduction(loginInfo)
        }

        if (loginResponse.isNotNull()) {
            // Map LoginResponse to OrganizationEntity
            val organization = mapLoginToOrganizationEntity(loginResponse, loginInfo)
            // Store OrganizationEntity in DB
            loginRepository.saveOrganization(organization)
            // Set User is Logged In Flag to True SharedPrefs
            prefs.writeUserIsLoggedIn()
            // Set Is Internet Available Flag to True in SharedPrefs Due to Successful API Call
            prefs.writeInternetIsAvailable()

            // Selection based on Scanner Mode
            val scopePrefix : String = if (scannerMode == TESTING_MODE) {
                getScopePrefixTesting()
            } else {
                getScopePrefixProduction()
            }

            val authenticateInfo = AuthenticateInfo(
                grantType = getGrantType(),
                clientId = loginResponse.clientId!!,
                clientSecret = loginResponse.clientSecret!!,
                scope = getScope(scopePrefix)
            )

            // Selection based on Scanner Mode
            val authenticateResponse : AuthenticateResponse = if (scannerMode == TESTING_MODE) {
                authenticateRepository.scannerAuthenticateTesting(authenticateInfo = authenticateInfo)
            } else {
                authenticateRepository.scannerAuthenticateProduction(authenticateInfo = authenticateInfo)
            }

            if (authenticateResponse.isNotNull()) {
                // Map AuthenticationResponse to AuthenticationEntity
                val authentication = mapAuthenticateResponseToAuthenticationEntity(authenticateResponse)
                // Store AuthenticationEntity in DB
                authenticateRepository.saveAuthentication(authentication)
                // Set Is Internet Available Flag to True in SharedPrefs Due to Successful API Call
                prefs.writeInternetIsAvailable()
            } else {
                val errorMessage = "${AppErrorCodes.NULL_AUTHENTICATION_RESPONSE.code}: ${AppErrorCodes.NULL_AUTHENTICATION_RESPONSE.message}"
                throw AuthenticationException(errorMessage)
            }

        } else {
            val errorMessage = "${AppErrorCodes.NULL_LOGIN_RESPONSE.code}: ${AppErrorCodes.NULL_LOGIN_RESPONSE.message}"
            throw AuthenticationException(errorMessage)
        }

    }

    fun writeInternetIsNotAvailable() = prefs.writeInternetIsNotAvailable()

    // Expose AuthenticationObject to UI for observing
    fun getSavedAuthentication() = authenticateRepository.getSavedAuthentication()

    private fun getScopePrefixProduction() : String = getApplication<Application>().applicationContext.getString(R.string.backend_base_url_production)

    private fun getScopePrefixTesting() : String = getApplication<Application>().applicationContext.getString(R.string.backend_base_url_testing)

    fun getDeviceId() = deviceInformationRepository.getDeviceId()

}