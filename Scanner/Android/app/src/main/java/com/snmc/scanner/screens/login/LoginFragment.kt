package com.snmc.scanner.screens.login

import android.os.Bundle
import android.util.Log
import androidx.fragment.app.Fragment
import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.widget.ProgressBar
import android.widget.Toast
import androidx.lifecycle.LiveData
import androidx.lifecycle.Observer
import androidx.lifecycle.ViewModelProvider
import com.snmc.scanner.data.db.entities.Authentication
import com.snmc.scanner.data.db.entities.Organization
import com.snmc.scanner.databinding.LoginFragmentBinding
import com.snmc.scanner.utils.*
import kotlinx.android.synthetic.main.login_fragment.*

class LoginFragment : Fragment(), LoginListener {

    // Initialize ViewModel
    private lateinit var viewModel: LoginViewModel

    override fun onCreateView(inflater: LayoutInflater, container: ViewGroup?,
                              savedInstanceState: Bundle?): View? {
        // Binding object that connects to the layout
        val binding : LoginFragmentBinding = LoginFragmentBinding.inflate(inflater, container, false)

        // ViewModel
        viewModel = ViewModelProvider(this).get(LoginViewModel::class.java)

        // Set ViewModel on Binding object
        binding.viewmodel = viewModel

        // Set LifecycleOwner on Binding object
        binding.lifecycleOwner = this

        // Fragment implements methods defined in LoginListener which are called by ViewModel
        viewModel.loginListener = this

        // Return the View at the Root of the Binding object
        return binding.root
    }

    override fun onStarted() {
        disableUi()
    }

    override fun onLoginSuccess(organization: Organization) {
        // activity?.toast("Organization Id: ${organization.organizationId}, Organization Name: ${organization.organizationName}, ClientId: ${organization.scannerClientId}, ClientSecret: ${organization.scannerClientSecret}")
        Log.d("Organization", "Organization Id: ${organization.organizationId}, Organization Name: ${organization.organizationName}, ClientId: ${organization.scannerClientId}, ClientSecret: ${organization.scannerClientSecret}")
    }

    override fun onAuthenticateSuccess(authentication: Authentication) {
        enableUi()
        activity?.toast("Token Type: ${authentication.tokenType}, Expires In: ${authentication.expiresIn}, Ext Expires In: ${authentication.extExpiresIn}, Access Token: ${authentication.accessToken}")
        Log.d("Organization", "Token Type: ${authentication.tokenType}, Expires In: ${authentication.expiresIn}, Ext Expires In: ${authentication.extExpiresIn}, Access Token: ${authentication.accessToken}")
    }

    override fun onFailure(message: String) {
        enableUi()
        activity?.toast(message)
        Log.e("Error Message", message)
    }

    private fun disableUi() {
        login_progress_indicator.show()
        login_button.disable()
    }

    private fun enableUi() {
        login_progress_indicator.hide()
        login_button.enable()
    }
}