package ca.snmc.scanner.data.db

import android.content.Context
import androidx.room.Database
import androidx.room.Room
import androidx.room.RoomDatabase
import androidx.room.migration.Migration
import androidx.sqlite.db.SupportSQLiteDatabase
import ca.snmc.scanner.data.db.entities.*

// RoomDB used to hold app-wide data
@Database(
    entities = [
        OrganizationEntity::class,
        AuthenticationEntity::class,
        OrganizationDoorEntity::class,
        VisitEntity::class,
        DeviceInformationEntity::class,
        EventEntity::class,
        SelectedEventEntity::class
    ],
    version = 4
)
abstract class AppDatabase : RoomDatabase() {

    abstract fun getOrganizationDao() : OrganizationDao
    abstract fun getAuthenticationDao() : AuthenticationDao
    abstract fun getOrganizationDoorDao() : OrganizationDoorDao
    abstract fun getVisitDao() : VisitDao
    abstract fun getDeviceInformationDao() : DeviceInformationDao
    abstract fun getEventDao() : EventDao
    abstract fun getSelectedEventDao() : SelectedEventDao

    companion object {

        @Volatile
        private var instance: AppDatabase? = null
        private val LOCK = Any()

        operator fun invoke(context: Context) = instance?: synchronized(LOCK) {
            instance?: buildDatabase(context).also {
                instance = it
            }
        }

        // Migrations

        private val MIGRATION_1_2 = object : Migration(1, 2) {
            override fun migrate(database: SupportSQLiteDatabase) {
                database.execSQL("CREATE TABLE IF NOT EXISTS `DeviceInformationEntity` (`deviceId` TEXT, `location` TEXT, `diid` INTEGER NOT NULL, PRIMARY KEY(`diid`))")
            }
        }

        private val MIGRATION_2_3 = object : Migration(2, 3) {
            override fun migrate(database: SupportSQLiteDatabase) {
                database.execSQL("CREATE TABLE IF NOT EXISTS `EventEntity` (`time` INTEGER NOT NULL, `id` INTEGER NOT NULL, `hall` TEXT NOT NULL, `name` TEXT NOT NULL, `capacity` INTEGER NOT NULL, `currentNumberOfVisitors` INTEGER NOT NULL, PRIMARY KEY(`id`))")
            }
        }

        private val MIGRATION_3_4 = object : Migration(3, 4) {
            override fun migrate(database: SupportSQLiteDatabase) {
                database.execSQL("CREATE TABLE IF NOT EXISTS `SelectedEventEntity` (`eventId` INTEGER, `seid` INTEGER NOT NULL, PRIMARY KEY(`seid`))")
            }
        }

        private fun buildDatabase(context: Context) =
            Room.databaseBuilder(
                context.applicationContext,
                AppDatabase::class.java,
                "ScannerDatabase.db"
            ).addMigrations(MIGRATION_1_2, MIGRATION_2_3, MIGRATION_3_4).build()
    }

}