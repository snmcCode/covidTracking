package ca.snmc.scanner.models

import java.util.*

data class Visit(
    val visitorId: UUID?,
    var organization: String?,
    var door: String?,
    var direction: String?
)