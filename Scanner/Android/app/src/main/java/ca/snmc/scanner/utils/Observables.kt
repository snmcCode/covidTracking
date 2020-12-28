package ca.snmc.scanner.utils

import ca.snmc.scanner.data.db.entities.EventEntity
import ca.snmc.scanner.data.db.entities.OrganizationDoorEntity

data class CombinedOrgAuthData(
    val id: Int?,
    val authorization: String?,
    val username: String?,
    val password: String?
)

data class CombinedDoorEventVisitData(
    val doors: List<OrganizationDoorEntity>?,
    var organizationName: String? = null,
    var doorName: String? = null,
    var direction: String? = null,
    val events: List<EventEntity>?
)