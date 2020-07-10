package com.snmc.scanner.views.fragments

import androidx.lifecycle.ViewModelProviders
import android.os.Bundle
import androidx.fragment.app.Fragment
import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.widget.Toast
import androidx.lifecycle.ViewModelProvider
import com.snmc.scanner.viewmodels.LoginViewModel
import com.snmc.scanner.R
import com.snmc.scanner.views.interfaces.LoginListener

class LoginFragment : Fragment(), LoginListener {

    companion object {
        fun newInstance() = LoginFragment()
    }

    private lateinit var viewModel: LoginViewModel

    override fun onCreateView(inflater: LayoutInflater, container: ViewGroup?,
                              savedInstanceState: Bundle?): View? {
        return inflater.inflate(R.layout.login_fragment, container, false)
    }

    override fun onActivityCreated(savedInstanceState: Bundle?) {
        super.onActivityCreated(savedInstanceState)
        viewModel = ViewModelProvider(this).get(LoginViewModel::class.java)
        // TODO: Use the ViewModel
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