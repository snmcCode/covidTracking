package ca.snmc.scanner.utils

import ca.snmc.scanner.models.Error

object AppErrorCodes {
    val EMPTY_USERNAME = Error(10001, "Please Enter a Username.")
    val EMPTY_PASSWORD = Error(10002, "Please Enter a Password.")
    val NULL_LOGIN_RESPONSE = Error(10003, "Login Failed. Please Try Again.")
    val NULL_AUTHENTICATION_RESPONSE = Error(10004, "Login Failed. Please Try Again.")
    val NULL_ORGANIZATION_DOORS_RESPONSE = Error(10005, "No Doors Found For Your Organization.")
    val NO_INTERNET = Error(10005, "Make Sure You Have An Active Internet Connection.")
}

object ApiErrorCodes {
    val UNAUTHORIZED = Error(401, "Unauthorized")
    val NOT_FOUND_IN_SQL_DATABASE = Error(404, "Invalid Username or Password. Please Check Your Credentials")
    val UNVERIFIED_VISITOR = Error(402, "Unverified Visitor.")
    val GENERAL_ERROR = Error(500, "An Error Occurred in the Server.")
}