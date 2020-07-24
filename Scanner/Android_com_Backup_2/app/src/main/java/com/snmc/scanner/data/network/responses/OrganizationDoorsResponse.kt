package com.snmc.scanner.data.network.responses

import com.snmc.scanner.models.OrganizationDoor
import retrofit2.Response

// Using Delegation to Extend List class
data class OrganizationDoorsResponse(private val organizationDoorsResponse: List<OrganizationDoor>)
    : List<OrganizationDoor> by organizationDoorsResponse