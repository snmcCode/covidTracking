package com.snmc.scanner.screens.login

import android.util.Log
import android.view.View
import androidx.lifecycle.ViewModel

class LoginViewModel : ViewModel() {

    var username : String? = null;
    var password: String? = null;

    var loginListener : LoginListener? = null

    fun onLoginButtonClick(view: View) {
        loginListener?.onStarted()
        Log.d("Username: ",username.toString())
        Log.d("Password: ",password.toString())
        if (username.isNullOrEmpty() || password.isNullOrEmpty()) {
            loginListener?.onFailure("Invalid Username or Password")

            return
        }

        loginListener?.onSuccess()
    }

}