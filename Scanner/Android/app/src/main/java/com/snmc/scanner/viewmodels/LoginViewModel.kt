package com.snmc.scanner.viewmodels

import android.view.View
import androidx.lifecycle.ViewModel
import com.snmc.scanner.views.interfaces.LoginListener

class LoginViewModel : ViewModel() {

    var email : String? = null;
    var password: String? = null;

    var loginListener : LoginListener? = null

    fun onLoginButtonClick(view: View) {
        loginListener?.onStarted()
        if (email.isNullOrEmpty() || password.isNullOrEmpty()) {
            loginListener?.onFailure("Invalid Email or Password")

            return
        }

        loginListener?.onSuccess()
    }

}