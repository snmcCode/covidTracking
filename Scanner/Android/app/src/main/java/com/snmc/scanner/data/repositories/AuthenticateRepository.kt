package com.snmc.scanner.data.repositories

import com.snmc.scanner.data.network.AuthenticateApi
import com.snmc.scanner.data.network.responses.AuthenticateResponse
import com.snmc.scanner.models.AuthenticateInfo
import retrofit2.Response

// Used to abstract API calls away from ViewModel, returns Response object to ViewModel
class AuthenticateRepository {

    suspend fun scannerAuthenticate(baseUrl: String, authenticateInfo: AuthenticateInfo) : Response<AuthenticateResponse> {
        // TODO: Bad Practice to have the repository dependent upon the API, should be removed later and injected
        return AuthenticateApi.invoke(baseUrl).scannerAuthenticate(
            grantType = authenticateInfo.grantType,
            clientId = authenticateInfo.clientId,
            clientSecret = authenticateInfo.clientSecret,
            scope = authenticateInfo.scope
        )
    }
}