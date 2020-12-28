package ca.snmc.scanner.data.db

import androidx.lifecycle.LiveData
import androidx.room.Dao
import androidx.room.Insert
import androidx.room.OnConflictStrategy
import androidx.room.Query
import ca.snmc.scanner.data.db.entities.CURRENT_SELECTED_EVENT_ID
import ca.snmc.scanner.data.db.entities.SelectedEventEntity

@Dao
interface SelectedEventDao {

    @Insert(onConflict = OnConflictStrategy.REPLACE)
    suspend fun upsert(selectedEventEntity: SelectedEventEntity) : Long

    @Query("SELECT * FROM SelectedEventEntity WHERE seid = $CURRENT_SELECTED_EVENT_ID")
    fun getSelectedEvent() : LiveData<SelectedEventEntity>

    @Query("DELETE FROM SelectedEventEntity")
    suspend fun delete()

}