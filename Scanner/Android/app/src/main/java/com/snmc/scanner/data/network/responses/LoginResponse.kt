package com.snmc.scanner.data.network.responses

// Maps the LoginResponse
// TODO: Modify these to match the Updated API return params
data class LoginResponse (
    val organizationId: Int?,
    val organizationName: String?,
    val scannerClientId: String?,
    val scannerClientSecret: String?
)