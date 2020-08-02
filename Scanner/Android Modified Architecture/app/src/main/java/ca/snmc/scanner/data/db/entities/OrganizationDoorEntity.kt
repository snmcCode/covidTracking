package ca.snmc.scanner.data.db.entities

import androidx.annotation.NonNull
import androidx.room.Entity
import androidx.room.PrimaryKey

@Entity
data class OrganizationDoorEntity(
    var organizationId: Int,
    @PrimaryKey @NonNull var doorName: String
)