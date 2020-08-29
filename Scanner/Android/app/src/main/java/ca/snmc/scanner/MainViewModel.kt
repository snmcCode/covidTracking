package ca.snmc.scanner

import android.app.Application
import androidx.lifecycle.AndroidViewModel
import ca.snmc.scanner.data.providers.PreferenceProvider
import ca.snmc.scanner.data.repositories.DeviceIORepository

class MainViewModel(
    application: Application,
    private val deviceIORepository: DeviceIORepository,
    private val prefs: PreferenceProvider
) : AndroidViewModel(application) {

    fun getScannerMode(): Int = prefs.readScannerMode()

    fun checkIfVisitLogFileExists() = deviceIORepository.checkIfFileExists()

}