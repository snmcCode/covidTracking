package ca.snmc.scanner.data.db.entities

import androidx.annotation.NonNull
import androidx.room.Entity
import androidx.room.PrimaryKey

@Entity
data class EventAttendanceEntity(
    @PrimaryKey @NonNull var id: Int,
    var attendance: Int
)