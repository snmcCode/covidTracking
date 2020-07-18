package com.snmc.scanner.data.network

import android.content.res.Resources
import com.snmc.scanner.R
import com.snmc.scanner.models.LoginInfo
import okhttp3.ResponseBody
import retrofit2.Call
import retrofit2.Retrofit
import retrofit2.converter.gson.GsonConverterFactory
import retrofit2.http.*

// Used by Retrofit to make API call
interface MobileAuthenticateApi {

    @POST("authenticate")
    @Headers("Content-Type: application/json", "Cache-Control: max-age=640000")
    fun scannerLogin(
        @Body loginInfo: LoginInfo
    ) : Call<ResponseBody>

    companion object {
        operator fun invoke() : MobileAuthenticateApi {
            return Retrofit.Builder()
                .baseUrl("https://snmtrackingapi-anonymous-testing.azurewebsites.net/api/")
                .addConverterFactory(GsonConverterFactory.create())
                .build()
                .create(MobileAuthenticateApi::class.java)
        }
    }
}