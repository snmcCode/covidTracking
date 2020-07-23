package ca.snmc.scanner.data.repositories

import androidx.lifecycle.LiveData
import androidx.lifecycle.MutableLiveData
import ca.snmc.scanner.data.db.AppDatabase
import ca.snmc.scanner.data.network.BackEndApi
import ca.snmc.scanner.data.network.SafeApiRequest
import ca.snmc.scanner.data.db.entities.OrganizationDoorEntity
import ca.snmc.scanner.models.OrganizationDoorInfo
import ca.snmc.scanner.utils.Coroutines
import ca.snmc.scanner.utils.mapOrganizationDoorResponseToOrganizationDoorEntityList
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext

// Used to abstract API calls away from ViewModel, returns Response object to ViewModel
class BackEndRepository(
    private val api: BackEndApi,
    private val db: AppDatabase
) : SafeApiRequest() {

    private val organizationDoors = MutableLiveData<List<OrganizationDoorEntity>>()
    private var savedOrganizationDoorInfo : OrganizationDoorInfo? = null

    init {
        organizationDoors.observeForever {
            saveOrganizationDoors(it)
        }
    }

    private suspend fun fetchOrganizationDoors(organizationDoorInfo: OrganizationDoorInfo) {
        savedOrganizationDoorInfo = organizationDoorInfo
        if (isFetchNeeded()) {
            val response = apiRequest {
                api.getOrganizationDoors(
                    url = organizationDoorInfo.url,
                    authorization = organizationDoorInfo.authorization,
                    xFunctionsKey = organizationDoorInfo.xFunctionsKey
                )}

            organizationDoors.postValue(mapOrganizationDoorResponseToOrganizationDoorEntityList(response))
        }
    }

    suspend fun getOrganizationDoors() : LiveData<List<OrganizationDoorEntity>> {
        return withContext(Dispatchers.IO) {
            // TODO: Figure out how to optimize this
            savedOrganizationDoorInfo?.let { fetchOrganizationDoors(it) }
            db.getOrganizationDoorDao().getOrganizationDoors()
        }
    }

    private fun saveOrganizationDoors(organizationDoorEntities: List<OrganizationDoorEntity>) {
        Coroutines.io {
            db.getOrganizationDoorDao().saveOrganizationDoors(organizationDoorEntities)
        }
    }

    private fun isFetchNeeded() : Boolean {
        // TODO: Write this function
        return true
    }

}