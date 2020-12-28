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

    fun getLogs(): List<VisitInfo>? {
        val rows = readLogs()

        var visitLogsList: List<VisitInfo>? = null
        if (rows.isNotEmpty()) {
            visitLogsList = MutableList<VisitInfo>(rows.size) { index ->
                VisitInfo(
                    visitorId = UUID.fromString(rows[index][0]),
                    organization = rows[index][1],
                    door = rows[index][2],
                    direction = rows[index][3],
                    scannerVersion = rows[index][4],
                    deviceId = rows[index][5],
                    deviceLocation = rows[index][6],
                    dateTimeFromScanner = rows[index][7],
                    anti_duplication_timestamp = rows[index][8].toLong(),
                    eventId = null,
                    eventCheckInOverride = null
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