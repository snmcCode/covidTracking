package ca.snmc.scanner.utils

import ca.snmc.scanner.data.db.entities.*
import ca.snmc.scanner.data.network.responses.AuthenticateResponse
import ca.snmc.scanner.data.network.responses.EventsResponse
import ca.snmc.scanner.data.network.responses.LoginResponse
import ca.snmc.scanner.data.network.responses.OrganizationDoorsResponse
import ca.snmc.scanner.models.*

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

fun mapEventResponseToEventEntityList(eventsResponse: EventsResponse) : List<EventEntity> {
    val events = mutableListOf<EventEntity>()
    eventsResponse.forEach {
        events.add(mapEventToEventEntity(it))
    }
    return events.toList()
}

fun mapEventResponseToEventAttendanceEntityList(eventsResponse: EventsResponse) : List<EventAttendanceEntity> {
    val events = mutableListOf<EventAttendanceEntity>()
    eventsResponse.forEach {
        events.add(mapEventToEventAttendanceEntity(it))
    }
    return events.toList()
}

fun mapEventToEventEntity(event: Event) : EventEntity {
    return EventEntity(
        time = event.minuteOfTheDay,
        id = event.id,
        hall = event.hall,
        name = event.name,
        capacity = event.capacity
    )
}

fun mapEventToEventAttendanceEntity(event: Event) : EventAttendanceEntity {
    return EventAttendanceEntity(
        id = event.id,
        attendance = 0
    )
}

fun mapEventEntityListToEventListItemList(eventEntities: List<EventEntity>): List<EventListItem> {
    val events = mutableListOf<EventListItem>()
    val eventEntitiesSorted = eventEntities.sortedBy { it.time }
    eventEntitiesSorted.forEach {
        events.add(mapEventEntityToEventListItem(it))
    }
    return events.toList()
}

fun mapEventEntityToEventListItem(eventEntity: EventEntity) : EventListItem {
    return EventListItem(
        id = eventEntity.id,
        name = eventEntity.name,
        hall = eventEntity.hall
    )
}