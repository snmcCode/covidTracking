package ca.snmc.scanner.data.network

import android.content.Context
import android.net.ConnectivityManager
import android.net.NetworkCapabilities
import android.os.Build
import ca.snmc.scanner.utils.AppErrorCodes
import ca.snmc.scanner.utils.ConnectionTimeoutException
import ca.snmc.scanner.utils.NoInternetException
import okhttp3.Interceptor
import okhttp3.Response
import java.net.SocketTimeoutException
import javax.net.ssl.SSLException
import javax.net.ssl.SSLHandshakeException


// Used to Intercept Network Calls
class NetworkConnectionInterceptor(
    context: Context
) : Interceptor {

    private val applicationContext = context.applicationContext

    // Intercepts Network Calls. Chain contains the request
    override fun intercept(chain: Interceptor.Chain): Response {

        if (!isInternetAvailable()) {
            val error = AppErrorCodes.NO_INTERNET
            throw NoInternetException("${error.code}: ${error.message}")
        }

        try {

            var response: Response = chain.proceed(chain.request())

            var tryCount: Int = 0
            while (!response.isSuccessful && tryCount < 3) {
                tryCount++
                response.close()
                response = chain.proceed(chain.request())
            }

            return response

        } catch (exception: SocketTimeoutException) {
            throw ConnectionTimeoutException("${AppErrorCodes.CONNECTION_TIMEOUT.code}: ${AppErrorCodes.CONNECTION_TIMEOUT.message}")
        } catch (exception: SSLHandshakeException) {
            throw ConnectionTimeoutException("${AppErrorCodes.CONNECTION_TIMEOUT.code}: ${AppErrorCodes.CONNECTION_TIMEOUT.message}")
        } catch (exception: SSLException) {
            throw ConnectionTimeoutException("${AppErrorCodes.CONNECTION_TIMEOUT.code}: ${AppErrorCodes.CONNECTION_TIMEOUT.message}")
        }

    }

    private fun isInternetAvailable() : Boolean {

        val connectivityManager =
            applicationContext.getSystemService(Context.CONNECTIVITY_SERVICE) as ConnectivityManager

        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.M) {
            val capabilities = connectivityManager.getNetworkCapabilities(connectivityManager.activeNetwork)
            if (capabilities != null) {
                when {
                    capabilities.hasTransport(NetworkCapabilities.TRANSPORT_CELLULAR) -> {
                        return true
                    }
                    capabilities.hasTransport(NetworkCapabilities.TRANSPORT_WIFI) -> {
                        return true
                    }
                    capabilities.hasTransport(NetworkCapabilities.TRANSPORT_ETHERNET) -> {
                        return true
                    }
                }
            }
        }

        else {
            try {
                connectivityManager.activeNetworkInfo.also {
                    return it != null && it.isConnected
                }
            } catch (e: Exception) { }
        }
        return false
    }

}