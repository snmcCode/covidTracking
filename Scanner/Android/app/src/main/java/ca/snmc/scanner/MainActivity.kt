package ca.snmc.scanner

import android.os.Build
import android.os.Bundle
import android.util.Log
import android.view.View
import android.view.WindowInsets
import androidx.appcompat.app.AppCompatActivity
import androidx.lifecycle.Observer
import androidx.lifecycle.ViewModelProvider
import androidx.lifecycle.lifecycleScope
import ca.snmc.scanner.databinding.ActivityMainBinding
import ca.snmc.scanner.utils.TESTING_MODE
import com.microsoft.appcenter.AppCenter
import com.microsoft.appcenter.analytics.Analytics
import com.microsoft.appcenter.crashes.Crashes
import com.microsoft.appcenter.distribute.Distribute
import kotlinx.android.synthetic.main.activity_main.*
import kotlinx.coroutines.launch
import org.kodein.di.KodeinAware
import org.kodein.di.android.kodein
import org.kodein.di.generic.instance

class MainActivity : AppCompatActivity(), KodeinAware {

    override val kodein by kodein()

    private lateinit var viewModel: MainViewModel
    private val MainViewModelFactory : MainViewModelFactory by instance()

    private val permissionsRequestCode = 1001

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_main)

        // Binding object that connects to the layout
        val binding = ActivityMainBinding.inflate(layoutInflater)

        viewModel = ViewModelProvider(this, MainViewModelFactory).get(MainViewModel::class.java)

        // Set LifecycleOwner on Binding object
        binding.lifecycleOwner = this

        lifecycleScope.launch {
            // Check if visit logs file exists
            viewModel.checkIfVisitLogFileExists()

            updateTestingModeIndicator()

            // Monitor the log count
            viewModel.getVisitLogFileLogsCount().observe(this@MainActivity, Observer { logCount ->
                updateSavedScanLogsIndicator(logCount)
            })
        }

        AppCenter.setLogLevel(Log.VERBOSE)
        AppCenter.start(
            application, getString(R.string.app_center_secret),
            Distribute::class.java, Analytics::class.java, Crashes::class.java
        )
        Distribute.checkForUpdate()
    }

    fun updateTestingModeIndicator() {
        val scannerMode = viewModel.getScannerMode()
        if (scannerMode == TESTING_MODE) {
            testing_mode_indicator.visibility = View.VISIBLE
        } else {
            testing_mode_indicator.visibility = View.GONE
        }
    }

    private fun updateSavedScanLogsIndicator(logCount : Int) {
        if (logCount > 0) {
            saved_scan_logs_indicator.text = getString(R.string.saved_scan_logs_notification, logCount)
            saved_scan_logs_indicator.visibility = View.VISIBLE
        } else {
            saved_scan_logs_indicator.text = getString(R.string.saved_scan_logs_notification, 0)
            saved_scan_logs_indicator.visibility = View.GONE
        }
    }

    fun fullscreenMode() {
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.R) {
            window.setDecorFitsSystemWindows(false)
            window.insetsController?.hide(WindowInsets.Type.statusBars())
            window.insetsController?.hide(WindowInsets.Type.navigationBars())
            window.insetsController?.hide(WindowInsets.Type.systemBars())
        } else {
            window.decorView.systemUiVisibility = (View.SYSTEM_UI_FLAG_FULLSCREEN
                    or View.SYSTEM_UI_FLAG_LOW_PROFILE
                    or View.SYSTEM_UI_FLAG_LAYOUT_STABLE
                    or View.SYSTEM_UI_FLAG_IMMERSIVE_STICKY
                    or View.SYSTEM_UI_FLAG_LAYOUT_HIDE_NAVIGATION
                    or View.SYSTEM_UI_FLAG_HIDE_NAVIGATION)
        }
    }

    fun windowedMode() {
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.R) {
            window.setDecorFitsSystemWindows(true)
            window.insetsController?.show(WindowInsets.Type.statusBars())
            window.insetsController?.show(WindowInsets.Type.navigationBars())
            window.insetsController?.show(WindowInsets.Type.systemBars())
        } else {
            window.decorView.systemUiVisibility = (View.SYSTEM_UI_FLAG_FULLSCREEN)
        }
    }

    fun logError(exception: Exception, properties: Map<String, String>, attachments: List<String>?) {
        Crashes.trackError(
            exception,
            properties,
            null)
    }

}