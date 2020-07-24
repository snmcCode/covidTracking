package ca.snmc.scanner.utils

import java.text.SimpleDateFormat
import java.util.*

private val CALENDAR_FORMAT = "M/d/y H:m:s"

fun getCurrentDateTimeString() : String {
    val calendar = Calendar.getInstance()
    val formatter = SimpleDateFormat(CALENDAR_FORMAT)
    return formatter.format(calendar.time)
}

fun getDateTimeFromString(dateTimeString: String) : Calendar {
    val calendar = Calendar.getInstance()
    val formatter = SimpleDateFormat(CALENDAR_FORMAT)
    calendar.time = formatter.parse(dateTimeString)!!
    return calendar
}

fun getHoursPassed(referenceTime: Calendar) : Int {
    return Calendar.getInstance().get(Calendar.HOUR_OF_DAY) - referenceTime.get(Calendar.HOUR_OF_DAY)
}