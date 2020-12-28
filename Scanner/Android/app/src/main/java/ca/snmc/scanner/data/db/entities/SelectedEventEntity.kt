package ca.snmc.scanner.data.db.entities

import androidx.room.Entity
import androidx.room.PrimaryKey

// This is not the actual event Id, this is simply to prevent id increment in the room database
const val CURRENT_SELECTED_EVENT_ID = 0

@Entity
data class SelectedEventEntity(
    var eventId: Int? = null
) {
    @PrimaryKey
    var seid: Int = CURRENT_SELECTED_EVENT_ID
}