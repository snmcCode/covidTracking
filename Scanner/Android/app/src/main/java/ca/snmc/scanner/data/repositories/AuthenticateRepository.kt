package ca.snmc.scanner.data.repositories

import ca.snmc.scanner.data.db.AppDatabase
import ca.snmc.scanner.data.db.entities.AuthenticationEntity
import ca.snmc.scanner.data.network.AuthenticateApi
import ca.snmc.scanner.data.network.SafeApiRequest
import ca.snmc.scanner.data.network.responses.AuthenticateResponse
import ca.snmc.scanner.models.AuthenticateInfo

// Used to abstract API calls away from ViewModel, returns Response object to ViewModel
class AuthenticateRepository(
    private val api: AuthenticateApi,
    private val db: _root_ide_package_.ca.snmc.scanner.data.db.AppDatabase
) : SafeApiRequest() {

    suspend fun scannerAuthenticate(authenticateInfo: AuthenticateInfo) : AuthenticateResponse {
        return apiRequest {
            api.scannerAuthenticate(
                grantType = authenticateInfo.grantType,
                clientId = authenticateInfo.clientId,
                clientSecret = authenticateInfo.clientSecret,
                scope = authenticateInfo.scope
            )}
    }

    suspend fun saveAuthentication(authenticationEntity: AuthenticationEntity) = db.getAuthenticationDao().upsert(authenticationEntity)
}