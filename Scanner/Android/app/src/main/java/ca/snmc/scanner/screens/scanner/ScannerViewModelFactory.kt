package ca.snmc.scanner.screens.scanner

import android.app.Application
import androidx.lifecycle.ViewModel
import androidx.lifecycle.ViewModelProvider
import ca.snmc.scanner.data.preferences.PreferenceProvider
import ca.snmc.scanner.data.repositories.BackEndRepository

// Used to generate the ViewModel with custom Parameters, allows for dependency injection through constructors
@Suppress("UNCHECKED_CAST")
class ScannerViewModelFactory(
    val application: Application,
    private val backEndRepository: BackEndRepository,
    private val prefs: PreferenceProvider
) : ViewModelProvider.AndroidViewModelFactory(application){
    override fun <T : ViewModel?> create(modelClass: Class<T>): T {
        return ScannerViewModel(application, backEndRepository, prefs) as T
    }
}