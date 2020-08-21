package ca.snmc.scanner.screens.splash

import android.app.Application
import androidx.lifecycle.ViewModel
import androidx.lifecycle.ViewModelProvider
import ca.snmc.scanner.data.providers.PreferenceProvider

// Used to generate the ViewModel with custom Parameters, allows for dependency injection through constructors
@Suppress("UNCHECKED_CAST")
class SplashViewModelFactory(
    val application: Application,
    private val prefs: PreferenceProvider
) : ViewModelProvider.AndroidViewModelFactory(application) {
    override fun <T : ViewModel?> create(modelClass: Class<T>): T {
        return SplashViewModel(application, prefs) as T
    }
}