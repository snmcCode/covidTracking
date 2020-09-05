package ca.snmc.scanner.utils

// Breathing room of 5 minutes
private const val BREATHING_ROOM: Long = 5 * 60 * 1000

fun isAccessTokenExpired(accessTokenExpiryTime: Long) : Boolean {
    return (accessTokenExpiryTime - System.currentTimeMillis()) <= BREATHING_ROOM
}

fun getAccessTokenExpiryTime(expiryTime: Int) : Long {
    return System.currentTimeMillis() + (expiryTime * 1000)
}