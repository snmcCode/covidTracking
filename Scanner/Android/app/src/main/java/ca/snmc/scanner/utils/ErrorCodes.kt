package ca.snmc.scanner.utils

import ca.snmc.scanner.models.Error

object AppErrorCodes {
    val EMPTY_USERNAME = Error(10001, "Please enter a username.")
    val EMPTY_PASSWORD = Error(10002, "Please enter a password.")
    val NULL_LOGIN_RESPONSE = Error(10003, "Login failed. Please try again.")
    val NULL_AUTHENTICATION_RESPONSE = Error(10004, "Login failed. please try again.")
    val NULL_ORGANIZATION_DOORS_RESPONSE = Error(10005, "No doors found for your organization.")
    val NO_INTERNET = Error(10005, "Make sure you have an active internet connection.")
    val PERMISSIONS_NOT_GRANTED = Error(10006, "Cannot proceed until permissions are granted.")
    val PERMISSIONS_NOT_GRANTED_NEVER_ASK_AGAIN = Error(10007, "Please grant permissions in settings")
    val CAMERA_ERROR = Error(10008, "An error occurred while trying to start the camera. please restart the app.")
    val INVALID_VISITOR_ID = Error(10009, "QR code does not contain a valid visitor ID.")
    val MULTIPLE_CODES_SCANNED = Error(10010, "Please scan only one code at a time.")
}

object ApiErrorCodes {
    val UNAUTHORIZED = Error(401, "Unauthorized.")
    val NOT_FOUND_IN_SQL_DATABASE = Error(404, "Invalid username or password. Please check your credentials.")
    val UNVERIFIED_VISITOR = Error(402, "Unverified visitor.")
    val GENERAL_ERROR = Error(500, "An error occurred in the server.")
}