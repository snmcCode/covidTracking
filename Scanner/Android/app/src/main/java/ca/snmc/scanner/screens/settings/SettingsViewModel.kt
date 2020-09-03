package ca.snmc.scanner.screens.settings

import android.app.Application
import androidx.lifecycle.AndroidViewModel
import androidx.lifecycle.LiveData
import androidx.lifecycle.MediatorLiveData
import ca.snmc.scanner.BuildConfig
import ca.snmc.scanner.R
import ca.snmc.scanner.data.db.entities.AuthenticationEntity
import ca.snmc.scanner.data.db.entities.OrganizationDoorEntity
import ca.snmc.scanner.data.db.entities.OrganizationEntity
import ca.snmc.scanner.data.db.entities.VisitEntity
import ca.snmc.scanner.data.network.responses.AuthenticateResponse
import ca.snmc.scanner.data.network.responses.LoginResponse
import ca.snmc.scanner.data.network.responses.OrganizationDoorsResponse
import ca.snmc.scanner.data.providers.PreferenceProvider
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
    private lateinit var mergedOrgAuthData : MediatorLiveData<CombinedOrgAuthData>

    private lateinit var doors : LiveData<List<OrganizationDoorEntity>>
    private lateinit var visitSettings : LiveData<VisitEntity>
    private lateinit var mergedDoorVisitData: MediatorLiveData<CombinedDoorVisitData>

    fun initialize() {
        getSavedOrganization()
        getSavedAuthentication()
        mergedOrgAuthData = MediatorLiveData()
        mergedOrgAuthData.addSource(organization) {
            mergedOrgAuthData.value = CombinedOrgAuthData(
                id = organization.value?.id,
                authorization = authentication.value?.accessToken,
                username = organization.value?.username,
                password = organization.value?.password
            )
        }
        mergedOrgAuthData.addSource(authentication) {
            mergedOrgAuthData.value = CombinedOrgAuthData(
                id = organization.value?.id,
                authorization = authentication.value?.accessToken,
                username = organization.value?.username,
                password = organization.value?.password
            )
        }

        getSavedVisitSettings()
        getSavedDoors()
        mergedDoorVisitData = MediatorLiveData()
        mergedDoorVisitData.addSource(visitSettings) {
            mergedDoorVisitData.value = CombinedDoorVisitData(
                doors = doors.value,
                organizationName = visitSettings.value?.organizationName,
                doorName = visitSettings.value?.doorName,
                direction = visitSettings.value?.direction
            )
        }
        mergedDoorVisitData.addSource(doors) {
            mergedDoorVisitData.value = CombinedDoorVisitData(
                doors = doors.value,
                organizationName = visitSettings.value?.organizationName,
                doorName = visitSettings.value?.doorName,
                direction = visitSettings.value?.direction
            )
        }
    }

    suspend fun fetchOrganizationDoors() {

        val scannerMode = prefs.readScannerMode()

//        Log.e("E-Time", authentication.value!!.expireTime!!.toString())
//        Log.e("C-Time", System.currentTimeMillis().toString())
//        Log.e("E-C Diff", (authentication.value!!.expireTime!! - System.currentTimeMillis()).toString())

        // Check access token
        if (isAccessTokenExpired(authentication.value!!.expireTime!!)) {

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

                    val organizationDoorInfo = OrganizationDoorInfo(
                        url = generateUrl(organization.value!!.id!!),
                        authorization = generateAuthorization(authentication.accessToken!!)
                    )

                    // Selection based on Scanner Mode
                    val organizationDoorsResponse : OrganizationDoorsResponse = if (scannerMode == TESTING_MODE) {
                        backEndRepository.fetchOrganizationDoorsTesting(organizationDoorInfo)
                    } else {
                        backEndRepository.fetchOrganizationDoorsProduction(organizationDoorInfo)
                    }

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
                        throw AuthenticationException(errorMessage)
                    }

                } else {
                    val errorMessage = "${AppErrorCodes.NULL_AUTHENTICATION_RESPONSE.code}: ${AppErrorCodes.NULL_AUTHENTICATION_RESPONSE.message}"
                    throw AuthenticationException(errorMessage)
                }

            } else {
                val errorMessage = "${AppErrorCodes.NULL_LOGIN_RESPONSE.code}: ${AppErrorCodes.NULL_LOGIN_RESPONSE.message}"
                throw AuthenticationException(errorMessage)
            }
        } else {

            val organizationDoorInfo = OrganizationDoorInfo(
                url = generateUrl(organization.value!!.id!!),
                authorization = generateAuthorization(authentication.value!!.accessToken!!)
            )

            // Selection based on Scanner Mode
            val organizationDoorsResponse : OrganizationDoorsResponse = if (scannerMode == TESTING_MODE) {
                backEndRepository.fetchOrganizationDoorsTesting(organizationDoorInfo)
            } else {
                backEndRepository.fetchOrganizationDoorsProduction(organizationDoorInfo)
            }

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
                throw AuthenticationException(errorMessage)
            }

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

    fun getMergedOrgAuthData() = mergedOrgAuthData

    fun getMergedDoorVisitData() = mergedDoorVisitData

    // Expose Doors DB to UI to Read from it
    fun getOrganizationDoors() = backEndRepository.getOrganizationDoors()

    suspend fun saveVisitSettings(
        selectedDoor: String,
        selectedDirection: String
    ) {
        backEndRepository.saveVisitSettings(VisitEntity(
            organizationName = organization.value!!.name,
            doorName = selectedDoor,
            direction = selectedDirection,
            scannerVersion = BuildConfig.VERSION_NAME
        ))
    }

    fun getSavedVisitSettingsDirectly() = backEndRepository.getSavedVisitSettings()

    private fun getScopePrefixProduction() : String = getApplication<Application>().applicationContext.getString(
        R.string.backend_base_url_production)

    private fun getScopePrefixTesting() : String = getApplication<Application>().applicationContext.getString(
        R.string.backend_base_url_testing)

    fun saveScannerMode(mode: Int) {
        when(mode) {
            TESTING_MODE -> { prefs.writeTestingMode() }
            PRODUCTION_MODE -> { prefs.writeProductionMode() }
        }
    }

    fun getScannerMode() : Int = prefs.readScannerMode()
}