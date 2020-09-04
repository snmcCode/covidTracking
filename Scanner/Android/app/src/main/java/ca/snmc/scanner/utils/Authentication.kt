package ca.snmc.scanner.utils

// Test
private const val BREATHING_ROOM: Long = 10 * 1000

//// Breathing room of 5 minutes
//private const val BREATHING_ROOM: Long = 5 * 60 * 1000

fun isAccessTokenExpired(accessTokenExpiryTime: Long) : Boolean {
    // Test
    return (accessTokenExpiryTime - System.currentTimeMillis()) <= BREATHING_ROOM
//    return (accessTokenExpiryTime - System.currentTimeMillis()) <= BREATHING_ROOM
}

fun getAccessTokenExpiryTime(expiryTime: Int) : Long {
    // Test
    return System.currentTimeMillis() + (30 * 1000)
//    return System.currentTimeMillis() + (expiryTime * 1000)
}