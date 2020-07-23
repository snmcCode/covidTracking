package ca.snmc.scanner.data.repositories

import ca.snmc.scanner.data.db.AppDatabase
import ca.snmc.scanner.data.db.entities.OrganizationEntity
import ca.snmc.scanner.data.network.LoginApi
import ca.snmc.scanner.data.network.SafeApiRequest
import ca.snmc.scanner.data.network.responses.LoginResponse
import ca.snmc.scanner.models.LoginInfo

// Used to abstract API calls away from ViewModel, returns Response object to ViewModel
class LoginRepository(
    private val api: LoginApi,
    private val db: _root_ide_package_.ca.snmc.scanner.data.db.AppDatabase
) : SafeApiRequest() {

    suspend fun scannerLogin(loginInfo: LoginInfo) : LoginResponse {
        return apiRequest { api.scannerLogin(loginInfo) }
    }

    suspend fun saveOrganization(organizationEntity: OrganizationEntity) = db.getOrganizationDao().upsert(organizationEntity)

    fun getSavedOrganization() = db.getOrganizationDao().getOrganization()

}