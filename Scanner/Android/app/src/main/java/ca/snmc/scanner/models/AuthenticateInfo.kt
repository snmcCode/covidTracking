package ca.snmc.scanner.models

// Used to create Authentication JSON object
data class AuthenticateInfo(
    val grantType: String,
    val clientId: String,
    val clientSecret: String,
    val scope: String
)