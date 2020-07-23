package com.snmc.scanner.data.repositories

import com.snmc.scanner.data.db.AppDatabase
import com.snmc.scanner.data.network.BackEndApi
import com.snmc.scanner.data.network.SafeApiRequest
import com.snmc.scanner.data.network.responses.OrganizationDoorsResponse
import com.snmc.scanner.data.db.entities.OrganizationDoor
import com.snmc.scanner.models.OrganizationDoorInfo

// Used to abstract API calls away from ViewModel, returns Response object to ViewModel
class BackEndRepository(
    private val api: BackEndApi,
    private val db: AppDatabase
) : SafeApiRequest() {

    suspend fun scannerGetOrganizationDoors(organizationDoorsInfo: OrganizationDoorInfo) : OrganizationDoorsResponse {
        return apiRequest {
            api.getOrganizationDoors(
                url = organizationDoorsInfo.url,
                authorization = organizationDoorsInfo.authorization,
                xFunctionsKey = organizationDoorsInfo.xFunctionsKey
            )}
    }

    suspend fun saveOrganizationDoors(organizationDoors: List<OrganizationDoor>) = db.getOrganizationDoorsDao().upsert(organizationDoors)

    fun getOrganizationDoors() = db.getOrganizationDoorsDao().getOrganizationDoors()

}