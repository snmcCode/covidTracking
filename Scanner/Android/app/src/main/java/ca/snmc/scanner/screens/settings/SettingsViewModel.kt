package ca.snmc.scanner.screens.settings

import android.app.Application
import androidx.lifecycle.AndroidViewModel
import androidx.lifecycle.LiveData
import androidx.lifecycle.MediatorLiveData
import ca.snmc.scanner.data.db.entities.AuthenticationEntity
import ca.snmc.scanner.data.db.entities.OrganizationEntity
import ca.snmc.scanner.data.db.entities.VisitEntity
import ca.snmc.scanner.data.preferences.PreferenceProvider
import ca.snmc.scanner.data.repositories.BackEndRepository
import ca.snmc.scanner.models.OrganizationDoorInfo
import ca.snmc.scanner.utils.AppErrorCodes
import ca.snmc.scanner.utils.AppException
import ca.snmc.scanner.utils.BackEndApiUtils.generateAuthorization
import ca.snmc.scanner.utils.CombinedOrgAuthData
import ca.snmc.scanner.utils.GetDoorsApiUtils.generateUrl
import ca.snmc.scanner.utils.mapOrganizationDoorResponseToOrganizationDoorEntityList

class SettingsViewModel(
    application: Application,
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
                authorization = authentication.value?.accessToken
            )
        }
        mergedData.addSource(authentication) {
            mergedData.value = CombinedOrgAuthData(
                id = organization.value?.id,
                authorization = authentication.value?.accessToken
            )
        }
    }

    suspend fun fetchOrganizationDoors() {
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
        prefs.writeUserIsNotAuthenticated()
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

}