package ca.snmc.scanner.screens.login

import ca.snmc.scanner.data.db.entities.AuthenticationEntity
import ca.snmc.scanner.data.db.entities.OrganizationEntity
import ca.snmc.scanner.models.Error

// LoginListener contains the methods implemented by LoginFragment that are called by LoginViewModel
interface LoginListener {
    fun onStarted()
    fun onLoginSuccess(organizationEntity: OrganizationEntity)
    fun onAuthenticateSuccess(authenticationEntity: AuthenticationEntity)
    fun onFailure(error: Error)
}