package ca.snmc.scanner.data.db

import android.content.Context
import androidx.room.Database
import androidx.room.Room
import androidx.room.RoomDatabase
import ca.snmc.scanner.data.db.entities.*

// RoomDB used to hold app-wide data
@Database(
    entities = [
        OrganizationEntity::class,
        AuthenticationEntity::class,
        OrganizationDoorEntity::class,
        VisitEntity::class,
        DeviceInformationEntity::class
    ],
    version = 7
)
abstract class AppDatabase : RoomDatabase() {

    abstract fun getOrganizationDao() : OrganizationDao
    abstract fun getAuthenticationDao() : AuthenticationDao
    abstract fun getOrganizationDoorDao() : OrganizationDoorDao
    abstract fun getVisitDao() : VisitDao
    abstract fun getDeviceInformationDao() : DeviceInformationDao

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