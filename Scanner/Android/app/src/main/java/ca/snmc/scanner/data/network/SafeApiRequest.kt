package ca.snmc.scanner.data.network

import ca.snmc.scanner.utils.ApiException
import retrofit2.Response

// Used to Abstract Making API Calls
abstract class SafeApiRequest {

    suspend fun<T: Any> apiRequest(call: suspend () -> Response<T>): T {
        val response = call.invoke()

        if (response.isSuccessful) {
//            Log.d("Response Body", response.body()!!.toString())
            return response.body()!!
        } else {
            val errorCode = response.code()
            val errorMessage = response.errorBody()?.charStream().use { it?.readText() } // Optimal way to convert body to string

            val error = "$errorCode: $errorMessage"

            throw ApiException(error)
        }
    }

}