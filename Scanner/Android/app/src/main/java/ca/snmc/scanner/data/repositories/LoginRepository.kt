package ca.snmc.scanner.data.repositories

import ca.snmc.scanner.data.db.AppDatabase
import ca.snmc.scanner.data.db.entities.OrganizationEntity
import ca.snmc.scanner.data.network.SafeApiRequest
import ca.snmc.scanner.data.network.apis.production.LoginProductionApi
import ca.snmc.scanner.data.network.apis.testing.LoginTestingApi
import ca.snmc.scanner.data.network.responses.LoginResponse
import ca.snmc.scanner.models.LoginInfo

// Used to abstract API calls away from ViewModel, returns Response object to ViewModel
class LoginRepository(
    private val productionApi: LoginProductionApi,
    private val testingApi: LoginTestingApi,
    private val db: AppDatabase
) : SafeApiRequest() {

    suspend fun scannerLoginProduction(loginInfo: LoginInfo) : LoginResponse {
        return apiRequest { productionApi.scannerLogin(loginInfo) }
    }

    suspend fun scannerLoginTesting(loginInfo: LoginInfo) : LoginResponse {
        return apiRequest { testingApi.scannerLogin(loginInfo) }
    }

    suspend fun saveOrganization(organizationEntity: OrganizationEntity) = db.getOrganizationDao().upsert(organizationEntity)

}