package com.snmc.scanner.utils

import com.snmc.scanner.data.db.entities.AuthenticationEntity
import com.snmc.scanner.data.db.entities.OrganizationDoorEntity
import com.snmc.scanner.data.db.entities.OrganizationEntity
import com.snmc.scanner.data.network.responses.AuthenticateResponse
import com.snmc.scanner.data.network.responses.LoginResponse
import com.snmc.scanner.data.network.responses.OrganizationDoorsResponse
import com.snmc.scanner.models.Error
import com.snmc.scanner.models.LoginInfo
import com.snmc.scanner.models.OrganizationDoor

fun mapLoginToOrganization(loginResponse: LoginResponse, loginInfo: LoginInfo) : OrganizationEntity {
    return OrganizationEntity(
        id = loginResponse.id,
        name = loginResponse.name,
        username = loginInfo.username,
        password = loginInfo.password
    )
}

fun mapAuthenticationResponseToAuthentication(authenticateResponse: AuthenticateResponse) : AuthenticationEntity {
    val authentication = AuthenticationEntity(
        tokenType = authenticateResponse.token_type,
        expiresIn = authenticateResponse.expires_in,
        extExpiresIn = authenticateResponse.ext_expires_in,
        accessToken = authenticateResponse.access_token
    )
    authentication.setExpireTime()
    authentication.setIsExpired()
    return authentication
}

fun mapErrorStringToError(errorString: String) : Error {
    val errorStringSplit = errorString.split(": ")
    val code: Int = errorStringSplit[0].toInt()
    val message: String = errorStringSplit[1]
    return Error(code, message)
}

fun mapOrganizationDoorResponseToOrganizationDoorEntityList(organizationDoorsResponse: OrganizationDoorsResponse) : List<OrganizationDoor> {
    val organizationDoors =
}

fun mapOrganizationDoorToOrganizationDoorEntity(organizationDoor: OrganizationDoor) : OrganizationDoorEntity {
    return OrganizationDoorEntity(
        organizationId = organizationDoor.organizationId,
        doorName = organizationDoor.doorName
    )
}