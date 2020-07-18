package com.snmc.scanner.screens.login

interface LoginListener {
    fun onStarted()
    fun onSuccess()
    fun onFailure(message: String)
}