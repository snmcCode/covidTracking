package ca.snmc.scanner.screens.scanner

import android.app.Application
import androidx.lifecycle.ViewModel
import androidx.lifecycle.ViewModelProvider
import ca.snmc.scanner.data.preferences.PreferenceProvider
import ca.snmc.scanner.data.repositories.AuthenticateRepository
import ca.snmc.scanner.data.repositories.BackEndRepository
import ca.snmc.scanner.data.repositories.LoginRepository

// Used to generate the ViewModel with custom Parameters, allows for dependency injection through constructors
@Suppress("UNCHECKED_CAST")
class ScannerViewModelFactory(
    val application: Application,
    private val loginRepository: LoginRepository,
    private val authenticateRepository: AuthenticateRepository,
    private val backEndRepository: BackEndRepository,
    private val prefs: PreferenceProvider
) : ViewModelProvider.AndroidViewModelFactory(application){
    override fun <T : ViewModel?> create(modelClass: Class<T>): T {
        return ScannerViewModel(application, loginRepository, authenticateRepository, backEndRepository, prefs) as T
    }
}