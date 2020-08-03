package ca.snmc.scanner.data.db

import androidx.lifecycle.LiveData
import androidx.room.Dao
import androidx.room.Insert
import androidx.room.OnConflictStrategy
import androidx.room.Query
import ca.snmc.scanner.data.db.entities.OrganizationDoorEntity

// Interface used to access OrganizationDoors stored in DB
@Dao
interface OrganizationDoorDao {

    @Insert(onConflict = OnConflictStrategy.REPLACE)
    suspend fun saveOrganizationDoors(organizationDoorEntities: List<OrganizationDoorEntity>) : List<Long>

    @Query("SELECT * FROM OrganizationDoorEntity")
    fun getOrganizationDoors() : LiveData<List<OrganizationDoorEntity>>

    @Query("DELETE FROM OrganizationDoorEntity")
    suspend fun deleteAll()

}