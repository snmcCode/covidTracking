package com.snmc.scanner.data.repositories

import com.snmc.scanner.data.db.AppDatabase
import com.snmc.scanner.data.db.entities.AuthenticationEntity
import com.snmc.scanner.data.network.AuthenticateApi
import com.snmc.scanner.data.network.SafeApiRequest
import com.snmc.scanner.data.network.responses.AuthenticateResponse
import com.snmc.scanner.models.AuthenticateInfo

// Used to abstract API calls away from ViewModel, returns Response object to ViewModel
class AuthenticateRepository(
    private val api: AuthenticateApi,
    private val db: AppDatabase
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