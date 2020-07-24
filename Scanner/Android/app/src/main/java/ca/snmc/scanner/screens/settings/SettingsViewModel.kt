package ca.snmc.scanner.screens.settings

import android.app.Application
import android.view.View
import androidx.databinding.ObservableField
import androidx.lifecycle.AndroidViewModel
import androidx.lifecycle.LiveData
import androidx.lifecycle.MutableLiveData
import ca.snmc.scanner.data.db.entities.AuthenticationEntity
import ca.snmc.scanner.data.db.entities.OrganizationEntity
import ca.snmc.scanner.data.repositories.BackEndRepository
import ca.snmc.scanner.utils.lazyDeferred

class SettingsViewModel(
    application: Application,
    private val backEndRepository: BackEndRepository
) : AndroidViewModel(application) {

    private lateinit var organization: LiveData<OrganizationEntity>
    private lateinit var authentication: LiveData<AuthenticationEntity>

    // Fields connected to layout
    val organizationDoors by lazyDeferred {
        organization = getSavedOrganization()
        authentication = getSavedAuthentication()
        backEndRepository.getOrganizationDoors()
    }
    var organizationDoor : String? = null
    var direction: String? = null

    fun getSavedOrganization() = backEndRepository.getSavedOrganization()

    fun getSavedAuthentication() = backEndRepository.getSavedAuthentication()

}