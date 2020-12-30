package ca.snmc.scanner.data.repositories

import ca.snmc.scanner.data.providers.VisitLogFileProvider
import ca.snmc.scanner.models.VisitInfo

class DeviceIORepository(
    private val visitLogFileProvider: VisitLogFileProvider
) {

    fun getLogCountObservable() = visitLogFileProvider.getLogCountObservable()

    fun writeLog(visitInfo: VisitInfo) = visitLogFileProvider.writeLog(visitInfo)

    fun readLogs() = visitLogFileProvider.getLogs()

    fun updateLogs(visitInfoList: List<VisitInfo>) = visitLogFileProvider.updateLogs(visitInfoList)

    fun deleteLogs() = visitLogFileProvider.deleteLogs()

    fun checkIfFileExists() = visitLogFileProvider.checkIfFileExists()

    fun updateObsoleteLogs() = visitLogFileProvider.updateObsoleteLogs()

}