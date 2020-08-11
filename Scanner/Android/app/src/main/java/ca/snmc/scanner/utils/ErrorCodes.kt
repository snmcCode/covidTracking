package ca.snmc.scanner.utils

import ca.snmc.scanner.models.Error

object AppErrorCodes {
    val EMPTY_USERNAME = Error(10001, "Please enter a username.")
    val EMPTY_PASSWORD = Error(10002, "Please enter a password.")
    val NULL_LOGIN_RESPONSE = Error(10003, "Login failed. Please try again.")
    val NULL_AUTHENTICATION_RESPONSE = Error(10004, "Login failed. please try again.")
    val NULL_ORGANIZATION_DOORS_RESPONSE = Error(10005, "No doors found for your organization.")
    val NO_INTERNET = Error(10006, "Make sure you have an active internet connection.")
    val CONNECTION_TIMEOUT = Error(10007, "Connection timed out. Please try again.")
    val PERMISSIONS_NOT_GRANTED = Error(10008, "Cannot proceed until permissions are granted.")
    val PERMISSIONS_NOT_GRANTED_NEVER_ASK_AGAIN = Error(10009, "Please grant permissions in settings.")
    val CAMERA_ERROR = Error(10010, "An error occurred while trying to start the camera. Please restart the app.")
    val INVALID_QR_CODE = Error(10011, "QR code does not contain a valid visitor ID.")
    val MULTIPLE_CODES_SCANNED = Error(10012, "Please scan only one code at a time.")
}

object ApiErrorCodes {
    val UNAUTHORIZED = Error(401, "Unauthorized.")
    val UNVERIFIED_VISITOR = Error(402, "Unverified visitor.")
    val USER_NOT_FOUND_IN_SQL_DATABASE = Error(404, "Invalid username or password. Please check your credentials.")
    val ORGANIZATION_NOT_FOUND_IN_SQL_DATABASE = Error(404, message = "Organization not found. Please contact administrator.")
    val VISITOR_NOT_FOUND_IN_SQL_DATABASE = Error(404, "Visitor not found.")
    // TODO: Decide the error code for INFECTED_VISITOR and the message
    val INFECTED_VISITOR = Error(423, "Infected visitor!")
    val GENERAL_ERROR = Error(500, "An error occurred in the server.")
}

fun getErrorMessage(code: Int) : String? {

    // TODO: Find a better way to do this
    when (code) {
        AppErrorCodes.EMPTY_USERNAME.code -> {
            return AppErrorCodes.EMPTY_USERNAME.message!!
        }
        AppErrorCodes.EMPTY_PASSWORD.code -> {
            return AppErrorCodes.EMPTY_PASSWORD.message!!
        }
        AppErrorCodes.NULL_LOGIN_RESPONSE.code -> {
            return AppErrorCodes.NULL_LOGIN_RESPONSE.message!!
        }
        AppErrorCodes.NULL_AUTHENTICATION_RESPONSE.code -> {
            return AppErrorCodes.NULL_AUTHENTICATION_RESPONSE.message!!
        }
        AppErrorCodes.NULL_ORGANIZATION_DOORS_RESPONSE.code -> {
            return AppErrorCodes.NULL_ORGANIZATION_DOORS_RESPONSE.message!!
        }
        AppErrorCodes.NO_INTERNET.code -> {
            return AppErrorCodes.NO_INTERNET.message!!
        }
        AppErrorCodes.CONNECTION_TIMEOUT.code -> {
            return AppErrorCodes.CONNECTION_TIMEOUT.message!!
        }
        AppErrorCodes.PERMISSIONS_NOT_GRANTED.code -> {
            return AppErrorCodes.PERMISSIONS_NOT_GRANTED.message!!
        }
        AppErrorCodes.PERMISSIONS_NOT_GRANTED_NEVER_ASK_AGAIN.code -> {
            return AppErrorCodes.PERMISSIONS_NOT_GRANTED_NEVER_ASK_AGAIN.message!!
        }
        AppErrorCodes.CAMERA_ERROR.code -> {
            return AppErrorCodes.CAMERA_ERROR.message!!
        }
        AppErrorCodes.INVALID_QR_CODE.code -> {
            return AppErrorCodes.INVALID_QR_CODE.message!!
        }
        AppErrorCodes.MULTIPLE_CODES_SCANNED.code -> {
            return AppErrorCodes.MULTIPLE_CODES_SCANNED.message!!
        }
        ApiErrorCodes.UNAUTHORIZED.code -> {
            return ApiErrorCodes.UNAUTHORIZED.message!!
        }
        ApiErrorCodes.UNVERIFIED_VISITOR.code -> {
            return ApiErrorCodes.UNVERIFIED_VISITOR.message!!
        }
        ApiErrorCodes.USER_NOT_FOUND_IN_SQL_DATABASE.code -> {
            return ApiErrorCodes.USER_NOT_FOUND_IN_SQL_DATABASE.message!!
        }
        ApiErrorCodes.ORGANIZATION_NOT_FOUND_IN_SQL_DATABASE.code -> {
            return ApiErrorCodes.ORGANIZATION_NOT_FOUND_IN_SQL_DATABASE.message!!
        }
        ApiErrorCodes.VISITOR_NOT_FOUND_IN_SQL_DATABASE.code -> {
            return ApiErrorCodes.VISITOR_NOT_FOUND_IN_SQL_DATABASE.message!!
        }
        ApiErrorCodes.INFECTED_VISITOR.code -> {
            return ApiErrorCodes.INFECTED_VISITOR.message!!
        }
        ApiErrorCodes.GENERAL_ERROR.code -> {
            return ApiErrorCodes.GENERAL_ERROR.message!!
        }
    }

    return null

}