package com.snmc.scanner

import android.os.Bundle
import androidx.appcompat.app.AppCompatActivity
import com.snmc.scanner.screens.login.LoginFragment
import com.snmc.scanner.screens.scanner.ScannerFragment
import com.snmc.scanner.screens.settings.SettingsFragment

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