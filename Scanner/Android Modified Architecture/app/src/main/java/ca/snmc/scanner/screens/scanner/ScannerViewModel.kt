package ca.snmc.scanner.screens.scanner

import android.app.Application
import androidx.lifecycle.AndroidViewModel
import androidx.lifecycle.LiveData
import ca.snmc.scanner.R
import ca.snmc.scanner.data.db.entities.AuthenticationEntity
import ca.snmc.scanner.data.db.entities.OrganizationEntity
import ca.snmc.scanner.data.db.entities.VisitEntity
import ca.snmc.scanner.data.preferences.PreferenceProvider
import ca.snmc.scanner.data.repositories.AuthenticateRepository
import ca.snmc.scanner.data.repositories.BackEndRepository
import ca.snmc.scanner.data.repositories.LoginRepository
import ca.snmc.scanner.models.AuthenticateInfo
import ca.snmc.scanner.models.LoginInfo
import ca.snmc.scanner.models.VisitInfo
import ca.snmc.scanner.utils.*
import ca.snmc.scanner.utils.BackEndApiUtils.generateAuthorization

class ScannerViewModel (
    application: Application,
    private val loginRepository: LoginRepository,
    private val authenticateRepository: AuthenticateRepository,
    private val backEndRepository: BackEndRepository,
    private val prefs: PreferenceProvider
) : AndroidViewModel(application) {

    private lateinit var authentication : AuthenticationEntity
    private lateinit var organization : OrganizationEntity
    private lateinit var visitSettings : LiveData<VisitEntity>

    val visitInfo : VisitInfo = VisitInfo(null, null, null, null)

    fun initialize() {
        getSavedAuthentication()
        getSavedOrganization()
        getSavedVisitSettings()
    }

    suspend fun logVisit() {

        // Check if there is a need to authenticate
        if (isAccessTokenExpired(authentication.expireTime!!)) {
            val loginResponse = loginRepository.scannerLogin(LoginInfo(
                    username = organization.username!!,
                    password = organization.password!!
                ))

            if (loginResponse.isNotNull()) {

                val authenticateInfo = AuthenticateInfo(
                    grantType = AuthApiUtils.getGrantType(),
                    clientId = loginResponse.clientId!!,
                    clientSecret = loginResponse.clientSecret!!,
                    scope = AuthApiUtils.getScope(scopePrefix = getScopePrefix())
                )
                val authenticateResponse = authenticateRepository.scannerAuthenticate(authenticateInfo = authenticateInfo)
                if (authenticateResponse.isNotNull()) {
                    // Map AuthenticationResponse to AuthenticationEntity
                    val authentication = mapAuthenticateResponseToAuthenticationEntity(authenticateResponse)
                    // Store AuthenticationEntity in DB
                    authenticateRepository.saveAuthentication(authentication)
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

        // Continue with request
        backEndRepository.logVisit(
            authorization = generateAuthorization(authentication.accessToken!!),
            visitInfo = visitInfo
        )
    }

    private fun getSavedVisitSettings() {
        visitSettings = backEndRepository.getSavedVisitSettings()
    }

    fun getVisitSettings() = visitSettings

    private fun getSavedAuthentication() {
        authentication = backEndRepository.getSavedAuthentication()
    }

    private fun getSavedOrganization() {
        organization = backEndRepository.getSavedOrganization()
    }

    fun writeInternetIsNotAvailable() = prefs.writeInternetIsNotAvailable()

    private fun getScopePrefix() : String = getApplication<Application>().applicationContext.getString(
        R.string.backend_base_url)

}