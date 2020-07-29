package ca.snmc.scanner.data.repositories

import androidx.lifecycle.LiveData
import ca.snmc.scanner.data.db.AppDatabase
import ca.snmc.scanner.data.network.BackEndApi
import ca.snmc.scanner.data.network.SafeApiRequest
import ca.snmc.scanner.data.db.entities.OrganizationDoorEntity
import ca.snmc.scanner.data.db.entities.VisitEntity
import ca.snmc.scanner.data.network.responses.OrganizationDoorsResponse
import ca.snmc.scanner.models.OrganizationDoorInfo
import ca.snmc.scanner.models.VisitInfo

// Used to abstract API calls away from ViewModel, returns Response object to ViewModel
class BackEndRepository(
    private val api: BackEndApi,
    private val db: AppDatabase
) : SafeApiRequest() {

    suspend fun fetchOrganizationDoors(organizationDoorInfo: OrganizationDoorInfo): OrganizationDoorsResponse {
        return apiRequest {
            api.getOrganizationDoors(
                url = organizationDoorInfo.url,
                authorization = organizationDoorInfo.authorization
            )
        }
    }

    suspend fun logVisit(authorization: String, visitInfo: VisitInfo) : String {
        return apiRequest {
            api.logVisit(
                authorization = authorization, // TODO: Make sure ScannerViewModel retrieves Authorization from DB following example of SettingsViewModel, except you should save it as a variable local to the ViewModel
                visitInfo = visitInfo
            )
        }
    }

    suspend fun saveOrganizationDoors(organizationDoorEntities: List<OrganizationDoorEntity>) =
        db.getOrganizationDoorDao().saveOrganizationDoors(organizationDoorEntities)

    suspend fun saveVisitSettings(visitEntity: VisitEntity) = db.getVisitDao().upsert(visitEntity)

    fun getOrganizationDoors(): LiveData<List<OrganizationDoorEntity>> =
        db.getOrganizationDoorDao().getOrganizationDoors()

    fun getSavedOrganization() = db.getOrganizationDao().getOrganization()

    fun getSavedAuthentication() = db.getAuthenticationDao().getAuthentication()

    fun getSavedVisitSettings() = db.getVisitDao().getVisit()

    suspend fun deleteAllData() {
        db.getVisitDao().delete()
        db.getOrganizationDoorDao().deleteAll()
        db.getAuthenticationDao().delete()
        db.getOrganizationDao().delete()
    }

}