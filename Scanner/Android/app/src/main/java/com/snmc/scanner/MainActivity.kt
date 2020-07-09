package com.snmc.scanner

import android.os.Bundle
import androidx.appcompat.app.AppCompatActivity
import com.snmc.scanner.views.LoginFragment
import com.snmc.scanner.views.ScannerFragment
import com.snmc.scanner.views.SettingsFragment

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