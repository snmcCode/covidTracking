package com.snmc.scanner.data.repositories

import androidx.lifecycle.LiveData
import androidx.lifecycle.MutableLiveData
import com.snmc.scanner.data.network.MobileAuthenticateApi
import com.snmc.scanner.models.LoginInfo
import okhttp3.ResponseBody
import retrofit2.Call
import retrofit2.Callback
import retrofit2.Response

// Used to abstract API calls away from ViewModel, returns Response object to ViewModel
class LoginRepository {

    fun scannerLogin(loginInfo: LoginInfo) : LiveData<String> {

        val loginResponse = MutableLiveData<String>()

        // TODO: Bad Practice to have the repository dependent upon the API, should be removed later and injected
        MobileAuthenticateApi().scannerLogin(loginInfo)
            .enqueue(object: Callback<ResponseBody> {
                override fun onFailure(call: Call<ResponseBody>, t: Throwable) {
                    loginResponse.value = t.message
                }

                override fun onResponse(
                    call: Call<ResponseBody>,
                    response: Response<ResponseBody>
                ) {
                    if (response.isSuccessful) {
                        loginResponse.value = response.body()?.string()
                    } else {
                        loginResponse.value = response.errorBody()?.string()
                    }
                }

            })

        return loginResponse
    }

}