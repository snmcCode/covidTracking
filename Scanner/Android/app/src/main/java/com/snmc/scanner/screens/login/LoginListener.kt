package com.snmc.scanner.screens.login

import com.snmc.scanner.data.db.entities.AuthenticationEntity
import com.snmc.scanner.data.db.entities.OrganizationEntity
import com.snmc.scanner.models.Error

// LoginListener contains the methods implemented by LoginFragment that are called by LoginViewModel
interface LoginListener {
    fun onStarted()
    fun onLoginSuccess(organizationEntity: OrganizationEntity)
    fun onAuthenticateSuccess(authenticationEntity: AuthenticationEntity)
    fun onFailure(error: Error)
}