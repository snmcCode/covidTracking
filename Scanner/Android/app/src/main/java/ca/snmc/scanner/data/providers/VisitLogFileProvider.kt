package ca.snmc.scanner.data.providers

import android.content.Context
import android.util.Log
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

    fun getLogs(): List<VisitInfo>? {
        val rows = readLogs()

        var visitLogsList: List<VisitInfo>? = null
        if (rows.isNotEmpty()) {
            visitLogsList = MutableList<VisitInfo>(rows.size) { index ->

                val eventId : Int? =
                    if (rows[index][4] == "") {
                        null
                    } else {
                        rows[index][4].toInt()
                    }

                return@MutableList VisitInfo(
                    visitorId = UUID.fromString(rows[index][0]),
                    organization = rows[index][1],
                    door = rows[index][2],
                    direction = rows[index][3],
                    eventId = eventId,
                    bookingOverride = null,
                    scannerVersion = rows[index][6],
                    deviceId = rows[index][7],
                    deviceLocation = rows[index][8],
                    dateTimeFromScanner = rows[index][9],
                    anti_duplication_timestamp = rows[index][10].toLong()
                )
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