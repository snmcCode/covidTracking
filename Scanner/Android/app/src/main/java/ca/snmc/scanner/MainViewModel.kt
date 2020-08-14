package ca.snmc.scanner

import android.app.Application
import androidx.lifecycle.AndroidViewModel
import ca.snmc.scanner.data.preferences.PreferenceProvider

class MainViewModel(
    application: Application,
    private val prefs: PreferenceProvider
) : AndroidViewModel(application) {

    fun getScannerMode(): Int = prefs.readScannerMode()

}