package ca.snmc.scanner.data.network.apis.testing

import ca.snmc.scanner.data.network.NetworkConnectionInterceptor
import ca.snmc.scanner.data.network.responses.EventsResponse
import ca.snmc.scanner.data.network.responses.OrganizationDoorsResponse
import ca.snmc.scanner.models.VisitInfo
import okhttp3.OkHttpClient
import retrofit2.Response
import retrofit2.Retrofit
import retrofit2.converter.gson.GsonConverterFactory
import retrofit2.converter.scalars.ScalarsConverterFactory
import retrofit2.http.*

// Used by Retrofit to make API call
interface BackEndTestingApi {

    @GET
    suspend fun getOrganizationDoors(
        @Url url: String,
        @Header("Authorization") authorization: String
    ) : Response<OrganizationDoorsResponse>

    @POST("visits")
    suspend fun logVisit(
        @Header("Authorization") authorization: String,
        @Body visitInfo: VisitInfo
    ) : Response<String>

    @POST("visits/bulk")
    suspend fun logVisitBulk(
        @Header("Authorization") authorization: String,
        @Body visitInfoList: List<VisitInfo>
    ) : Response<String>

    // Get Today's Events
    @GET("event/today")
    suspend fun getEvents(
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