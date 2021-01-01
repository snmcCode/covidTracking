package ca.snmc.scanner.models

data class EventData(
    var eventId: Int? = null,
    var eventName: String? = null,
    var eventCapacity: Int? = null,
    var eventAttendance: Int? = null
)
