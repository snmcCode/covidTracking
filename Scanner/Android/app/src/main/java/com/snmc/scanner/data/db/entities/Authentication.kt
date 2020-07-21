package com.snmc.scanner.data.db.entities

import androidx.room.Entity
import androidx.room.PrimaryKey
import java.lang.System.currentTimeMillis

// This is not the actual org Id, this is simply to prevent id increment in the room database
const val CURRENT_AUTHENTICATION_ID = 0

// Object to hold response from Authenticate and write it into RoomDB
@Entity
data class Authentication(
    var token_type: String? = null,
    var expires_in: Int? = null,
    var ext_expires_in: Int? = null,
    var access_token: String? = null,
    var expireTime: Long? = null,
    var isExpired: Boolean? = null
) {
    @PrimaryKey
    var aid: Int = CURRENT_AUTHENTICATION_ID

    fun setExpireTime() {
        expires_in = expires_in ?: 0
        expireTime = currentTimeMillis() + expires_in!!
    }

    fun setIsExpired() {
        if (currentTimeMillis() == expireTime) {
            isExpired = true
        }
    }
}