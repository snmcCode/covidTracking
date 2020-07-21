package com.snmc.scanner.data.db.entities

import androidx.room.Entity
import androidx.room.PrimaryKey

// This is not the actual org Id, this is simply to prevent id increment in the room database
const val CURRENT_ORG_ID = 0

// Object to hold response from Login and write it into RoomDB
// TODO: Modify these to match the Updated API return params
@Entity
data class Organization(
    var organizationId: Int? = null,
    var organizationName: String? = null,
    var scannerClientId: String? = null,
    var scannerClientSecret: String? = null
) {
    @PrimaryKey
    var oid: Int = CURRENT_ORG_ID
}