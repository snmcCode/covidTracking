package ca.snmc.scanner.screens.settings

import android.app.Application
import androidx.lifecycle.AndroidViewModel
import androidx.lifecycle.LiveData
import androidx.lifecycle.MediatorLiveData
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
import ca.snmc.scanner.models.OrganizationDoorInfo
import ca.snmc.scanner.utils.*
import ca.snmc.scanner.utils.BackEndApiUtils.generateAuthorization
import ca.snmc.scanner.utils.GetDoorsApiUtils.generateUrl

class SettingsViewModel(
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

    fun initialize() {
        getSavedVisitSettings()
        getSavedOrganization()
        getSavedAuthentication()
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

    suspend fun fetchOrganizationDoors() {

        // Check access token
//        if (isAccessTokenExpired(authentication.value!!.expireTime!!)) {
//            val loginResponse = loginRepository.scannerLogin(LoginInfo(
//                username = organization.value!!.username!!,
//                password = organization.value!!.password!!
//            ))
//            if (loginResponse.isNotNull()) {
//                // Set Is Internet Available Flag to True in SharedPrefs Due to Successful API Call
//                prefs.writeInternetIsAvailable()
//
//                val authenticateInfo = AuthenticateInfo(
//                    grantType = AuthApiUtils.getGrantType(),
//                    clientId = loginResponse.clientId!!,
//                    clientSecret = loginResponse.clientSecret!!,
//                    scope = AuthApiUtils.getScope(scopePrefix = getScopePrefix())
//                )
//                val authenticateResponse = authenticateRepository.scannerAuthenticate(authenticateInfo = authenticateInfo)
//                if (authenticateResponse.isNotNull()) {
//                    // Map AuthenticationResponse to AuthenticationEntity
//                    val authentication = mapAuthenticateResponseToAuthenticationEntity(authenticateResponse)
//                    // Store AuthenticationEntity in DB
//                    authenticateRepository.saveAuthentication(authentication)
//                    // Set Is Internet Available Flag to True in SharedPrefs Due to Successful API Call
//                    prefs.writeInternetIsAvailable()
//                } else {
//                    val errorMessage = "${AppErrorCodes.NULL_AUTHENTICATION_RESPONSE.code}: ${AppErrorCodes.NULL_AUTHENTICATION_RESPONSE.message}"
//                    throw AppException(errorMessage)
//                }
//
//            } else {
//                val errorMessage = "${AppErrorCodes.NULL_LOGIN_RESPONSE.code}: ${AppErrorCodes.NULL_LOGIN_RESPONSE.message}"
//                throw AppException(errorMessage)
//            }
//        }

        val organizationDoorInfo = OrganizationDoorInfo(
            url = generateUrl(organization.value!!.id!!),
            authorization = generateAuthorization(authentication.value!!.accessToken!!)
        )
        val organizationDoorsResponse = backEndRepository.fetchOrganizationDoors(organizationDoorInfo)
        if (organizationDoorsResponse.isNotEmpty()) {
            // Map OrganizationDoorsResponse to OrganizationDoorEntityList
            val organizationDoorEntityList = mapOrganizationDoorResponseToOrganizationDoorEntityList(organizationDoorsResponse)
            // Store OrganizationDoorEntityList in DB
            backEndRepository.saveOrganizationDoors(organizationDoorEntityList)
            // Set Doors Are Fetched Flag in SharedPrefs
            prefs.writeDoorsAreFetched()
            // Set Is Internet Available Flag to True in SharedPrefs Due to Successful API Call
            prefs.writeInternetIsAvailable()
        } else {
            val errorMessage = "${AppErrorCodes.NULL_ORGANIZATION_DOORS_RESPONSE.code}: ${AppErrorCodes.NULL_ORGANIZATION_DOORS_RESPONSE.message}"
            throw AppException(errorMessage)
        }
    }

    suspend fun deleteAllData() = backEndRepository.deleteAllData()

    fun clearPrefs() {
        prefs.writeDoorsAreNotFetched()
        prefs.writeUserIsNotLoggedIn()
    }

    fun areOrganizationDoorsFetched() = prefs.readAreDoorsFetched()

    fun writeInternetIsNotAvailable() = prefs.writeInternetIsNotAvailable()

    private fun getSavedOrganization() {
        organization = backEndRepository.getSavedOrganization()
    }

    private fun getSavedAuthentication() {
        authentication = backEndRepository.getSavedAuthentication()
    }

    private fun getSavedVisitSettings() {
        visitSettings = backEndRepository.getSavedVisitSettings()
    }

    fun getMergedData() = mergedData

    // Expose Doors DB to UI to Read from it
    fun getOrganizationDoors() = backEndRepository.getOrganizationDoors()

    suspend fun saveVisitSettings(
        selectedDoor: String,
        selectedDirection: String
    ) {
        backEndRepository.saveVisitSettings(VisitEntity(
            organizationName = organization.value!!.name,
            doorName = selectedDoor,
            direction = selectedDirection
        ))
    }

    fun getSavedVisitSettingsDirectly() = backEndRepository.getSavedVisitSettings()

    private fun getScopePrefix() : String = getApplication<Application>().applicationContext.getString(
        R.string.backend_base_url)

}