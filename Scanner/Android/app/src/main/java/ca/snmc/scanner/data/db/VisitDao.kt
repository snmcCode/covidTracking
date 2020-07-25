package ca.snmc.scanner.data.db

import androidx.lifecycle.LiveData
import androidx.room.Dao
import androidx.room.Insert
import androidx.room.OnConflictStrategy
import androidx.room.Query
import ca.snmc.scanner.data.db.entities.CURRENT_VISIT_ID
import ca.snmc.scanner.data.db.entities.VisitEntity

// Interface used to access Visit information stored in DB
@Dao
interface VisitDao {

    @Insert(onConflict = OnConflictStrategy.REPLACE)
    suspend fun upsert(visitEntity: VisitEntity) : Long

    @Query("SELECT * FROM VisitEntity WHERE vid = $CURRENT_VISIT_ID")
    fun getVisit() : LiveData<VisitEntity>

    @Query("DELETE FROM VisitEntity")
    suspend fun delete()

}