package com.snmc.scanner.data.network.responses

// Maps the LoginResponse
// TODO: Modify these to match the Updated API return params
data class LoginResponse (
    val id: Int?,
    val name: String?,
    val clientId: String?,
    val clientSecret: String?
) {
    fun isNotNull() : Boolean {
        return id != null && name != null && clientId != null && clientSecret != null
    }
}