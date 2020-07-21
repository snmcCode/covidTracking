package com.snmc.scanner.screens.login

import android.app.Application
import android.util.Log
import android.view.View
import androidx.lifecycle.AndroidViewModel
import com.snmc.scanner.R
import com.snmc.scanner.data.db.entities.Authentication
import com.snmc.scanner.data.db.entities.Organization
import com.snmc.scanner.data.repositories.AuthenticateRepository
import com.snmc.scanner.data.repositories.LoginRepository
import com.snmc.scanner.models.AuthenticateInfo
import com.snmc.scanner.models.LoginInfo
import com.snmc.scanner.utils.Coroutines

class LoginViewModel(application: Application) : AndroidViewModel(application) {

    // Fields connected to layout
    // TODO: Can these be abstracted?
    var username : String? = null
    var password : String? = null

    // Initialize LoginListener
    var loginListener : LoginListener? = null

    // onClick called by layout, which calls the methods that the LoginFragment has implemented from LoginListener
    fun onLoginButtonClick(view: View) {
        loginListener?.onStarted()
        Log.d("Username", username.toString())
        Log.d("Password", password.toString())
        if (username.isNullOrEmpty() || password.isNullOrEmpty()) {
            loginListener?.onFailure("Invalid Username or Password")
            return
        }

        // Model used to create JSON object
        val loginInfo = LoginInfo(username!!, password!!)

        // Invoke Async API Call as a Coroutine
        Coroutines.main {
            // Call the API from the Repository, Always Pass in the Base URL from the ViewModel
            // TODO: Bad practice to have Repository instance inside ViewModel, will fix later
            val loginResponse = LoginRepository().scannerLogin(baseUrl = getLoginBaseUrl(), loginInfo = loginInfo)
            if (loginResponse.isSuccessful) {
                Log.d("Response Body", if (loginResponse.body() != null) loginResponse.body().toString() else "Failure")

                // TODO: Modify these to match the Updated API return params
                // TODO: Create ResponseMappers for These
                val organization = Organization(
                    organizationId = loginResponse.body()?.organizationId!!,
                    organizationName = loginResponse.body()?.organizationName!!,
                    scannerClientId = loginResponse.body()?.scannerClientId!!,
                    scannerClientSecret = loginResponse.body()?.scannerClientSecret
                )

                loginListener?.onLoginSuccess(organization)

                val authenticateInfo = AuthenticateInfo(
                    grantType = getGrantType(),
                    clientId = organization.scannerClientId!!,
                    clientSecret = organization.scannerClientSecret!!,
                    scope = getScope(organization.scannerClientId!!)
                )

                // Call authenticate API
                // TODO: Bad practice to have Repository instance inside ViewModel, will fix later
                val authenticateResponse = AuthenticateRepository().scannerAuthenticate(baseUrl = getAuthenticateBaseUrl(), authenticateInfo = authenticateInfo)
                if (authenticateResponse.isSuccessful) {
                    Log.d("Response Body", if (authenticateResponse.body() != null) authenticateResponse.body().toString() else "Failure")

                    // TODO: Create ResponseMappers for These
                    val authentication = Authentication(
                        tokenType = authenticateResponse.body()?.token_type!!,
                        expiresIn = authenticateResponse.body()?.expires_in!!,
                        extExpiresIn = authenticateResponse.body()?.ext_expires_in!!,
                        accessToken = authenticateResponse.body()?.access_token!!
                    )
                    authentication.setExpireTime()
                    authentication.setIsExpired()

                    loginListener?.onAuthenticateSuccess(authentication)

                } else {
                    loginListener?.onFailure("Error Code: ${authenticateResponse.code()}\nError Message: ${authenticateResponse.message()}\nError Body: ${authenticateResponse.errorBody().toString()}\nError Response Raw: ${authenticateResponse.raw().toString()}")
                }
            } else {
                loginListener?.onFailure("Error Code: ${loginResponse.code()}")
            }
        }

        // TODO: Add Room database to store retrieved info
    }

    private fun getLoginBaseUrl(): String {
        return "${getApplication<Application>().resources.getString(R.string.login_base_url)}/"
    }

    private fun getAuthenticateBaseUrl(): String {
        val tenantId : String = getTenantId()

        return "${getApplication<Application>().resources.getString(R.string.authentication_base_url)}/${tenantId}/"
    }

    private fun getTenantId() : String {
        return getApplication<Application>().resources.getString(R.string.tenant_id)
    }

    private fun getGrantType() : String {
        return getApplication<Application>().resources.getString(R.string.grant_type)
    }

    private fun getScopeSuffix() : String {
        return getApplication<Application>().resources.getString(R.string.scope_suffix)
    }

    private fun getScope(clientId: String) : String {
        val scopeSuffix : String = getScopeSuffix()

        return "${clientId}/${scopeSuffix}"
    }

}