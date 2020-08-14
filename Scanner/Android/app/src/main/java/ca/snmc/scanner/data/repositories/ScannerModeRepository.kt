package ca.snmc.scanner.data.repositories

import ca.snmc.scanner.data.db.AppDatabase
import ca.snmc.scanner.data.db.entities.ScannerModeEntity

class ScannerModeRepository(
    private val db: AppDatabase
) {

    suspend fun saveScannerMode(scannerModeEntity: ScannerModeEntity) =
        db.getScannerModeDao().upsert(scannerModeEntity)

    fun getScannerMode() = db.getScannerModeDao().getScannerMode()

}