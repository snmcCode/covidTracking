package ca.snmc.scanner.utils

import java.util.*

// Breathing room of 5 minutes
private const val BREATHING_ROOM: Long = 5 * 1000 * 60
fun isAccessTokenExpired(accessTokenExpiryTime: Long) : Boolean {
    val currentTime : Long = Calendar.getInstance().timeInMillis
    return currentTime - accessTokenExpiryTime <= BREATHING_ROOM
}

fun getAccessTokenExpiryTime(expiryTime: Int) : Long {
    return Calendar.getInstance().timeInMillis + expiryTime
}