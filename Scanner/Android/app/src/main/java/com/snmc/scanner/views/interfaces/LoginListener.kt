package com.snmc.scanner.views.interfaces

interface LoginListener {
    fun onStarted()
    fun onSuccess()
    fun onFailure(message: String)
}