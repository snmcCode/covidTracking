package ca.snmc.scanner.models

import java.util.*

data class VisitInfo(
    var visitorId: UUID?,
    var organization: String?,
    var door: String?,
    var direction: String?,
    var scannerVersion: String?
)