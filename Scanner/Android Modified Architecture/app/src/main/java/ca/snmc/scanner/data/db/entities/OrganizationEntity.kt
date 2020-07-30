package ca.snmc.scanner.data.db.entities

import androidx.room.Entity
import androidx.room.PrimaryKey

// This is not the actual org Id, this is simply to prevent id increment in the room database
const val CURRENT_ORG_ID = 0

// Object to hold response from Login and write it into RoomDB
@Entity
data class OrganizationEntity(
    var id: Int? = null,
    var name: String? = null,
    var username: String? = null,
    var password: String? = null
) {
    @PrimaryKey
    var oid: Int = CURRENT_ORG_ID
}