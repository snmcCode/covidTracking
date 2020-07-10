package com.snmc.scanner.views.fragments

import androidx.lifecycle.ViewModelProviders
import android.os.Bundle
import androidx.fragment.app.Fragment
import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import com.snmc.scanner.R
import com.snmc.scanner.viewmodels.ScannerViewModel

class ScannerFragment : Fragment() {

    companion object {
        fun newInstance() = ScannerFragment()
    }

    private lateinit var viewModel: ScannerViewModel

    override fun onCreateView(
        inflater: LayoutInflater, container: ViewGroup?,
        savedInstanceState: Bundle?
    ): View? {
        return inflater.inflate(R.layout.scanner_fragment, container, false)
    }

    override fun onActivityCreated(savedInstanceState: Bundle?) {
        super.onActivityCreated(savedInstanceState)
        viewModel = ViewModelProviders.of(this).get(ScannerViewModel::class.java)
        // TODO: Use the ViewModel
    }

}