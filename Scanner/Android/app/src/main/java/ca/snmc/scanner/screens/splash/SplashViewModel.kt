package ca.snmc.scanner.screens.splash

import android.app.Application
import androidx.lifecycle.AndroidViewModel
import ca.snmc.scanner.data.preferences.PreferenceProvider
import ca.snmc.scanner.data.repositories.AuthenticateRepository
import ca.snmc.scanner.data.repositories.LoginRepository

class SplashViewModel(
    application: Application,
    private val loginRepository: LoginRepository,
    private val authenticateRepository: AuthenticateRepository,
    private val prefs: PreferenceProvider
) : AndroidViewModel(application) {
    // TODO: Implement the ViewModel
}