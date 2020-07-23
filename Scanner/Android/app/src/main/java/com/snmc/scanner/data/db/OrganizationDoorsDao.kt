package com.snmc.scanner.data.db

import androidx.lifecycle.LiveData
import androidx.room.Dao
import androidx.room.Insert
import androidx.room.OnConflictStrategy
import androidx.room.Query
import com.snmc.scanner.data.db.entities.OrganizationDoor

// Interface used to access OrganizationDoors stored in DB
@Dao
interface OrganizationDoorsDao {

    @Insert(onConflict = OnConflictStrategy.REPLACE)
    suspend fun upsert(organizationDoors: List<OrganizationDoor>) : List<Long>

    @Query("SELECT * FROM OrganizationDoor")
    fun getOrganizationDoors() : LiveData<List<OrganizationDoor>>

}