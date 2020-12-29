package ca.snmc.scanner.models

data class Event (
    val minuteOfTheDay: Int,
    val id: Int,
    val name: String,
    val hall: String,
    val capacity: Int
)