package com.snmc.scanner.data.db.entities

import androidx.room.Entity
import androidx.room.PrimaryKey
import java.lang.System.currentTimeMillis

// This is not the actual org Id, this is simply to prevent id increment in the room database
const val CURRENT_AUTHENTICATION_ID = 0

// Object to hold response from Authenticate and write it into RoomDB
@Entity
data class Authentication(
    var tokenType: String? = null,
    var expiresIn: Int? = null,
    var extExpiresIn: Int? = null,
    var accessToken: String? = null,
    var expireTime: Long? = null,
    var isExpired: Boolean? = null
) {
    @PrimaryKey
    var aid: Int = CURRENT_AUTHENTICATION_ID

    fun setExpireTime() {
        expiresIn = expiresIn ?: 0
        expireTime = currentTimeMillis() + expiresIn!!
    }

    fun setIsExpired() {
        if (currentTimeMillis() == expireTime) {
            isExpired = true
        }
    }
}