package ca.snmc.scanner.data.db

import androidx.lifecycle.LiveData
import androidx.room.Dao
import androidx.room.Insert
import androidx.room.OnConflictStrategy
import androidx.room.Query
import ca.snmc.scanner.data.db.entities.EventEntity

@Dao
interface EventDao {

    @Insert(onConflict = OnConflictStrategy.REPLACE)
    suspend fun saveEvents(eventEntities: List<EventEntity>) : List<Long>

    @Query(value = "SELECT * FROM EventEntity")
    fun getEvents() : LiveData<List<EventEntity>>

    @Query(value = "SELECT * FROM EventEntity WHERE id=:eventId")
    fun getEventById(eventId: Int) : EventEntity

    @Query(value = "SELECT capacity FROM EventEntity WHERE id=:eventId")
    fun getEventCapacityById(eventId: Int) : Int

    @Query(value = "SELECT currentNumberOfVisitors FROM EventEntity WHERE id=:eventId")
    fun getEventCurrentNumberOfVisitorsById(eventId: Int) : Int

    @Query(value = "UPDATE EventEntity SET currentNumberOfVisitors = currentNumberOfVisitors + 1 WHERE id=:eventId")
    suspend fun updateEventCurrentNumberOfVisitorsById(eventId: Int)

    @Query(value = "DELETE FROM EventEntity")
    suspend fun deleteAll()

}