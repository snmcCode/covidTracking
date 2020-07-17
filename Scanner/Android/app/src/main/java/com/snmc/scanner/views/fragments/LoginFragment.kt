package com.snmc.scanner.views.fragments

import androidx.lifecycle.ViewModelProviders
import android.os.Bundle
import androidx.fragment.app.Fragment
import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.widget.Toast
import androidx.databinding.DataBindingUtil
import androidx.lifecycle.ViewModelProvider
import com.snmc.scanner.viewmodels.LoginViewModel
import com.snmc.scanner.R
import com.snmc.scanner.databinding.LoginFragmentBinding
import com.snmc.scanner.views.interfaces.LoginListener

class LoginFragment : Fragment(), LoginListener {

    companion object {
        fun newInstance() = LoginFragment()
    }

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
        Toast.makeText(activity, "Login Started", Toast.LENGTH_LONG).show()
    }

    override fun onSuccess() {
        Toast.makeText(activity, "Login Success", Toast.LENGTH_LONG).show()
    }

    override fun onFailure(message: String) {
        Toast.makeText(activity, message, Toast.LENGTH_LONG).show()
    }

}