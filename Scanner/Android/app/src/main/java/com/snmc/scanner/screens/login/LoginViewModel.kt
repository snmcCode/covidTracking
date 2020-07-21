package com.snmc.scanner.screens.login

import android.app.Application
import android.util.Log
import android.view.View
import androidx.lifecycle.AndroidViewModel
import com.snmc.scanner.R
import com.snmc.scanner.data.db.entities.Organization
import com.snmc.scanner.data.repositories.LoginRepository
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
            val loginResponse = LoginRepository().scannerLogin(baseUrl = getAuthenticationBaseUrl(), loginInfo = loginInfo)
            if (loginResponse.isSuccessful) {
                Log.d("Response Body", if (loginResponse.body() != null) loginResponse.body().toString() else "Failure")
                // TODO: Modify these to match the Updated API return params
                loginListener?.onLoginSuccess(organization = Organization(
                    organizationId = loginResponse.body()?.organizationId!!,
                    organizationName = loginResponse.body()?.organizationName!!,
                    scannerClientId = loginResponse.body()?.scannerClientId!!,
                    scannerClientSecret = loginResponse.body()?.scannerClientSecret
                ))
            } else {
                loginListener?.onFailure("Error Code: ${loginResponse.code()}")
            }
        }

        // TODO: Add Room database to store retrieved info
    }

    private fun getAuthenticationBaseUrl(): String {
        return getApplication<Application>().resources.getString(R.string.authentication_base_url)
    }

}