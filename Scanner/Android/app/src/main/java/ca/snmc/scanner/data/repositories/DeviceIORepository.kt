package ca.snmc.scanner.data.repositories

import ca.snmc.scanner.data.providers.VisitLogFileProvider
import ca.snmc.scanner.models.VisitInfo
import com.github.doyaaaaaken.kotlincsv.client.KotlinCsvExperimental

class DeviceIORepository(
    private val visitLogFileProvider: VisitLogFileProvider
) {

    suspend fun writeLog(visitInfo: VisitInfo) = visitLogFileProvider.writeLog(visitInfo)

    suspend fun readLogs() = visitLogFileProvider.getLogs()

    suspend fun deleteLogs() = visitLogFileProvider.deleteLogs()

    fun checkIfFileExists() = visitLogFileProvider.checkIfFileExists()

}