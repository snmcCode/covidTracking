package com.snmc.scanner.data.db

import androidx.lifecycle.LiveData
import androidx.room.Dao
import androidx.room.Insert
import androidx.room.OnConflictStrategy
import androidx.room.Query
import com.snmc.scanner.data.db.entities.Authentication
import com.snmc.scanner.data.db.entities.CURRENT_AUTHENTICATION_ID
import com.snmc.scanner.data.db.entities.Organization

// Interface used to access Authentication stored in DB
@Dao
interface AuthenticationDao {

    @Insert(onConflict = OnConflictStrategy.REPLACE)
    fun write(organization: Organization): Long

    @Query("SELECT * FROM authentication WHERE aid = $CURRENT_AUTHENTICATION_ID")
    fun getAuthentication(): LiveData<Authentication>

}