package ca.snmc.scanner.data.network.responses

// Maps the LoginResponse
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