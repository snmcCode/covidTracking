package ca.snmc.scanner.data.db.entities

import androidx.room.Entity
import androidx.room.PrimaryKey

// This is not the actual org Id, this is simply to prevent id increment in the room database
const val CURRENT_AUTHENTICATION_ID = 0

// Object to hold response from Authenticate and write it into RoomDB
@Entity
data class AuthenticationEntity(
    var tokenType: String? = null,
    var expiresIn: Int? = null,
    var extExpiresIn: Int? = null,
    var accessToken: String? = null,
    var expireTime: Long? = null
) {
    @PrimaryKey
    var aid: Int = CURRENT_AUTHENTICATION_ID
}