package com.snmc.scanner.screens.login

import android.os.Bundle
import android.util.Log
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

    private lateinit var viewModel: LoginViewModel

    override fun onCreateView(inflater: LayoutInflater, container: ViewGroup?,
                              savedInstanceState: Bundle?): View? {
        val binding : LoginFragmentBinding = DataBindingUtil.inflate(inflater, R.layout.login_fragment, container, false)
        viewModel = ViewModelProvider(this).get(LoginViewModel::class.java)
        binding.viewmodel = viewModel
        binding.lifecycleOwner = this
        viewModel.loginListener = this
        return inflater.inflate(R.layout.login_fragment, container, false)
    }

    override fun onStarted() {
        Log.i("Login Status", "Login Started")
    }

    override fun onSuccess() {
        Log.i("Login Status", "Login Success")
    }

    override fun onFailure(message: String) {
        Log.i("Login Status", message)
    }

}