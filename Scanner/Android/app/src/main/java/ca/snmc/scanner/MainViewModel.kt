package ca.snmc.scanner

import android.app.Application
import android.content.SharedPreferences
import androidx.lifecycle.AndroidViewModel
import ca.snmc.scanner.data.preferences.PreferenceProvider
import ca.snmc.scanner.utils.PRODUCTION_MODE
import ca.snmc.scanner.utils.UNDEFINED

class MainViewModel(
    application: Application,
    private val prefs: PreferenceProvider
) : AndroidViewModel(application) {

    fun getScannerMode(): Int {
        val scannerMode: Int = prefs.readScannerMode()

        return if (scannerMode != UNDEFINED) {
            prefs.writeProductionMode()
            scannerMode
        } else {
            PRODUCTION_MODE
        }
    }

}