package ca.snmc.scanner.data.db

import androidx.lifecycle.LiveData
import androidx.room.Dao
import androidx.room.Insert
import androidx.room.OnConflictStrategy
import androidx.room.Query
import ca.snmc.scanner.data.db.entities.CURRENT_DEVICE_INFORMATION_ID
import ca.snmc.scanner.data.db.entities.DeviceInformationEntity

// Interface used to access device information stored in DB
@Dao
interface DeviceInformationDao {

    @Insert(onConflict = OnConflictStrategy.REPLACE)
    suspend fun upsert(deviceInformationEntity: DeviceInformationEntity) : Long

    @Query("SELECT * FROM DeviceInformationEntity WHERE diid = $CURRENT_DEVICE_INFORMATION_ID")
    fun getDeviceInformation() : LiveData<DeviceInformationEntity>

    @Query("DELETE FROM DeviceInformationEntity")
    suspend fun delete()

}