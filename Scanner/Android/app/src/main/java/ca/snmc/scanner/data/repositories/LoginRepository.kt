package ca.snmc.scanner.data.repositories

import android.util.Log
import androidx.lifecycle.LiveData
import ca.snmc.scanner.data.db.AppDatabase
import ca.snmc.scanner.data.db.entities.AuthenticationCredentialsEntity
import ca.snmc.scanner.data.db.entities.AuthenticationEntity
import ca.snmc.scanner.data.db.entities.OrganizationEntity
import ca.snmc.scanner.data.network.LoginApi
import ca.snmc.scanner.data.network.SafeApiRequest
import ca.snmc.scanner.data.network.responses.LoginResponse
import ca.snmc.scanner.models.LoginInfo
import ca.snmc.scanner.utils.AppErrorCodes
import ca.snmc.scanner.utils.Coroutines
import java.lang.Exception

// Five minute breathing room for retrieving new token
private const val TOKEN_REFRESH_THRESHOLD : Long = 5 * 60 * 1000

// Used to abstract API calls away from ViewModel, returns Response object to ViewModel
class LoginRepository(
    private val api: LoginApi,
    private val db: AppDatabase
) : SafeApiRequest() {

    private lateinit var organization : LiveData<OrganizationEntity>
    private lateinit var authentication : LiveData<AuthenticationEntity>

    private var isOrganizationLoaded = false

    init {
        Coroutines.io {
            getSavedOrganization()
            getSavedAuthentication()
        }.invokeOnCompletion {
            Coroutines.main {
                organization.observeForever {
                    if (it?.username != null && it.password != null) {
                        isOrganizationLoaded = true
                    }
                }
                authentication.observeForever {
                    // Check if difference between the current time in milliseconds and the expiry time is less than the refresh threshold time
                    if (it?.expireTime != null && ((it.expireTime!! - System.currentTimeMillis()) < TOKEN_REFRESH_THRESHOLD)) {
                        Log.e("Current Time", System.currentTimeMillis().toString())
                        Log.e("Expire Time", it.expireTime.toString())
                        Log.e("Expiration Difference", (System.currentTimeMillis() - it.expireTime!!).toString())
                        Log.e("Threshold", TOKEN_REFRESH_THRESHOLD.toString())
                        Log.e("Threshold Difference", ((System.currentTimeMillis() - it.expireTime!!) - TOKEN_REFRESH_THRESHOLD).toString())

                        if (isOrganizationLoaded) {

                            Coroutines.io {
                                try {

                                    val loginResponse = scannerLogin(
                                        LoginInfo(
                                            username = organization.value!!.username!!,
                                            password = organization.value!!.password!!
                                        ))

                                    if (loginResponse.isNotNull()) {
                                        // Temporarily saving client credentials in DB
                                        saveAuthenticationCredentials(
                                            AuthenticationCredentialsEntity(
                                                clientId = loginResponse.clientId,
                                                clientSecret = loginResponse.clientSecret
                                            ))
                                    } else {
                                        Log.e("Token Refresh Error", AppErrorCodes.NULL_LOGIN_RESPONSE.message!!)
                                    }

                                } catch (e: Exception) {
                                    Log.e("Token Refresh Exception", e.message!!)
                                }
                            }

                        }

                    }
                }
            }
        }
    }

    suspend fun scannerLogin(loginInfo: LoginInfo) : LoginResponse {
        return apiRequest { api.scannerLogin(loginInfo) }
    }

    suspend fun saveOrganization(organizationEntity: OrganizationEntity) = db.getOrganizationDao().upsert(organizationEntity)

    private fun getSavedOrganization() {
        organization = db.getOrganizationDao().getOrganization()
    }

    private suspend fun saveAuthenticationCredentials(
        authenticationCredentialsEntity: AuthenticationCredentialsEntity
    ) = db.getAuthenticationCredentialsDao().upsert(authenticationCredentialsEntity)

    private fun getSavedAuthentication() {
        authentication = db.getAuthenticationDao().getAuthentication()
    }

}