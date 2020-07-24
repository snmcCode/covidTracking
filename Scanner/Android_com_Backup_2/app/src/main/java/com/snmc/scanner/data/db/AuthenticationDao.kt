package com.snmc.scanner.data.db

import androidx.lifecycle.LiveData
import androidx.room.Dao
import androidx.room.Insert
import androidx.room.OnConflictStrategy
import androidx.room.Query
import com.snmc.scanner.data.db.entities.AuthenticationEntity
import com.snmc.scanner.data.db.entities.CURRENT_AUTHENTICATION_ID

// Interface used to access Authentication stored in DB
@Dao
interface AuthenticationDao {

    @Insert(onConflict = OnConflictStrategy.REPLACE)
    suspend fun upsert(authenticationEntity: AuthenticationEntity): Long

    @Query("SELECT * FROM authentication WHERE aid = $CURRENT_AUTHENTICATION_ID")
    fun getAuthentication(): LiveData<AuthenticationEntity>

}