package ca.snmc.scanner.data.network.apis.testing

import ca.snmc.scanner.data.network.NetworkConnectionInterceptor
import ca.snmc.scanner.data.network.responses.LoginResponse
import ca.snmc.scanner.models.LoginInfo
import okhttp3.OkHttpClient
import retrofit2.Response
import retrofit2.Retrofit
import retrofit2.converter.gson.GsonConverterFactory
import retrofit2.http.Body
import retrofit2.http.Headers
import retrofit2.http.POST

// Used by Retrofit to make API call
interface LoginTestingApi {

    @POST("authenticate")
    @Headers("Content-Type: application/json", "Cache-Control: max-age=640000")
    suspend fun scannerLogin(
        @Body loginInfo: LoginInfo
    ) : Response<LoginResponse>

    companion object {
        operator fun invoke(
            baseUrl: String,
            networkConnectionInterceptor: NetworkConnectionInterceptor
        ) : LoginTestingApi {

            val okHttpClient = OkHttpClient.Builder()
                .addInterceptor(networkConnectionInterceptor)
                .retryOnConnectionFailure(false)
                .build()

            return Retrofit.Builder()
                .baseUrl(baseUrl)
                .client(okHttpClient)
                .addConverterFactory(GsonConverterFactory.create())
                .build()
                .create(LoginTestingApi::class.java)
        }
    }
}