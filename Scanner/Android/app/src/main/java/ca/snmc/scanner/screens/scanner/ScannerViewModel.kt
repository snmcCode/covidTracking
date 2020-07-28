package ca.snmc.scanner.screens.scanner

import android.app.Application
import androidx.lifecycle.AndroidViewModel
import androidx.lifecycle.LiveData
import ca.snmc.scanner.data.db.entities.VisitEntity
import ca.snmc.scanner.data.preferences.PreferenceProvider
import ca.snmc.scanner.data.repositories.BackEndRepository
import ca.snmc.scanner.models.Visit

class ScannerViewModel (
    application: Application,
    private val backEndRepository: BackEndRepository,
    private val prefs: PreferenceProvider
) : AndroidViewModel(application) {

    private lateinit var visitInfo : LiveData<VisitEntity>
    val visit : Visit = Visit(null, null, null, null)

    fun initialize() {
        getSavedVisitInfo()
    }

    private fun getSavedVisitInfo() {
        visitInfo = backEndRepository.getSavedVisitInfo()
    }

    fun getVisitInfo() = backEndRepository.getSavedVisitInfo()

}