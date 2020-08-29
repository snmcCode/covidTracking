package ca.snmc.scanner.data.providers

import android.Manifest
import android.annotation.SuppressLint
import android.content.Context
import android.content.pm.PackageManager
import android.location.Location
import android.location.LocationManager
import androidx.core.content.ContextCompat
import ca.snmc.scanner.utils.AppErrorCodes
import ca.snmc.scanner.utils.LocationPermissionNotGrantedException
import ca.snmc.scanner.utils.LocationServicesDisabledException
import ca.snmc.scanner.utils.asDeferred
import com.google.android.gms.location.FusedLocationProviderClient
import kotlinx.coroutines.Deferred
import kotlin.math.abs

class LocationProvider(
    private val fusedLocationProviderClient: FusedLocationProviderClient,
    val context: Context
) {

    suspend fun hasLocationChanged(savedLocation: Location) : Boolean {
        val deviceLocation = getLastLocationAsync().await() ?: return false
        val comparisonThreshold = 0.03

        return abs(deviceLocation.latitude - savedLocation.latitude) > comparisonThreshold &&
                abs(deviceLocation.longitude - savedLocation.longitude) > comparisonThreshold
    }

    suspend fun getLocation() : Location? {
        return getLastLocationAsync().await()
    }

    @SuppressLint("MissingPermission")
    private fun getLastLocationAsync() : Deferred<Location?> {
        if (hasLocationPermission()) {
            if (isLocationEnabled()) {

                return fusedLocationProviderClient.lastLocation.asDeferred()

            } else {
                throw LocationServicesDisabledException("${AppErrorCodes.LOCATION_SERVICES_DISABLED.code}: ${AppErrorCodes.LOCATION_SERVICES_DISABLED.message}")
            }
        } else {
            throw LocationPermissionNotGrantedException("${AppErrorCodes.PERMISSIONS_NOT_GRANTED.code}: ${AppErrorCodes.PERMISSIONS_NOT_GRANTED.message}")
        }
    }

    private fun hasLocationPermission(): Boolean {
        return ContextCompat.checkSelfPermission(context.applicationContext,
        Manifest.permission.ACCESS_FINE_LOCATION)  == PackageManager.PERMISSION_GRANTED
    }

    private fun isLocationEnabled(): Boolean {
        val locationManager : LocationManager = context.getSystemService(Context.LOCATION_SERVICE) as LocationManager
        return locationManager.isProviderEnabled(LocationManager.GPS_PROVIDER) && locationManager.isProviderEnabled(LocationManager.NETWORK_PROVIDER)
    }

}