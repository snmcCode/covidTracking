package ca.snmc.scanner.data.repositories

import android.app.Application
import android.util.Log
import androidx.lifecycle.LiveData
import ca.snmc.scanner.R
import ca.snmc.scanner.data.db.AppDatabase
import ca.snmc.scanner.data.db.entities.AuthenticationCredentialsEntity
import ca.snmc.scanner.data.db.entities.AuthenticationEntity
import ca.snmc.scanner.data.network.AuthenticateApi
import ca.snmc.scanner.data.network.SafeApiRequest
import ca.snmc.scanner.data.network.responses.AuthenticateResponse
import ca.snmc.scanner.models.AuthenticateInfo
import ca.snmc.scanner.utils.AppErrorCodes
import ca.snmc.scanner.utils.AuthApiUtils.getGrantType
import ca.snmc.scanner.utils.AuthApiUtils.getScope
import ca.snmc.scanner.utils.Coroutines
import ca.snmc.scanner.utils.mapAuthenticateResponseToAuthenticationEntity

// Used to abstract API calls away from ViewModel, returns Response object to ViewModel
class AuthenticateRepository(
    private val application: Application,
    private val api: AuthenticateApi,
    private val db: AppDatabase
) : SafeApiRequest() {

    private lateinit var authenticationCredentials : LiveData<AuthenticationCredentialsEntity>

    init {
        Coroutines.io {
            getSavedAuthenticationCredentials()
        }.invokeOnCompletion {
            Coroutines.main {
                authenticationCredentials.observeForever {
                    if (it?.clientId != null && it.clientSecret != null) {

                        Coroutines.io {
                            try {

                                val authenticateResponse = scannerAuthenticate(AuthenticateInfo(
                                    grantType = getGrantType(),
                                    clientId = it.clientId!!,
                                    clientSecret = it.clientSecret!!,
                                    scope = getScope(scopePrefix = getScopePrefix())
                                ))

                                if (authenticateResponse.isNotNull()) {
                                    saveAuthentication(
                                        authenticationEntity =
                                        mapAuthenticateResponseToAuthenticationEntity(authenticateResponse)
                                    )

                                    // Delete the saved Authentication Credentials
                                    deleteSavedAuthenticationCredentials()

                                } else {
                                    Log.e("Token Refresh Error", AppErrorCodes.NULL_AUTHENTICATION_RESPONSE.message!!)
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

    suspend fun scannerAuthenticate(authenticateInfo: AuthenticateInfo) : AuthenticateResponse {
        return apiRequest {
            api.scannerAuthenticate(
                grantType = authenticateInfo.grantType,
                clientId = authenticateInfo.clientId,
                clientSecret = authenticateInfo.clientSecret,
                scope = authenticateInfo.scope
            )}
    }

    suspend fun saveAuthentication(authenticationEntity: AuthenticationEntity) = db.getAuthenticationDao().upsert(authenticationEntity)

    fun getSavedAuthentication() = db.getAuthenticationDao().getAuthentication()

    private fun getSavedAuthenticationCredentials() {
        authenticationCredentials = db.getAuthenticationCredentialsDao().getAuthenticationCredentials()
    }

    private suspend fun deleteSavedAuthenticationCredentials() = db.getAuthenticationCredentialsDao().delete()

    private fun getScopePrefix() : String = application.applicationContext.getString(
        R.string.backend_base_url)

}