package com.snmc.scanner.data.network

import okhttp3.ResponseBody
import retrofit2.Call
import retrofit2.Retrofit
import retrofit2.converter.gson.GsonConverterFactory
import retrofit2.http.*

// Used by Retrofit to make API call
interface AuthenticateApi {

    @FormUrlEncoded
    @POST("oauth2/token")
    fun scannerAuthenticate(
        @Header("Host") hostUrl: String,
        @Field("grant_type") grantType: String,
        @Field("client_id") clientId: String,
        @Field("client_secret") clientSecret: String,
        @Field("scope") scope: String
    ) : Call<ResponseBody>

    companion object {
        operator fun invoke(baseUrl: String) : AuthenticateApi {
            return Retrofit.Builder()
                .baseUrl(baseUrl)
                .addConverterFactory(GsonConverterFactory.create())
                .build()
                .create(AuthenticateApi::class.java)
        }
    }
}