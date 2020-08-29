package ca.snmc.scanner.screens.scanner

import android.annotation.SuppressLint
import android.app.Application
import android.location.Location
import androidx.lifecycle.AndroidViewModel
import androidx.lifecycle.LiveData
import androidx.lifecycle.MediatorLiveData
import androidx.lifecycle.MutableLiveData
import ca.snmc.scanner.R
import ca.snmc.scanner.data.db.entities.AuthenticationEntity
import ca.snmc.scanner.data.db.entities.DeviceInformationEntity
import ca.snmc.scanner.data.db.entities.OrganizationEntity
import ca.snmc.scanner.data.db.entities.VisitEntity
import ca.snmc.scanner.data.network.responses.AuthenticateResponse
import ca.snmc.scanner.data.network.responses.LoginResponse
import ca.snmc.scanner.data.providers.PreferenceProvider
import ca.snmc.scanner.data.repositories.*
import ca.snmc.scanner.models.*
import ca.snmc.scanner.utils.*
import ca.snmc.scanner.utils.BackEndApiUtils.generateAuthorization
import java.text.SimpleDateFormat
import java.util.*
import kotlin.collections.ArrayList

private const val LOG_VISIT_BULK_PARTITION_SIZE = 20
// Sixty second timer for visit log upload
private const val VISIT_LOG_UPLOAD_TIMEOUT = 60 * 1000

class ScannerViewModel (
    application: Application,
    private val loginRepository: LoginRepository,
    private val authenticateRepository: AuthenticateRepository,
    private val backEndRepository: BackEndRepository,
    private val deviceInformationRepository: DeviceInformationRepository,
    private val deviceIORepository: DeviceIORepository,
    private val prefs: PreferenceProvider
) : AndroidViewModel(application) {

    private lateinit var organization : LiveData<OrganizationEntity>
    private lateinit var authentication : LiveData<AuthenticationEntity>
    private lateinit var mergedData : MediatorLiveData<CombinedOrgAuthData>
    private lateinit var deviceInformation : LiveData<DeviceInformationEntity>

    private var visitLogUploadProgress = VisitLogUploadProgress()
    private var visitLogUploadProgressObservable : MutableLiveData<VisitLogUploadProgress> = MutableLiveData(
        VisitLogUploadProgress())

    private lateinit var visitSettings : LiveData<VisitEntity>
    val visitInfo : VisitInfo = VisitInfo(null, null, null, null, null, null, null, null)

    var recentScanCode : UUID? = null

    var scanHistory : MutableList<ScanHistoryItem> = ArrayList()

    val scanHistoryObservable : MutableLiveData<MutableList<ScanHistoryItem>> = MutableLiveData()

    fun initialize() {
        getSavedVisitSettings()
        getSavedAuthentication()
        getSavedOrganization()
        getSavedDeviceInformation()
        mergedData = MediatorLiveData()
        mergedData.addSource(organization) {
            mergedData.value = CombinedOrgAuthData(
                id = organization.value?.id,
                authorization = authentication.value?.accessToken,
                username = organization.value?.username,
                password = organization.value?.password
            )
        }
        mergedData.addSource(authentication) {
            mergedData.value = CombinedOrgAuthData(
                id = organization.value?.id,
                authorization = authentication.value?.accessToken,
                username = organization.value?.username,
                password = organization.value?.password
            )
        }
    }

    suspend fun logVisit() {

        val scannerMode = prefs.readScannerMode()

        // Check access token
        if (isAccessTokenExpired(authentication.value!!.expireTime!!)) {

            recentScanCode = visitInfo.visitorId

            // Selection based on Scanner Mode
            val loginResponse : LoginResponse = if (scannerMode == TESTING_MODE) {
                loginRepository.scannerLoginTesting(LoginInfo(
                    username = organization.value!!.username!!,
                    password = organization.value!!.password!!
                ))
            } else {
                loginRepository.scannerLoginProduction(LoginInfo(
                    username = organization.value!!.username!!,
                    password = organization.value!!.password!!
                ))
            }

            if (loginResponse.isNotNull()) {

                // Set Is Internet Available Flag to True in SharedPrefs Due to Successful API Call
                prefs.writeInternetIsAvailable()

                // Selection based on Scanner Mode
                val scopePrefix : String = if (scannerMode == TESTING_MODE) {
                    getScopePrefixTesting()
                } else {
                    getScopePrefixProduction()
                }

                val authenticateInfo = AuthenticateInfo(
                    grantType = AuthApiUtils.getGrantType(),
                    clientId = loginResponse.clientId!!,
                    clientSecret = loginResponse.clientSecret!!,
                    scope = AuthApiUtils.getScope(scopePrefix)
                )

                // Selection based on Scanner Mode
                val authenticateResponse : AuthenticateResponse = if (scannerMode == TESTING_MODE) {
                    authenticateRepository.scannerAuthenticateTesting(authenticateInfo = authenticateInfo)
                } else {
                    authenticateRepository.scannerAuthenticateProduction(authenticateInfo = authenticateInfo)
                }

                if (authenticateResponse.isNotNull()) {

                    // Map AuthenticationResponse to AuthenticationEntity
                    val authentication = mapAuthenticateResponseToAuthenticationEntity(authenticateResponse)
                    // Store AuthenticationEntity in DB
                    authenticateRepository.saveAuthentication(authentication)
                    // Set Is Internet Available Flag to True in SharedPrefs Due to Successful API Call
                    prefs.writeInternetIsAvailable()

                    // Check location
                    if (deviceInformation.value == null || deviceInformation.value!!.location == null) {
                        getDeviceInformationOnStartupAndSet()
                    } else {
                        if (deviceInformationRepository.hasLocationChanged(getLocationFromString(deviceInformation.value!!.location!!))) {
                            refreshDeviceInformationAndSet()
                        }
                    }

                    if (scannerMode == TESTING_MODE) {
                        backEndRepository.logVisitTesting(
                            authorization = generateAuthorization(authentication.accessToken!!),
                            visitInfo = visitInfo
                        )
                    } else {
                        backEndRepository.logVisitProduction(
                            authorization = generateAuthorization(authentication.accessToken!!),
                            visitInfo = visitInfo
                        )
                    }


                } else {
                    val errorMessage = "${AppErrorCodes.NULL_AUTHENTICATION_RESPONSE.code}: ${AppErrorCodes.NULL_AUTHENTICATION_RESPONSE.message}"
                    throw AppException(errorMessage)
                }

            } else {
                val errorMessage = "${AppErrorCodes.NULL_LOGIN_RESPONSE.code}: ${AppErrorCodes.NULL_LOGIN_RESPONSE.message}"
                throw AppException(errorMessage)
            }

        } else {

            if (recentScanCode == null) {

                // Check location
                if (deviceInformation.value == null || deviceInformation.value!!.location == null) {
                    getDeviceInformationOnStartupAndSet()
                } else {
                    if (deviceInformationRepository.hasLocationChanged(getLocationFromString(deviceInformation.value!!.location!!))) {
                        refreshDeviceInformationAndSet()
                    }
                }

                if (scannerMode == TESTING_MODE) {
                    backEndRepository.logVisitTesting(
                        authorization = generateAuthorization(authentication.value!!.accessToken!!),
                        visitInfo = visitInfo
                    )
                } else {
                    backEndRepository.logVisitProduction(
                        authorization = generateAuthorization(authentication.value!!.accessToken!!),
                        visitInfo = visitInfo
                    )
                }

            }

        }
    }

    @SuppressLint("SimpleDateFormat")
    suspend fun logVisitLocal() {

        // Set date on visitInfo
        val simpleDateFormat = SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss'Z'")
        simpleDateFormat.timeZone = TimeZone.getTimeZone("UTC")
        visitInfo.dateTimeFromScanner = simpleDateFormat.format(Date())

        // Write it into the VisitLogs file
        deviceIORepository.writeLog(visitInfo)

    }

    suspend fun uploadVisitLogs() {

        // Start a timer and set a flag to check its expiry
        val timestamp = System.currentTimeMillis()

        // If there are no logs, return
        val visitLogList = deviceIORepository.readLogs()

        // If there are no logs, set the progress bar to 100% to indicate that the process is done and return
        if (visitLogList == null) {
            visitLogUploadProgress.progress = 100
            visitLogUploadProgressObservable.postValue(
                visitLogUploadProgress
            )
            return
        }

        // Set the total items on the progress indicator
        visitLogUploadProgress.totalItems = visitLogList.size
        visitLogUploadProgressObservable.postValue(
            visitLogUploadProgress
        )

        // Partition the logs into chunks that can be processed by the API
        val visitLogListPartitioned = visitLogList.chunked(LOG_VISIT_BULK_PARTITION_SIZE)

        val scannerMode = prefs.readScannerMode()

        // Check access token
        if (isAccessTokenExpired(authentication.value!!.expireTime!!)) {

            recentScanCode = visitInfo.visitorId

            // Selection based on Scanner Mode
            val loginResponse : LoginResponse = if (scannerMode == TESTING_MODE) {
                loginRepository.scannerLoginTesting(LoginInfo(
                    username = organization.value!!.username!!,
                    password = organization.value!!.password!!
                ))
            } else {
                loginRepository.scannerLoginProduction(LoginInfo(
                    username = organization.value!!.username!!,
                    password = organization.value!!.password!!
                ))
            }

            if (loginResponse.isNotNull()) {

                // Set Is Internet Available Flag to True in SharedPrefs Due to Successful API Call
                prefs.writeInternetIsAvailable()

                // Selection based on Scanner Mode
                val scopePrefix : String = if (scannerMode == TESTING_MODE) {
                    getScopePrefixTesting()
                } else {
                    getScopePrefixProduction()
                }

                val authenticateInfo = AuthenticateInfo(
                    grantType = AuthApiUtils.getGrantType(),
                    clientId = loginResponse.clientId!!,
                    clientSecret = loginResponse.clientSecret!!,
                    scope = AuthApiUtils.getScope(scopePrefix)
                )

                // Selection based on Scanner Mode
                val authenticateResponse : AuthenticateResponse = if (scannerMode == TESTING_MODE) {
                    authenticateRepository.scannerAuthenticateTesting(authenticateInfo = authenticateInfo)
                } else {
                    authenticateRepository.scannerAuthenticateProduction(authenticateInfo = authenticateInfo)
                }

                if (authenticateResponse.isNotNull()) {

                    // Map AuthenticationResponse to AuthenticationEntity
                    val authentication = mapAuthenticateResponseToAuthenticationEntity(authenticateResponse)
                    // Store AuthenticationEntity in DB
                    authenticateRepository.saveAuthentication(authentication)
                    // Set Is Internet Available Flag to True in SharedPrefs Due to Successful API Call
                    prefs.writeInternetIsAvailable()

                    visitLogListPartitioned.forEachIndexed { index, visitLogListPartition ->

                        // Check if timeout happened
                        if (System.currentTimeMillis() >= (timestamp + VISIT_LOG_UPLOAD_TIMEOUT)) {
//                            Log.e("Timeout", "Occurred")
                            visitLogUploadProgress.timeout = true
                            visitLogUploadProgress.progress = 100
                            visitLogUploadProgressObservable.postValue(
                                visitLogUploadProgress
                            )
                            return@forEachIndexed
                        }

                        if (scannerMode == TESTING_MODE) {
                            backEndRepository.logVisitBulkTesting(
                                authorization = generateAuthorization(authentication.accessToken!!),
                                visitInfoList = visitLogListPartition
                            )
                        } else {
                            backEndRepository.logVisitBulkProduction(
                                authorization = generateAuthorization(authentication.accessToken!!),
                                visitInfoList = visitLogListPartition
                            )
                        }

                        // Update the saved log visits file to remove the logs that were sent, unless the process is finished
                        if (index != visitLogListPartitioned.lastIndex) {
                            val updatedVisitInfoList: List<VisitInfo> = visitLogListPartitioned.slice((index + 1)..visitLogListPartitioned.lastIndex).flatten()
                            deviceIORepository.updateLogs(updatedVisitInfoList)
                        } else {
                            // Clear the logs if we've reached the last index
                            clearVisitLogs()
                        }

                        updateUploadLogVisitsProgress(
                            index,
                            // This is not always guaranteed to be equal to the LOG_VISIT_BULK_PARTITION_SIZE
                            visitLogListPartition.size,
                            visitLogList.size
                        )

                    }
                } else {
                    val errorMessage = "${AppErrorCodes.NULL_AUTHENTICATION_RESPONSE.code}: ${AppErrorCodes.NULL_AUTHENTICATION_RESPONSE.message}"
                    throw AppException(errorMessage)
                }

            } else {
                val errorMessage = "${AppErrorCodes.NULL_LOGIN_RESPONSE.code}: ${AppErrorCodes.NULL_LOGIN_RESPONSE.message}"
                throw AppException(errorMessage)
            }

        } else {

            visitLogListPartitioned.forEachIndexed { index, visitLogListPartition ->

                // Check if timeout happened
                if (System.currentTimeMillis() >= timestamp + VISIT_LOG_UPLOAD_TIMEOUT) {
                    visitLogUploadProgress.timeout = true
                    visitLogUploadProgress.progress = 100
                    visitLogUploadProgressObservable.postValue(
                        visitLogUploadProgress
                    )
                    return@forEachIndexed
                }

                if (scannerMode == TESTING_MODE) {
                    backEndRepository.logVisitBulkTesting(
                        authorization = generateAuthorization(authentication.value!!.accessToken!!),
                        visitInfoList = visitLogListPartition
                    )
                } else {
                    backEndRepository.logVisitBulkProduction(
                        authorization = generateAuthorization(authentication.value!!.accessToken!!),
                        visitInfoList = visitLogListPartition
                    )
                }

                // Update the saved log visits file to remove the logs that were sent, unless the process is finished
                if (index != visitLogListPartitioned.lastIndex) {
                    val updatedVisitInfoList: List<VisitInfo> = visitLogListPartitioned.slice((index + 1)..visitLogListPartitioned.lastIndex).flatten()
                    deviceIORepository.updateLogs(updatedVisitInfoList)
                } else {
                    // Clear the logs if we've reached the last index
                    clearVisitLogs()
                }

                updateUploadLogVisitsProgress(
                    index,
                    // This is not always guaranteed to be equal to the LOG_VISIT_BULK_PARTITION_SIZE
                    visitLogListPartition.size,
                    visitLogList.size
                )

            }

        }
    }

    private fun updateUploadLogVisitsProgress(index: Int, visitLogListPartitionSize: Int, visitLogListSize: Int) {

        var sum = 0
        // Add the size of the latest partition
        sum += visitLogListPartitionSize
        // If there were previous partitions, add them by multiplying by the partition size
        if (index > 0) {
            sum += index * LOG_VISIT_BULK_PARTITION_SIZE
        }

        visitLogUploadProgress.uploadedItems = sum
        visitLogUploadProgress.progress = ((sum.toDouble() / visitLogListSize.toDouble()) * 100).toInt()
//        Log.d("Progress:", "${visitLogUploadProgress.progress}%")
        visitLogUploadProgressObservable.postValue(
            visitLogUploadProgress
        )
    }

    fun resetVisitLogUploadProgressIndicatorObservable() {
        visitLogUploadProgress.progress = 0
        visitLogUploadProgress.timeout = false
        visitLogUploadProgress.uploadedItems = 0
        visitLogUploadProgress.totalItems = 0
        visitLogUploadProgressObservable.postValue(
            visitLogUploadProgress
        )
    }

    private suspend fun clearVisitLogs() = deviceIORepository.deleteLogs()

    fun setDeviceInformation(deviceId: String, locationString: String) {
        visitInfo.deviceId = deviceId
        visitInfo.deviceLocation = locationString
    }

    suspend fun getDeviceInformationOnStartupAndSet() {
        val deviceId = deviceInformationRepository.getDeviceId()
        val location = deviceInformationRepository.getLocation()

        if (location != null) {
            visitInfo.deviceId = deviceId
            visitInfo.deviceLocation = getLocationString(location)

            // Write Info to DB
            saveDeviceInformation(deviceId, visitInfo.deviceLocation!!)
        }
    }

    private suspend fun refreshDeviceInformationAndSet() {
        val deviceId = deviceInformation.value!!.deviceId!!
        val location = deviceInformationRepository.getLocation()

        if (location != null) {
            visitInfo.deviceId = deviceId
            visitInfo.deviceLocation = getLocationString(location)

            // Write Info to DB
            saveDeviceInformation(deviceId, visitInfo.deviceLocation!!)
        }
    }

    private suspend fun saveDeviceInformation(deviceId: String, locationString: String) {
        deviceInformationRepository.saveDeviceInformation(
            DeviceInformationEntity(
                deviceId, locationString
        ))
    }

    private fun getLocationFromString(locationString: String) : Location {
        val arr = locationString.split(",")
        val latitude = arr[0].replace("Latitude: ", "").toDouble()
        val longitude = arr[1].replace("Longitude: ", "").toDouble()

        val location = Location("")
        location.latitude = latitude
        location.longitude = longitude

        return location
    }

    private fun getLocationString(location: Location): String {
        return "Latitude: ${location.latitude}, Longitude: ${location.longitude}"
    }

    private fun getSavedVisitSettings() {
        visitSettings = backEndRepository.getSavedVisitSettings()
    }

    fun getSavedVisitSettingsDirectly() = backEndRepository.getSavedVisitSettings()

    private fun getSavedAuthentication() {
        authentication = backEndRepository.getSavedAuthentication()
    }

    private fun getSavedOrganization() {
        organization = backEndRepository.getSavedOrganization()
    }

    private fun getSavedDeviceInformation() {
        deviceInformation = deviceInformationRepository.getSavedDeviceInformation()
    }

    fun getSavedDeviceInformationDirectly() = deviceInformationRepository.getSavedDeviceInformation()

    fun getMergedData() = mergedData

    fun getVisitLogUploadProgressBarProgressObservable() = visitLogUploadProgressObservable

    fun writeInternetIsAvailable() = prefs.writeInternetIsAvailable()

    fun writeInternetIsNotAvailable() = prefs.writeInternetIsNotAvailable()

    private fun getScopePrefixProduction() : String = getApplication<Application>().applicationContext.getString(
        R.string.backend_base_url_production)

    private fun getScopePrefixTesting() : String = getApplication<Application>().applicationContext.getString(
        R.string.backend_base_url_testing)

}