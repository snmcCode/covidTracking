package ca.snmc.scanner.models

import java.util.*

data class Visit(
    var visitorId: UUID?,
    var organization: String?,
    var door: String?,
    var direction: String?
)