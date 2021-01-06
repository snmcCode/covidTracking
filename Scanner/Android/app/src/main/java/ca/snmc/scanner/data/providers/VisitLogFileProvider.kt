package ca.snmc.scanner.data.providers

import android.content.Context
import androidx.lifecycle.LiveData
import androidx.lifecycle.MutableLiveData
import ca.snmc.scanner.models.VisitInfo
import com.github.doyaaaaaken.kotlincsv.client.CsvReader
import com.github.doyaaaaaken.kotlincsv.client.CsvWriter
import java.io.File
import java.util.*

class VisitLogFileProvider(
    context: Context,
    fileName: String
) {
    private val file: File = File(context.filesDir, fileName)
    private val csvReader: CsvReader = CsvReader()
    private val csvWriter: CsvWriter = CsvWriter()

    private var logCountObservable : MutableLiveData<Int>

    init {
        checkIfFileExists()
        logCountObservable = MutableLiveData(getLogCount())
    }

    fun checkIfFileExists() {
        if (!file.exists()) {
            file.createNewFile()
        }
    }

    private fun readLogs(): List<List<String>> {
            return csvReader.readAll(file)
    }

    private fun getLogCount(): Int {
        return readLogs().size
    }

    fun updateObsoleteLogs() {
        // Read the old logs
        val logs = readLogs()

        // Check if the old logs are empty
        if (logs.isNotEmpty()) {

            // Generate a list of new logs
            val newLogsVisitInfoList = mutableListOf<VisitInfo>()

            when (logs[0].size) {
                9 -> { // Check if the logs have the old format V2.1 and lower
                    // Copy each log from old list to new list
                    logs.forEach { log ->
                        newLogsVisitInfoList.add(VisitInfo(
                            visitorId = UUID.fromString(log[0]),
                            organization = log[1],
                            door = log[2],
                            direction = log[3],
                            eventId = null,
                            bookingOverride = null,
                            capacityOverride = null,
                            scannerVersion = log[4],
                            deviceId = log[5],
                            deviceLocation = log[6],
                            dateTimeFromScanner = log[7],
                            anti_duplication_timestamp = log[8].toLong()
                        ))
                    }
                }
                11 -> { // Check if the logs have the old format V2.2-V2.3
                    // Copy each log from old list to new list
                    logs.forEach { log ->
                        newLogsVisitInfoList.add(VisitInfo(
                            visitorId = UUID.fromString(log[0]),
                            organization = log[1],
                            door = log[2],
                            direction = log[3],
                            eventId = log[4].toIntOrNull(),
                            bookingOverride = null,
                            capacityOverride = null,
                            scannerVersion = log[6],
                            deviceId = log[7],
                            deviceLocation = log[8],
                            dateTimeFromScanner = log[9],
                            anti_duplication_timestamp = log[10].toLong()
                        ))
                    }
                }
                else -> {
                    // Copy each log from old list to new list
                    logs.forEach { log ->
                        newLogsVisitInfoList.add(VisitInfo(
                            visitorId = UUID.fromString(log[0]),
                            organization = log[1],
                            door = log[2],
                            direction = log[3],
                            eventId = log[4].toIntOrNull(),
                            bookingOverride = null,
                            capacityOverride = log[6].toBoolean(),
                            scannerVersion = log[7],
                            deviceId = log[8],
                            deviceLocation = log[9],
                            dateTimeFromScanner = log[10],
                            anti_duplication_timestamp = log[11].toLong()
                        ))
                    }
                }
            }

            // Delete old logs
            deleteLogs()

            // Write new logs
            updateLogs(newLogsVisitInfoList.toList())
        }
    }

    fun getLogs(): List<VisitInfo>? {
        val rows = readLogs()

        var visitLogsList: List<VisitInfo>? = null
        if (rows.isNotEmpty()) {
            visitLogsList = MutableList(rows.size) { index ->

                when (rows[index].size) {
                    9 -> { // Backwards compatibility for scanner versions 2.1 and below
                        return@MutableList VisitInfo(
                            visitorId = UUID.fromString(rows[index][0]),
                            organization = rows[index][1],
                            door = rows[index][2],
                            direction = rows[index][3],
                            eventId = null,
                            bookingOverride = null,
                            capacityOverride = null,
                            scannerVersion = rows[index][4],
                            deviceId = rows[index][5],
                            deviceLocation = rows[index][6],
                            dateTimeFromScanner = rows[index][7],
                            anti_duplication_timestamp = rows[index][8].toLong()
                        )
                    }
                    11 -> { // Backwards compatibility for scanner versions 2.2-2.3
                        return@MutableList VisitInfo(
                            visitorId = UUID.fromString(rows[index][0]),
                            organization = rows[index][1],
                            door = rows[index][2],
                            direction = rows[index][3],
                            eventId = rows[index][4].toIntOrNull(),
                            bookingOverride = null,
                            capacityOverride = null,
                            scannerVersion = rows[index][6],
                            deviceId = rows[index][7],
                            deviceLocation = rows[index][8],
                            dateTimeFromScanner = rows[index][9],
                            anti_duplication_timestamp = rows[index][10].toLong()
                        )
                    }
                    else -> {
                        return@MutableList VisitInfo(
                            visitorId = UUID.fromString(rows[index][0]),
                            organization = rows[index][1],
                            door = rows[index][2],
                            direction = rows[index][3],
                            eventId = rows[index][4].toIntOrNull(),
                            bookingOverride = null,
                            capacityOverride = rows[index][6].toBoolean(),
                            scannerVersion = rows[index][7],
                            deviceId = rows[index][8],
                            deviceLocation = rows[index][9],
                            dateTimeFromScanner = rows[index][10],
                            anti_duplication_timestamp = rows[index][11].toLong()
                        )
                    }
                }

            }.toList()
        }

        return visitLogsList
    }

    fun writeLog(visitInfo: VisitInfo) {
        csvWriter.open(file, append = true) {
            writeRow(listOf(
                visitInfo.visitorId,
                visitInfo.organization,
                visitInfo.door,
                visitInfo.direction,
                visitInfo.eventId,
                visitInfo.bookingOverride,
                visitInfo.capacityOverride,
                visitInfo.scannerVersion,
                visitInfo.deviceId,
                visitInfo.deviceLocation,
                visitInfo.dateTimeFromScanner,
                visitInfo.anti_duplication_timestamp
            ))
        }

        logCountObservable.postValue(getLogCount())
    }

    fun updateLogs(visitInfoList: List<VisitInfo>) {
        csvWriter.open(file, append = false) {
            visitInfoList.forEach { visitInfo ->
                writeRow(listOf(
                    visitInfo.visitorId,
                    visitInfo.organization,
                    visitInfo.door,
                    visitInfo.direction,
                    visitInfo.eventId,
                    visitInfo.bookingOverride,
                    visitInfo.capacityOverride,
                    visitInfo.scannerVersion,
                    visitInfo.deviceId,
                    visitInfo.deviceLocation,
                    visitInfo.dateTimeFromScanner,
                    visitInfo.anti_duplication_timestamp
                ))
            }
        }

        logCountObservable.postValue(getLogCount())
    }

    fun deleteLogs() {
        if (file.exists()) {
            file.delete()
        }
        checkIfFileExists()

        logCountObservable.postValue(getLogCount())
    }

    fun getLogCountObservable() : LiveData<Int> {
        return logCountObservable
    }

}