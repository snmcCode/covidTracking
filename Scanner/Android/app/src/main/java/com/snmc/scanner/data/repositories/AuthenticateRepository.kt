package com.snmc.scanner.data.repositories

import androidx.lifecycle.LiveData
import androidx.lifecycle.MutableLiveData
import com.snmc.scanner.data.network.AuthenticateApi
import com.snmc.scanner.models.AuthenticateInfo
import okhttp3.ResponseBody
import retrofit2.Call
import retrofit2.Callback
import retrofit2.Response

// Used to abstract API calls away from ViewModel, returns Response object to ViewModel
class AuthenticateRepository {

    fun scannerAuthenticate(baseUrl: String, authenticateInfo: AuthenticateInfo) : LiveData<String> {
        val authenticateResponse = MutableLiveData<String>()

        // TODO: Bad Practice to have the repository dependent upon the API, should be removed later and injected
        AuthenticateApi.invoke(baseUrl).scannerAuthenticate(
            hostUrl = authenticateInfo.hostUrl,
            grantType = authenticateInfo.grantType,
            clientId = authenticateInfo.clientId,
            clientSecret = authenticateInfo.clientSecret,
            scope = authenticateInfo.scope
        )
            .enqueue(object: Callback<ResponseBody> {
                override fun onFailure(call: Call<ResponseBody>, t: Throwable) {
                    authenticateResponse.value = t.message
                }

                override fun onResponse(
                    call: Call<ResponseBody>,
                    response: Response<ResponseBody>
                ) {
                    if (response.isSuccessful) {
                        authenticateResponse.value = response.body()?.toString()
                    } else {
                        authenticateResponse.value = response.errorBody()?.toString()
                    }
                }
            })

        return authenticateResponse
    }
}