package ca.snmc.scanner.utils

import ca.snmc.scanner.data.db.entities.AuthenticationEntity
import ca.snmc.scanner.data.db.entities.OrganizationDoorEntity
import ca.snmc.scanner.data.db.entities.OrganizationEntity
import ca.snmc.scanner.data.network.responses.AuthenticateResponse
import ca.snmc.scanner.data.network.responses.LoginResponse
import ca.snmc.scanner.data.network.responses.OrganizationDoorsResponse
import ca.snmc.scanner.models.Error
import ca.snmc.scanner.models.LoginInfo
import ca.snmc.scanner.models.OrganizationDoor

fun mapLoginToOrganizationEntity(loginResponse: LoginResponse, loginInfo: LoginInfo) : OrganizationEntity {
    return OrganizationEntity(
        id = loginResponse.id,
        name = loginResponse.name,
        username = loginInfo.username,
        password = loginInfo.password
    )
}

fun mapAuthenticateResponseToAuthenticationEntity(authenticateResponse: AuthenticateResponse) : AuthenticationEntity {
    val authentication = AuthenticationEntity(
        tokenType = authenticateResponse.token_type,
        expiresIn = authenticateResponse.expires_in,
        extExpiresIn = authenticateResponse.ext_expires_in,
        accessToken = authenticateResponse.access_token,
        // Get the current time in in milliseconds and add the expiry time multiplied by 60 * 1000 (since it is originally in minutes)
        expireTime = getAccessTokenExpiryTime(authenticateResponse.expires_in!!)
    )
    return authentication
}

fun mapErrorStringToError(errorString: String) : Error {
    val errorStringSplit = errorString.split(": ")
    val code: Int = errorStringSplit[0].toInt()
    val message: String = errorStringSplit[1]
    return Error(code, message)
}

fun mapOrganizationDoorResponseToOrganizationDoorEntityList(organizationDoorsResponse: OrganizationDoorsResponse) : List<OrganizationDoorEntity> {
    val organizationDoors = mutableListOf<OrganizationDoorEntity>()
    organizationDoorsResponse.forEach {
        organizationDoors.add(mapOrganizationDoorToOrganizationDoorEntity(it))
    }
    return organizationDoors.toList()
}

fun mapOrganizationDoorToOrganizationDoorEntity(organizationDoor: OrganizationDoor) : OrganizationDoorEntity {
    return OrganizationDoorEntity(
        organizationId = organizationDoor.organizationId,
        doorName = organizationDoor.doorName
    )
}