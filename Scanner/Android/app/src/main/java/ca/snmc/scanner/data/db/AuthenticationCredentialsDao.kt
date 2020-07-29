package ca.snmc.scanner.data.db

import androidx.lifecycle.LiveData
import androidx.room.Dao
import androidx.room.Insert
import androidx.room.OnConflictStrategy
import androidx.room.Query
import ca.snmc.scanner.data.db.entities.AuthenticationCredentialsEntity
import ca.snmc.scanner.data.db.entities.CURRENT_AUTHENTICATION_CREDENTIALS_ID

// Interface used to access Authentication Credentials temporarily stored in DB

@Dao
interface AuthenticationCredentialsDao {

    @Insert(onConflict = OnConflictStrategy.REPLACE)
    suspend fun upsert(authenticationCredentialsEntity: AuthenticationCredentialsEntity) : Long

    @Query("SELECT * FROM AuthenticationCredentialsEntity WHERE acid = $CURRENT_AUTHENTICATION_CREDENTIALS_ID")
    fun getAuthenticationCredentials(): LiveData<AuthenticationCredentialsEntity>

    @Query("DELETE FROM AuthenticationCredentialsEntity")
    suspend fun delete()

}