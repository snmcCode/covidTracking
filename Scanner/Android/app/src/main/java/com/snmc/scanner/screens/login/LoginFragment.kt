package com.snmc.scanner.screens.login

import android.os.Bundle
import androidx.fragment.app.Fragment
import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.widget.ProgressBar
import android.widget.Toast
import androidx.lifecycle.LiveData
import androidx.lifecycle.Observer
import androidx.lifecycle.ViewModelProvider
import com.snmc.scanner.databinding.LoginFragmentBinding
import com.snmc.scanner.utils.hide
import com.snmc.scanner.utils.show
import com.snmc.scanner.utils.toast
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
        login_progress_indicator.show()
    }

    override fun onSuccess(loginResponse: LiveData<String>) {
        login_progress_indicator.hide()
        loginResponse.observe(this, Observer {
            activity?.toast(it)
        })
    }

    override fun onFailure(message: String) {
        login_progress_indicator.hide()
        activity?.toast(message)
    }

}