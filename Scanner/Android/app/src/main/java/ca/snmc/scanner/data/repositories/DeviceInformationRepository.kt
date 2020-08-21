package ca.snmc.scanner.data.repositories

import android.location.Location
import ca.snmc.scanner.data.db.AppDatabase
import ca.snmc.scanner.data.db.entities.DeviceInformationEntity
import ca.snmc.scanner.data.providers.DeviceIdProvider
import ca.snmc.scanner.data.providers.LocationProvider

class DeviceInformationRepository(
    private val locationProvider: LocationProvider,
    private val deviceIdProvider: DeviceIdProvider,
    private val db: AppDatabase
) {

    suspend fun saveDeviceInformation(deviceInformationEntity: DeviceInformationEntity) =
        db.getDeviceInformationDao().upsert(deviceInformationEntity)

    fun getSavedDeviceInformation() = db.getDeviceInformationDao().getDeviceInformation()

    fun getDeviceId() = deviceIdProvider.getDeviceId()

    suspend fun getLocation() = locationProvider.getLocation()

    suspend fun hasLocationChanged(savedLocation: Location) =
        locationProvider.hasLocationChanged(savedLocation)

}