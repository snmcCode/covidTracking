package ca.snmc.scanner.utils

import java.io.IOException

class ApiException(message: String) : IOException(message)
class AppException(message: String) : IOException(message)
class NoInternetException(message: String) : IOException(message)
class ConnectionTimeoutException(message: String) : IOException(message)