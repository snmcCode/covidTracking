package ca.snmc.scanner.data.db.entities

import androidx.room.Entity
import androidx.room.PrimaryKey

// This is not the actual org Id, this is simply to prevent id increment in the room database
const val CURRENT_DEVICE_INFORMATION_ID = 0

// Object to hold device information and write it into RoomDB
@Entity
data class DeviceInformationEntity(
    var deviceId: String? = null,
    var location: String? = null
) {
   @PrimaryKey
   var diid: Int = CURRENT_DEVICE_INFORMATION_ID
}