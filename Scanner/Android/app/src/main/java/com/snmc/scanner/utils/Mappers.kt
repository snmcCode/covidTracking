package com.snmc.scanner.utils

import com.snmc.scanner.data.db.entities.Authentication
import com.snmc.scanner.data.db.entities.Organization
import com.snmc.scanner.data.network.responses.AuthenticateResponse
import com.snmc.scanner.data.network.responses.LoginResponse
import com.snmc.scanner.models.Error
import com.snmc.scanner.models.LoginInfo

fun mapLoginToOrganization(loginResponse: LoginResponse, loginInfo: LoginInfo) : Organization {
    return Organization(
        id = loginResponse.id,
        name = loginResponse.name,
        username = loginInfo.username,
        password = loginInfo.password
    )
}

fun mapAuthenticationResponseToAuthentication(authenticateResponse: AuthenticateResponse) : Authentication {
    val authentication = Authentication(
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