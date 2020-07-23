package com.snmc.scanner.data.network

import com.snmc.scanner.data.network.responses.AuthenticateResponse
import okhttp3.OkHttpClient
import okhttp3.ResponseBody
import retrofit2.Call
import retrofit2.Response
import retrofit2.Retrofit
import retrofit2.converter.gson.GsonConverterFactory
import retrofit2.http.*

// Used by Retrofit to make API call
interface AuthenticateApi {

    @FormUrlEncoded
    @POST("oauth2/v2.0/token")
    @Headers("Content-Type: application/x-www-form-urlencoded", "Cache-Control: max-age=640000")
    suspend fun scannerAuthenticate(
        @Field("grant_type") grantType: String,
        @Field("client_id") clientId: String,
        @Field("client_secret") clientSecret: String,
        @Field("scope") scope: String
    ) : Response<AuthenticateResponse>

    companion object {
        operator fun invoke(
            baseUrl: String,
            networkConnectionInterceptor: NetworkConnectionInterceptor
        ) : AuthenticateApi {

            val okHttpClient = OkHttpClient.Builder()
                .addInterceptor(networkConnectionInterceptor)
                .build()

            return Retrofit.Builder()
                .baseUrl(baseUrl)
                .client(okHttpClient)
                .addConverterFactory(GsonConverterFactory.create())
                .build()
                .create(AuthenticateApi::class.java)
        }
    }
}