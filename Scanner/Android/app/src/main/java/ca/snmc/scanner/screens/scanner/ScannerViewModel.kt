package ca.snmc.scanner.screens.scanner

import android.app.Application
import androidx.lifecycle.AndroidViewModel
import androidx.lifecycle.LiveData
import ca.snmc.scanner.data.db.entities.AuthenticationEntity
import ca.snmc.scanner.data.db.entities.VisitEntity
import ca.snmc.scanner.data.preferences.PreferenceProvider
import ca.snmc.scanner.data.repositories.BackEndRepository
import ca.snmc.scanner.models.VisitInfo
import ca.snmc.scanner.utils.BackEndApiUtils.generateAuthorization

class ScannerViewModel (
    application: Application,
    private val backEndRepository: BackEndRepository,
    private val prefs: PreferenceProvider
) : AndroidViewModel(application) {

    private lateinit var authentication : LiveData<AuthenticationEntity>
    private lateinit var visitSettings : LiveData<VisitEntity>
    val visitInfo : VisitInfo = VisitInfo(null, null, null, null)

    fun initialize() {
        getSavedAuthentication()
        getSavedVisitSettings()
    }

    suspend fun logVisit() {
        backEndRepository.logVisit(
            authorization = generateAuthorization(authentication.value!!.accessToken!!),
            visitInfo = visitInfo
        )
    }

    private fun getSavedVisitSettings() {
        visitSettings = backEndRepository.getSavedVisitSettings()
    }

    fun getSavedVisitSettingsDirectly() = backEndRepository.getSavedVisitSettings()

    private fun getSavedAuthentication() {
        authentication = backEndRepository.getSavedAuthentication()
    }

    fun getAuthentication() = authentication

    fun writeInternetIsNotAvailable() = prefs.writeInternetIsNotAvailable()

}