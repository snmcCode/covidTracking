package ca.snmc.scanner.data.db.entities

import androidx.annotation.NonNull
import androidx.room.Entity
import androidx.room.PrimaryKey

@Entity
data class EventEntity(
    var time: Int,
    @PrimaryKey @NonNull var id: Int,
    var hall: String,
    var name: String
)
