package com.snmc.scanner.data.repositories

import com.snmc.scanner.data.network.LoginApi
import com.snmc.scanner.data.network.responses.LoginResponse
import com.snmc.scanner.models.LoginInfo
import retrofit2.Response

// Used to abstract API calls away from ViewModel, returns Response object to ViewModel
class LoginRepository {

    suspend fun scannerLogin(baseUrl: String, loginInfo: LoginInfo) : Response<LoginResponse> {
        // TODO: Bad Practice, will be fixed later with dependency injection
        return LoginApi(baseUrl).scannerLogin(loginInfo)
    }

}