package ca.snmc.scanner.data.db.entities

import androidx.room.Entity
import androidx.room.PrimaryKey

// This is not the actual org Id, this is simply to prevent id increment in the room database
const val CURRENT_AUTHENTICATION_CREDENTIALS_ID = 0

// Object to hold response from Login and write it into RoomDB. It is only temporary
@Entity
data class AuthenticationCredentialsEntity(
    var clientId: String?,
    var clientSecret: String?
) {
    @PrimaryKey
    var acid: Int = CURRENT_AUTHENTICATION_CREDENTIALS_ID
}