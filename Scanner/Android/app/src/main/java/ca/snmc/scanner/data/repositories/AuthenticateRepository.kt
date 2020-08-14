package ca.snmc.scanner.data.repositories

import ca.snmc.scanner.data.db.AppDatabase
import ca.snmc.scanner.data.db.entities.AuthenticationEntity
import ca.snmc.scanner.data.network.SafeApiRequest
import ca.snmc.scanner.data.network.apis.production.AuthenticateProductionApi
import ca.snmc.scanner.data.network.apis.testing.AuthenticateTestingApi
import ca.snmc.scanner.data.network.responses.AuthenticateResponse
import ca.snmc.scanner.models.AuthenticateInfo

// Used to abstract API calls away from ViewModel, returns Response object to ViewModel
class AuthenticateRepository(
    private val productionApi: AuthenticateProductionApi,
    private val testingApi: AuthenticateTestingApi,
    private val db: AppDatabase
) : SafeApiRequest() {

    suspend fun scannerAuthenticateProduction(authenticateInfo: AuthenticateInfo) : AuthenticateResponse {
        return apiRequest {
            productionApi.scannerAuthenticate(
                grantType = authenticateInfo.grantType,
                clientId = authenticateInfo.clientId,
                clientSecret = authenticateInfo.clientSecret,
                scope = authenticateInfo.scope
            )}
    }

    suspend fun scannerAuthenticateTesting(authenticateInfo: AuthenticateInfo) : AuthenticateResponse {
        return apiRequest {
            testingApi.scannerAuthenticate(
                grantType = authenticateInfo.grantType,
                clientId = authenticateInfo.clientId,
                clientSecret = authenticateInfo.clientSecret,
                scope = authenticateInfo.scope
            )}
    }

    suspend fun saveAuthentication(authenticationEntity: AuthenticationEntity) = db.getAuthenticationDao().upsert(authenticationEntity)

    fun getSavedAuthentication() = db.getAuthenticationDao().getAuthentication()

}