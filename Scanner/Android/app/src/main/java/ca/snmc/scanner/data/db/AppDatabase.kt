package ca.snmc.scanner.data.db

import android.content.Context
import androidx.room.Database
import androidx.room.Room
import androidx.room.RoomDatabase
import ca.snmc.scanner.data.db.entities.AuthenticationEntity
import ca.snmc.scanner.data.db.entities.OrganizationDoorEntity
import ca.snmc.scanner.data.db.entities.OrganizationEntity
import ca.snmc.scanner.data.db.entities.VisitEntity

// RoomDB used to hold app-wide data
@Database(
    entities = [
        OrganizationEntity::class,
        AuthenticationEntity::class,
        OrganizationDoorEntity::class,
        VisitEntity::class
    ],
    version = 1
)
abstract class AppDatabase : RoomDatabase() {

    abstract fun getOrganizationDao() : OrganizationDao
    abstract fun getAuthenticationDao() : AuthenticationDao
    abstract fun getOrganizationDoorDao() : OrganizationDoorDao
    abstract fun getVisitDao() : VisitDao

    companion object {

        @Volatile
        private var instance: AppDatabase? = null
        private val LOCK = Any()

        operator fun invoke(context: Context) = instance?: synchronized(LOCK) {
            instance?: buildDatabase(context).also {
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