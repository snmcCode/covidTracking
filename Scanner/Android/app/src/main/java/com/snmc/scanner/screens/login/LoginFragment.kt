package com.snmc.scanner.screens.login

import android.os.Bundle
import androidx.fragment.app.Fragment
import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.widget.Toast
import androidx.databinding.DataBindingUtil
import androidx.lifecycle.ViewModelProvider
import com.snmc.scanner.R
import com.snmc.scanner.databinding.LoginFragmentBinding

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
        // Toast.makeText(activity, "Login Started", Toast.LENGTH_LONG).show()
    }

    override fun onSuccess() {
        Toast.makeText(activity, "Login Success", Toast.LENGTH_LONG).show()
    }

    override fun onFailure(message: String) {
        // Toast.makeText(activity, message, Toast.LENGTH_LONG).show()
    }

}