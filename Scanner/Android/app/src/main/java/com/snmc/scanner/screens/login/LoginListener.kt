package com.snmc.scanner.screens.login

import com.snmc.scanner.data.db.entities.Authentication
import com.snmc.scanner.data.db.entities.Organization

// LoginListener contains the methods implemented by LoginFragment that are called by LoginViewModel
interface LoginListener {
    fun onStarted()
    fun onLoginSuccess(organization: Organization)
    fun onAuthenticateSuccess(authentication: Authentication)
    fun onFailure(message: String)
}