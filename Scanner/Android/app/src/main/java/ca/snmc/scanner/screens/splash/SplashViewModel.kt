package ca.snmc.scanner.screens.splash

import android.app.Application
import androidx.lifecycle.AndroidViewModel
import androidx.lifecycle.LiveData
import androidx.lifecycle.MediatorLiveData
import ca.snmc.scanner.data.db.entities.AuthenticationEntity
import ca.snmc.scanner.data.db.entities.OrganizationEntity
import ca.snmc.scanner.data.preferences.PreferenceProvider
import ca.snmc.scanner.data.repositories.AuthenticateRepository
import ca.snmc.scanner.utils.CombinedOrgAuthData
import ca.snmc.scanner.utils.Coroutines
import java.util.*

class SplashViewModel(
    application: Application,
    private val prefs: PreferenceProvider
) : AndroidViewModel(application) {

    fun isOrgLoggedIn() : Boolean {
        return prefs.readIsUserLoggedIn()
    }

}