package ca.snmc.scanner.screens.settings

import ca.snmc.scanner.data.db.entities.AuthenticationEntity
import ca.snmc.scanner.data.db.entities.OrganizationEntity
import ca.snmc.scanner.models.Error

interface SettingsListener {
    fun onStarted()
    fun onDataLoaded()
    fun onFailure(error: Error)
}