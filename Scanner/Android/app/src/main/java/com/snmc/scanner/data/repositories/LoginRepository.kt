package com.snmc.scanner.repositories

import androidx.lifecycle.LiveData
import androidx.lifecycle.MutableLiveData
import com.snmc.scanner.data.network.MobileAuthenticateApi
import okhttp3.ResponseBody
import retrofit2.Call
import retrofit2.Callback
import retrofit2.Response

class LoginRepository {

    fun scannerLogin(username: String, password: String) : LiveData<String> {

        val loginResponse = MutableLiveData<String>()

        // Bad Practice to have the repository dependent upon the API, should be removed later
        MobileAuthenticateApi().scannerLogin(username, password)
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