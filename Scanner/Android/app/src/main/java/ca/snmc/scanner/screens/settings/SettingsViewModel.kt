package ca.snmc.scanner.screens.settings

import android.app.Application
import android.view.View
import androidx.databinding.ObservableField
import androidx.lifecycle.AndroidViewModel
import ca.snmc.scanner.data.repositories.BackEndRepository
import ca.snmc.scanner.utils.lazyDeferred

class SettingsViewModel(
    application: Application,
    private val backEndRepository: BackEndRepository
) : AndroidViewModel(application) {

    // Fields connected to layout
    val organizationDoors by lazyDeferred {
        backEndRepository.getOrganizationDoors()
    }
    var organizationDoor : String? = null
    var direction: String? = null

    // Initialize LoginListener
    var settingsListener : SettingsListener? = null

    // onClick called by layout, which also calls the methods that the SettingsFragment has implemented from SettingsListener
    fun onScanButtonClick(view: View) {

    }

    private fun get
}