package ca.snmc.scanner.utils

import androidx.lifecycle.LiveData
import androidx.lifecycle.MediatorLiveData
import ca.snmc.scanner.data.db.entities.AuthenticationEntity
import ca.snmc.scanner.data.db.entities.OrganizationEntity

data class CombinedOrgAuthData(
    val id: Int?,
    val authorization: String?,
    val username: String?,
    val password: String?
)