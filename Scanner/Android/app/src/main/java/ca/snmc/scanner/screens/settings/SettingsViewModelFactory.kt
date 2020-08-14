package ca.snmc.scanner.screens.settings

import android.app.Application
import androidx.lifecycle.ViewModel
import androidx.lifecycle.ViewModelProvider
import ca.snmc.scanner.data.preferences.PreferenceProvider
import ca.snmc.scanner.data.repositories.AuthenticateRepository
import ca.snmc.scanner.data.repositories.BackEndRepository
import ca.snmc.scanner.data.repositories.LoginRepository
import ca.snmc.scanner.data.repositories.ScannerModeRepository

// Used to generate the ViewModel with custom Parameters, allows for dependency injection through constructors
@Suppress("UNCHECKED_CAST")
class SettingsViewModelFactory(
    val application: Application,
    private val loginRepository: LoginRepository,
    private val authenticateRepository: AuthenticateRepository,
    private val backEndRepository: BackEndRepository,
    private val scannerModeRepository: ScannerModeRepository,
    private val prefs: PreferenceProvider
) : ViewModelProvider.AndroidViewModelFactory(application){
    override fun <T : ViewModel?> create(modelClass: Class<T>): T {
        return SettingsViewModel(application, loginRepository, authenticateRepository, backEndRepository, scannerModeRepository, prefs) as T
    }
}