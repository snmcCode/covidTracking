package ca.snmc.scanner.screens.splash

import android.app.Application
import androidx.lifecycle.AndroidViewModel
import ca.snmc.scanner.data.providers.PreferenceProvider

class SplashViewModel(
    application: Application,
    private val prefs: PreferenceProvider
) : AndroidViewModel(application) {

    fun isOrgLoggedIn() : Boolean {
        return prefs.readIsUserLoggedIn()
    }

}