package ca.snmc.scanner.screens.settings

import android.app.Application
import androidx.lifecycle.AndroidViewModel
import androidx.lifecycle.LiveData
import androidx.lifecycle.MediatorLiveData
import ca.snmc.scanner.R
import ca.snmc.scanner.data.db.entities.AuthenticationEntity
import ca.snmc.scanner.data.db.entities.OrganizationDoorEntity
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

    private lateinit var organization : OrganizationEntity
    private lateinit var authentication : AuthenticationEntity

    private lateinit var visitSettings : LiveData<VisitEntity>
    private lateinit var doors : LiveData<List<OrganizationDoorEntity>>
    private lateinit var mergedData: MediatorLiveData<CombinedDoorVisitSettingsData>

    fun initialize() {
        getSavedVisitSettings()
        getSavedOrganization()
        getSavedAuthentication()
        getSavedDoors()
        mergedData = MediatorLiveData()
        mergedData.addSource(visitSettings) {
            mergedData.value = CombinedDoorVisitSettingsData(
                doors = doors.value,
                organizationName = visitSettings.value?.organizationName,
                doorName = visitSettings.value?.doorName,
                direction = visitSettings.value?.direction
            )
        }
        mergedData.addSource(doors) {
            mergedData.value = CombinedDoorVisitSettingsData(
                doors = doors.value,
                organizationName = visitSettings.value?.organizationName,
                doorName = visitSettings.value?.doorName,
                direction = visitSettings.value?.direction
            )
        }
    }

    suspend fun fetchOrganizationDoors() {

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
        val organizationDoorInfo = OrganizationDoorInfo(
            url = generateUrl(organization.id!!),
            authorization = generateAuthorization(authentication.accessToken!!)
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

    private fun getSavedDoors() {
        doors = backEndRepository.getOrganizationDoors()
    }

    fun getMergedData() = mergedData

    suspend fun saveVisitSettings(
        selectedDoor: String,
        selectedDirection: String
    ) {
        backEndRepository.saveVisitSettings(VisitEntity(
            organizationName = organization.name,
            doorName = selectedDoor,
            direction = selectedDirection
        ))
    }

    private fun getScopePrefix() : String = getApplication<Application>().applicationContext.getString(
        R.string.backend_base_url)

}