package ca.snmc.scanner.data.db

import androidx.lifecycle.LiveData
import androidx.room.Dao
import androidx.room.Insert
import androidx.room.OnConflictStrategy
import androidx.room.Query
import ca.snmc.scanner.data.db.entities.EventAttendanceEntity

@Dao
interface EventAttendanceDao {

    @Insert(onConflict =  OnConflictStrategy.IGNORE)
    suspend fun saveEventAttendances(eventAttendanceEntities: List<EventAttendanceEntity>) : List<Long>

    @Query(value = "SELECT * FROM EventAttendanceEntity")
    fun getEventAttendances() : LiveData<List<EventAttendanceEntity>>

    @Query(value = "SELECT attendance FROM EventAttendanceEntity WHERE id=:eventId")
    fun getEventAttendanceById(eventId: Int) : Int

    @Query(value = "SELECT attendance FROM EventAttendanceEntity WHERE id=:eventId")
    fun getEventLiveAttendanceById(eventId: Int) : LiveData<Int>

    @Query(value = "UPDATE EventAttendanceEntity SET attendance = attendance + 1 WHERE id=:eventId")
    fun updateEventAttendanceById(eventId: Int)

    @Query(value = "DELETE FROM EventAttendanceEntity")
    suspend fun deleteAll()

}