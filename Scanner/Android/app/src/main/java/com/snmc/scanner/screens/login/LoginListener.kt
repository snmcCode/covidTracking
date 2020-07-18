package com.snmc.scanner.screens.login

// LoginListener contains the methods implemented by LoginFragment that are called by LoginViewModel
interface LoginListener {
    fun onStarted()
    fun onSuccess()
    fun onFailure(message: String)
}