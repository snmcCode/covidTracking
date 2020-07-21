package com.snmc.scanner.models

// Used to create Authentication JSON object
data class AuthenticateInfo(
    val hostUrl: String,
    val grantType: String,
    val clientId: String,
    val clientSecret: String,
    val scope: String
)