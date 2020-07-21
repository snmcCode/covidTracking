package com.snmc.scanner.data.network.responses

// Maps the AuthenticateResponse
data class AuthenticateResponse (
    val token_type: String?,
    val expires_in: Int?,
    val ext_expires_in: Int?,
    val access_token: String?
)