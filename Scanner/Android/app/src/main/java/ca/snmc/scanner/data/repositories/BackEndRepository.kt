package ca.snmc.scanner.data.repositories

import androidx.lifecycle.LiveData
import androidx.lifecycle.MutableLiveData
import ca.snmc.scanner.data.db.AppDatabase
import ca.snmc.scanner.data.db.entities.AuthenticationEntity
import ca.snmc.scanner.data.network.BackEndApi
import ca.snmc.scanner.data.network.SafeApiRequest
import ca.snmc.scanner.data.db.entities.OrganizationDoorEntity
import ca.snmc.scanner.data.db.entities.OrganizationEntity
import ca.snmc.scanner.data.db.entities.VisitEntity
import ca.snmc.scanner.data.network.responses.OrganizationDoorsResponse
import ca.snmc.scanner.data.preferences.PreferenceProvider
import ca.snmc.scanner.models.OrganizationDoorInfo
import ca.snmc.scanner.utils.*
import ca.snmc.scanner.utils.BackEndApiUtils.generateAuthorization
import ca.snmc.scanner.utils.GetDoorsApiUtils.generateUrl
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext

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

    suspend fun saveOrganizationDoors(organizationDoorEntities: List<OrganizationDoorEntity>) =
        db.getOrganizationDoorDao().saveOrganizationDoors(organizationDoorEntities)

    suspend fun saveVisitInfo(visitEntity: VisitEntity) = db.getVisitDao().upsert(visitEntity)

    fun getOrganizationDoors(): LiveData<List<OrganizationDoorEntity>> =
        db.getOrganizationDoorDao().getOrganizationDoors()

    fun getSavedOrganization() = db.getOrganizationDao().getOrganization()

    fun getSavedAuthentication() = db.getAuthenticationDao().getAuthentication()

    fun getSavedVisitInfo() = db.getVisitDao().getVisit()

}