package com.snmc.scanner

import android.os.Bundle
import androidx.appcompat.app.AppCompatActivity
import com.snmc.scanner.views.fragments.LoginFragment
import com.snmc.scanner.views.fragments.ScannerFragment
import com.snmc.scanner.views.fragments.SettingsFragment

class MainActivity : AppCompatActivity() {

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_main)

        val loginFragment = LoginFragment();
        val settingsFragment = SettingsFragment();
        val scannerFragment = ScannerFragment();

        supportFragmentManager.beginTransaction().apply {
            replace(R.id.mainFragment, loginFragment)
            commit()
        }
    }

}