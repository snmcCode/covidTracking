package ca.snmc.scanner.screens.scanner

import android.app.Application
import androidx.lifecycle.AndroidViewModel
import androidx.lifecycle.LiveData
import androidx.lifecycle.MediatorLiveData
import androidx.lifecycle.MutableLiveData
import ca.snmc.scanner.R
import ca.snmc.scanner.data.db.entities.AuthenticationEntity
import ca.snmc.scanner.data.db.entities.OrganizationEntity
import ca.snmc.scanner.data.db.entities.VisitEntity
import ca.snmc.scanner.data.network.responses.AuthenticateResponse
import ca.snmc.scanner.data.network.responses.LoginResponse
import ca.snmc.scanner.data.preferences.PreferenceProvider
import ca.snmc.scanner.data.repositories.AuthenticateRepository
import ca.snmc.scanner.data.repositories.BackEndRepository
import ca.snmc.scanner.data.repositories.LoginRepository
import ca.snmc.scanner.models.AuthenticateInfo
import ca.snmc.scanner.models.LoginInfo
import ca.snmc.scanner.models.ScanHistoryItem
import ca.snmc.scanner.models.VisitInfo
import ca.snmc.scanner.utils.*
import ca.snmc.scanner.utils.BackEndApiUtils.generateAuthorization
import java.util.*
import kotlin.collections.ArrayList

class ScannerViewModel (
    application: Application,
    private val loginRepository: LoginRepository,
    private val authenticateRepository: AuthenticateRepository,
    private val backEndRepository: BackEndRepository,
    private val prefs: PreferenceProvider
) : AndroidViewModel(application) {

    private lateinit var organization : LiveData<OrganizationEntity>
    private lateinit var authentication : LiveData<AuthenticationEntity>
    private lateinit var mergedData : MediatorLiveData<CombinedOrgAuthData>

    private lateinit var visitSettings : LiveData<VisitEntity>
    val visitInfo : VisitInfo = VisitInfo(null, null, null, null, null)

    var recentScanCode : UUID? = null

    var scanHistory : MutableList<ScanHistoryItem> = ArrayList()

    val scanHistoryObservable : MutableLiveData<MutableList<ScanHistoryItem>> = MutableLiveData()

    fun initialize() {
        getSavedVisitSettings()
        getSavedAuthentication()
        getSavedOrganization()
        mergedData = MediatorLiveData()
        mergedData.addSource(organization) {
            mergedData.value = CombinedOrgAuthData(
                id = organization.value?.id,
                authorization = authentication.value?.accessToken,
                username = organization.value?.username,
                password = organization.value?.password
            )
        }
        mergedData.addSource(authentication) {
            mergedData.value = CombinedOrgAuthData(
                id = organization.value?.id,
                authorization = authentication.value?.accessToken,
                username = organization.value?.username,
                password = organization.value?.password
            )
        }
    }

    suspend fun logVisit() {

        val scannerMode = prefs.readScannerMode()

        // Check access token
        if (isAccessTokenExpired(authentication.value!!.expireTime!!)) {

            recentScanCode = visitInfo.visitorId

            // Selection based on Scanner Mode
            val loginResponse : LoginResponse = if (scannerMode == TESTING_MODE) {
                loginRepository.scannerLoginTesting(LoginInfo(
                    username = organization.value!!.username!!,
                    password = organization.value!!.password!!
                ))
            } else {
                loginRepository.scannerLoginProduction(LoginInfo(
                    username = organization.value!!.username!!,
                    password = organization.value!!.password!!
                ))
            }

            if (loginResponse.isNotNull()) {

                // Set Is Internet Available Flag to True in SharedPrefs Due to Successful API Call
                prefs.writeInternetIsAvailable()

                // Selection based on Scanner Mode
                val scopePrefix : String = if (scannerMode == TESTING_MODE) {
                    getScopePrefixTesting()
                } else {
                    getScopePrefixProduction()
                }

                val authenticateInfo = AuthenticateInfo(
                    grantType = AuthApiUtils.getGrantType(),
                    clientId = loginResponse.clientId!!,
                    clientSecret = loginResponse.clientSecret!!,
                    scope = AuthApiUtils.getScope(scopePrefix)
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

                    if (scannerMode == TESTING_MODE) {
                        backEndRepository.logVisitTesting(
                            authorization = generateAuthorization(authentication.accessToken!!),
                            visitInfo = visitInfo
                        )
                    } else {
                        backEndRepository.logVisitProduction(
                            authorization = generateAuthorization(authentication.accessToken!!),
                            visitInfo = visitInfo
                        )
                    }


                } else {
                    val errorMessage = "${AppErrorCodes.NULL_AUTHENTICATION_RESPONSE.code}: ${AppErrorCodes.NULL_AUTHENTICATION_RESPONSE.message}"
                    throw AppException(errorMessage)
                }

            } else {
                val errorMessage = "${AppErrorCodes.NULL_LOGIN_RESPONSE.code}: ${AppErrorCodes.NULL_LOGIN_RESPONSE.message}"
                throw AppException(errorMessage)
            }

        } else {

            if (recentScanCode == null) {

                if (scannerMode == TESTING_MODE) {
                    backEndRepository.logVisitTesting(
                        authorization = generateAuthorization(authentication.value!!.accessToken!!),
                        visitInfo = visitInfo
                    )
                } else {
                    backEndRepository.logVisitProduction(
                        authorization = generateAuthorization(authentication.value!!.accessToken!!),
                        visitInfo = visitInfo
                    )
                }

            }

        }
    }

    private fun getSavedVisitSettings() {
        visitSettings = backEndRepository.getSavedVisitSettings()
    }

    fun getSavedVisitSettingsDirectly() = backEndRepository.getSavedVisitSettings()

    private fun getSavedAuthentication() {
        authentication = backEndRepository.getSavedAuthentication()
    }

    private fun getSavedOrganization() {
        organization = backEndRepository.getSavedOrganization()
    }

    fun getMergedData() = mergedData

    fun writeInternetIsNotAvailable() = prefs.writeInternetIsNotAvailable()

    private fun getScopePrefixProduction() : String = getApplication<Application>().applicationContext.getString(
        R.string.backend_base_url_production)

    private fun getScopePrefixTesting() : String = getApplication<Application>().applicationContext.getString(
        R.string.backend_base_url_testing)

}