package com.snmc.scanner.data.db

import android.content.Context
import androidx.room.Database
import androidx.room.Room
import androidx.room.RoomDatabase
import com.snmc.scanner.data.db.entities.Authentication
import com.snmc.scanner.data.db.entities.Organization
import com.snmc.scanner.data.db.entities.OrganizationDoor

// RoomDB used to hold app-wide data
@Database(
    entities = [Organization::class, Authentication::class, OrganizationDoor::class],
    version = 1
)
abstract class AppDatabase : RoomDatabase() {

    abstract fun getOrganizationDao() : OrganizationDao
    abstract fun getAuthenticationDao() : AuthenticationDao
    abstract fun getOrganizationDoorsDao() : OrganizationDoorsDao

    companion object {

        @Volatile
        private var instance: AppDatabase? = null
        private val LOCK = Any()

        operator fun invoke(context: Context) = instance ?: synchronized(LOCK) {
            instance?:buildDatabase(context).also {
                instance = it
            }
        }

        private fun buildDatabase(context: Context) =
            Room.databaseBuilder(
                context.applicationContext,
                AppDatabase::class.java,
                "ScannerDatabase.db"
            ).build()
    }

}