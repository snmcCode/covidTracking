package ca.snmc.scanner

import android.app.Application
import androidx.lifecycle.ViewModel
import androidx.lifecycle.ViewModelProvider
import ca.snmc.scanner.data.providers.PreferenceProvider
import ca.snmc.scanner.data.repositories.BackEndRepository
import ca.snmc.scanner.data.repositories.DeviceIORepository

// Used to generate the ViewModel with custom Parameters, allows for dependency injection through constructors
@Suppress("UNCHECKED_CAST")
class MainViewModelFactory(
    val application: Application,
    private val backEndRepository: BackEndRepository,
    private val deviceIORepository: DeviceIORepository,
    private val prefs: PreferenceProvider
) : ViewModelProvider.AndroidViewModelFactory(application) {
    override fun <T : ViewModel?> create(modelClass: Class<T>): T {
        return MainViewModel(application, backEndRepository, deviceIORepository, prefs) as T
    }
}