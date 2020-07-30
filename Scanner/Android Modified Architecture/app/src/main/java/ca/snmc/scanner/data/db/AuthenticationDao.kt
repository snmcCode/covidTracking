package ca.snmc.scanner.data.db

import androidx.lifecycle.LiveData
import androidx.room.*
import ca.snmc.scanner.data.db.entities.AuthenticationEntity
import ca.snmc.scanner.data.db.entities.CURRENT_AUTHENTICATION_ID

// Interface used to access Authentication stored in DB
@Dao
interface AuthenticationDao {

    @Insert(onConflict = OnConflictStrategy.REPLACE)
    suspend fun upsert(authenticationEntity: AuthenticationEntity): Long

    @Query("SELECT * FROM AuthenticationEntity WHERE aid = $CURRENT_AUTHENTICATION_ID")
    fun getAuthentication(): AuthenticationEntity

    @Query("DELETE FROM AuthenticationEntity")
    suspend fun delete()

}