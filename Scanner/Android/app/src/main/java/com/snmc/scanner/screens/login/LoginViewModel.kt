package com.snmc.scanner.screens.login

import android.util.Log
import android.view.View
import androidx.lifecycle.ViewModel
import com.snmc.scanner.data.repositories.LoginRepository
import com.snmc.scanner.models.LoginInfo

class LoginViewModel : ViewModel() {

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

        // Call the API from the Repository
        // TODO: Bad practice to have Repository instance inside ViewModel, will fix later
        val loginResponse = LoginRepository().scannerLogin(loginInfo)

        loginListener?.onSuccess(loginResponse)

        // TODO: Add Room database to store retrieved info
    }

}