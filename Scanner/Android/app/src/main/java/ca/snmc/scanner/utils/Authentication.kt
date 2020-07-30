package ca.snmc.scanner.utils

import java.util.*

// Breathing room of 5 minutes
private const val BREATHING_ROOM: Long = 9 * 60 * 1000 // TODO: Change back to 5 minutes after testing
fun isAccessTokenExpired(accessTokenExpiryTime: Long) : Boolean {
    return (accessTokenExpiryTime - System.currentTimeMillis()) <= BREATHING_ROOM
}

fun getAccessTokenExpiryTime(expiryTime: Int) : Long {
    return System.currentTimeMillis() + (expiryTime * 1000)
}