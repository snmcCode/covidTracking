package ca.snmc.scanner.utils

import ca.snmc.scanner.models.Error

object AppErrorCodes {
    val EMPTY_USERNAME = Error(10001, "Please enter a username.")
    val EMPTY_PASSWORD = Error(10002, "Please enter a password.")
    val NULL_LOGIN_RESPONSE = Error(10003, "Login failed. Please try again.")
    val NULL_AUTHENTICATION_RESPONSE = Error(10004, "Login failed. please try again.")
    val NULL_ORGANIZATION_DOORS_RESPONSE = Error(10005, "No doors found for your organization.")
    val NO_INTERNET = Error(10006, "Make sure you have an active internet connection.")
    val CONNECTION_TIMEOUT = Error(10007, "Connection timed out. Please try again. If this persists, please check your internet connection.")
    val PERMISSIONS_NOT_GRANTED = Error(10008, "Cannot proceed until permissions are granted.")
    val PERMISSIONS_NOT_GRANTED_NEVER_ASK_AGAIN = Error(10009, "Please grant permissions in settings.")
    val CAMERA_ERROR = Error(10010, "An error occurred while trying to start the camera. Please restart the app.")
    val INVALID_QR_CODE = Error(10011, "QR code does not contain a valid visitor ID.")
    val MULTIPLE_CODES_SCANNED = Error(10012, "Please scan only one code at a time.")
    val LOCATION_SERVICES_DISABLED = Error(10013, "Please enable Location Services and enable Improve Location Accuracy.")
    val DUPLICATE_SCAN = Error(10014, "This visitor has already been scanned.")
    val NULL_EVENTS_RESPONSE = Error(10016, "No Events found for your organization for Today.")
    val CAPACITY_REACHED = Error(10017, "Event has reached maximum capacity!")
}

object ApiErrorCodes {
    val UNAUTHORIZED = Error(401, "Unauthorized.")
    val UNVERIFIED_VISITOR = Error(402, "Unverified visitor.")
    val USER_NOT_FOUND_IN_SQL_DATABASE = Error(404, "Invalid username or password. Please check your credentials.")
    val ORGANIZATION_NOT_FOUND_IN_SQL_DATABASE = Error(404, message = "Organization not found. Please contact administrator.")
    val VISITOR_NOT_FOUND_IN_SQL_DATABASE = Error(404, "Visitor not found.")
    val NOT_BOOKED = Error(412, "Visitor not booked.")
    // TODO: Decide the error code for INFECTED_VISITOR and the message
    val INFECTED_VISITOR = Error(423, "Infected visitor!")
    val GENERAL_ERROR = Error(500, "An error occurred in the server.")
}