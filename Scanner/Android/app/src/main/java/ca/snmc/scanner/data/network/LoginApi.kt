package ca.snmc.scanner.data.network

import ca.snmc.scanner.data.network.responses.LoginResponse
import ca.snmc.scanner.models.LoginInfo
import okhttp3.OkHttpClient
import okhttp3.ResponseBody
import retrofit2.Call
import retrofit2.Response
import retrofit2.Retrofit
import retrofit2.converter.gson.GsonConverterFactory
import retrofit2.http.*

// Used by Retrofit to make API call
interface LoginApi {

    @POST("authenticate")
    @Headers("Content-Type: application/json", "Cache-Control: max-age=640000")
    suspend fun scannerLogin(
        @Body loginInfo: LoginInfo
    ) : Response<LoginResponse>

    companion object {
        operator fun invoke(
            baseUrl: String,
            networkConnectionInterceptor: NetworkConnectionInterceptor
        ) : LoginApi {

            val okHttpClient = OkHttpClient.Builder()
                .addInterceptor(networkConnectionInterceptor)
                .build()

            return Retrofit.Builder()
                .baseUrl(baseUrl)
                .client(okHttpClient)
                .addConverterFactory(GsonConverterFactory.create())
                .build()
                .create(LoginApi::class.java)
        }
    }
}