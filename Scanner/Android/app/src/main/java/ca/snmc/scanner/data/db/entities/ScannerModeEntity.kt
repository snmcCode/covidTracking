package ca.snmc.scanner.data.db.entities

import androidx.room.Entity
import androidx.room.PrimaryKey

// This is  to prevent id increment in the room database
const val CURRENT_SCANNER_MODE_ID = 0

const val PRODUCTION_MODE = 0
const val TESTING_MODE = 0

@Entity
data class ScannerModeEntity(
    var mode: Int? = null
) {
    @PrimaryKey
    var csmid: Int = CURRENT_SCANNER_MODE_ID
}