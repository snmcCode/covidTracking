package ca.snmc.scanner.models

import java.util.*

data class VisitInfo(
    var visitorId: UUID?,
    var organization: String?,
    var door: String?,
    var direction: String?,
    var scannerVersion: String?,
    var deviceId: String?,
    var deviceLocation: String?,
    var dateTimeFromScanner: String?,
    var anti_duplication_timestamp: Long?
)