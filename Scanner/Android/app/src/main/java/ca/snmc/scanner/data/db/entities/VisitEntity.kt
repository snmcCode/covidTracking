package ca.snmc.scanner.data.db.entities

import androidx.room.Entity
import androidx.room.PrimaryKey

// This is not the actual org Id, this is simply to prevent id increment in the room database
const val CURRENT_VISIT_ID = 0

// Object to hold response from Login and write it into RoomDB
@Entity
data class VisitEntity(
    var organizationName: String? = null,
    var doorName: String? = null,
    var direction: String? = null,
    var scannerVersion: String? = null
) {
    @PrimaryKey
    var vid: Int = CURRENT_VISIT_ID
}