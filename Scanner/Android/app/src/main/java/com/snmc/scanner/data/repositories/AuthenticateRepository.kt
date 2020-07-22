package com.snmc.scanner.data.repositories

import com.snmc.scanner.data.db.AppDatabase
import com.snmc.scanner.data.db.entities.Authentication
import com.snmc.scanner.data.network.AuthenticateApi
import com.snmc.scanner.data.network.LoginApi
import com.snmc.scanner.data.network.SafeApiRequest
import com.snmc.scanner.data.network.responses.AuthenticateResponse
import com.snmc.scanner.models.AuthenticateInfo
import retrofit2.Response

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

    suspend fun saveAuthentication(authentication: Authentication) = db.getAuthenticationDao().upsert(authentication)
}