package com.snmc.scanner.data.db.entities

import androidx.annotation.NonNull
import androidx.room.Entity
import androidx.room.PrimaryKey

@Entity
data class OrganizationDoor(
    var organizationId: Int,
    @PrimaryKey @NonNull var doorName: String
)