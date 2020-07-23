package com.snmc.scanner.data.db

import androidx.lifecycle.LiveData
import androidx.room.Dao
import androidx.room.Insert
import androidx.room.OnConflictStrategy
import androidx.room.Query
import com.snmc.scanner.data.db.entities.CURRENT_ORG_ID
import com.snmc.scanner.data.db.entities.OrganizationEntity

// Interface used to access Organization stored in DB
@Dao
interface OrganizationDao {

    @Insert(onConflict = OnConflictStrategy.REPLACE)
    suspend fun upsert(organizationEntity: OrganizationEntity) : Long

    @Query("SELECT * FROM organization WHERE oid = $CURRENT_ORG_ID")
    fun getOrganization() : LiveData<OrganizationEntity>

}