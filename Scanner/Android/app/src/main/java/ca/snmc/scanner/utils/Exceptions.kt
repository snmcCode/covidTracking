package ca.snmc.scanner.utils

import java.io.IOException

class ApiException(message: String) : IOException(message)
class AuthenticationException(message: String) : IOException(message)
class NoInternetException(message: String) : IOException(message)
class ConnectionTimeoutException(message: String) : IOException(message)
class LocationServicesDisabledException(message: String) : IOException(message)
class LocationPermissionNotGrantedException(message: String) : IOException(message)
class DuplicateScanException(message: String) : IOException(message)
class EmptyResponseException(message: String) : IOException(message)
class CapacityReachedException(message: String) : IOException(message)