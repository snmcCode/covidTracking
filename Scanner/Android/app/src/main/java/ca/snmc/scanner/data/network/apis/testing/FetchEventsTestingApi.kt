package ca.snmc.scanner.data.network.apis.testing

import ca.snmc.scanner.data.network.NetworkConnectionInterceptor
import ca.snmc.scanner.data.network.responses.EventsResponse
import okhttp3.OkHttpClient
import retrofit2.Response
import retrofit2.Retrofit
import retrofit2.converter.gson.GsonConverterFactory
import retrofit2.converter.scalars.ScalarsConverterFactory
import retrofit2.http.GET
import retrofit2.http.Header
import retrofit2.http.Query
import retrofit2.http.Url

// Used by Retrofit to make API call
// TODO: Write this properly so that it passes any necessary parameters to the URl
interface FetchEventsTestingApi {

    @GET("event/today")
    suspend fun getOrganizationDoors(
        @Url url: String,
        @Header("Authorization") authorization: String,
        @Query("orgId") orgId: Int
    ) : Response<EventsResponse>

    companion object {
        operator fun invoke(
            baseUrl: String,
            networkConnectionInterceptor: NetworkConnectionInterceptor
        ) : BackEndTestingApi {
            val okHttpClient = OkHttpClient.Builder()
                .addInterceptor(networkConnectionInterceptor)
                .retryOnConnectionFailure(false)
                .build()

            return Retrofit.Builder()
                .baseUrl(baseUrl)
                .client(okHttpClient)
                .addConverterFactory(ScalarsConverterFactory.create())
                .addConverterFactory(GsonConverterFactory.create())
                .build()
                .create(BackEndTestingApi::class.java)
        }
    }

}