package com.snmc.scanner.data.repositories

import androidx.lifecycle.MutableLiveData
import com.snmc.scanner.data.db.AppDatabase
import com.snmc.scanner.data.network.BackEndApi
import com.snmc.scanner.data.network.SafeApiRequest
import com.snmc.scanner.data.network.responses.OrganizationDoorsResponse
import com.snmc.scanner.data.db.entities.OrganizationDoorEntity
import com.snmc.scanner.models.OrganizationDoorInfo
import com.snmc.scanner.utils.Coroutines

// Used to abstract API calls away from ViewModel, returns Response object to ViewModel
class BackEndRepository(
    private val api: BackEndApi,
    private val db: AppDatabase
) : SafeApiRequest() {

    private val organizationDoors = MutableLiveData<List<OrganizationDoorEntity>>()

    init {
        organizationDoors.observeForever {
            saveOrganizationDoors(it)
        }
    }

    private suspend fun fetchOrganizationDoors(organizationDoorsInfo: OrganizationDoorInfo) : OrganizationDoorsResponse {
        if (isFetchNeeded()) {
            val response = apiRequest {
                api.getOrganizationDoors(
                    url = organizationDoorsInfo.url,
                    authorization = organizationDoorsInfo.authorization,
                    xFunctionsKey = organizationDoorsInfo.xFunctionsKey
                )}

            organizationDoors.postValue(response.toList())

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

    fun getOrganizationDoors() = db.getOrganizationDoorDao().getOrganizationDoors()

}