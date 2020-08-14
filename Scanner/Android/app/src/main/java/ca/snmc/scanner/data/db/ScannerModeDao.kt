package ca.snmc.scanner.data.db

import androidx.lifecycle.LiveData
import androidx.room.Dao
import androidx.room.Insert
import androidx.room.OnConflictStrategy
import androidx.room.Query
import ca.snmc.scanner.data.db.entities.CURRENT_SCANNER_MODE_ID
import ca.snmc.scanner.data.db.entities.ScannerModeEntity

// Interface used to access ScannerMode stored in DB
@Dao
interface ScannerModeDao {

    @Insert(onConflict = OnConflictStrategy.REPLACE)
    suspend fun upsert(scannerModeEntity: ScannerModeEntity) : Long

    @Query("SELECT * FROM ScannerModeEntity WHERE csmid = $CURRENT_SCANNER_MODE_ID")
    fun getScannerMode() : LiveData<ScannerModeEntity>

    @Query("DELETE FROM ScannerModeEntity")
    suspend fun delete()

}