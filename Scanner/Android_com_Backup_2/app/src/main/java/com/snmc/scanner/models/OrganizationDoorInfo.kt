package com.snmc.scanner.models

import retrofit2.http.Url

// Used to pass parameters to API
data class OrganizationDoorInfo (
    val url: Url,
    val authorization: String,
    val xFunctionsKey: String
)