package ca.snmc.scanner.data.network

import ca.snmc.scanner.data.network.responses.OrganizationDoorsResponse
import ca.snmc.scanner.models.OrganizationDoor
import okhttp3.OkHttpClient
import retrofit2.Response
import retrofit2.Retrofit
import retrofit2.converter.gson.GsonConverterFactory
import retrofit2.http.GET
import retrofit2.http.Header
import retrofit2.http.Url

// Used by Retrofit to make API call
interface BackEndApi {

    @GET
    suspend fun getOrganizationDoors(
        @Url url: String,
        @Header("Authorization") authorization: String,
        @Header("x-functions-key") xFunctionsKey: String
    ) : Response<OrganizationDoorsResponse>

    companion object {
        operator fun invoke(
            baseUrl: String,
            networkConnectionInterceptor: NetworkConnectionInterceptor
        ) : BackEndApi {
            val okHttpClient = OkHttpClient.Builder()
                .addInterceptor(networkConnectionInterceptor)
                .build()

            return Retrofit.Builder()
                .baseUrl(baseUrl)
                .client(okHttpClient)
                .addConverterFactory(GsonConverterFactory.create())
                .build()
                .create(BackEndApi::class.java)
        }
    }
}