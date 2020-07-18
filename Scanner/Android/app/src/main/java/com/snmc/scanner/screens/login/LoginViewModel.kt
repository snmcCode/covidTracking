package com.snmc.scanner.screens.login

import android.util.Log
import android.view.View
import androidx.lifecycle.ViewModel

class LoginViewModel : ViewModel() {

    // Fields connected to layout
    var username : String? = null;
    var password : String? = null;

    // Initialize LoginListener
    var loginListener : LoginListener? = null

    // onClick called by layout, which calls the methods that the LoginFragment has implemented from LoginListener
    fun onLoginButtonClick(view: View) {
        loginListener?.onStarted()
        Log.d("Username",username.toString())
        Log.d("Password",password.toString())
        if (username.isNullOrEmpty() || password.isNullOrEmpty()) {
            loginListener?.onFailure("Invalid Username or Password")
            return
        }
        loginListener?.onSuccess()
    }

}