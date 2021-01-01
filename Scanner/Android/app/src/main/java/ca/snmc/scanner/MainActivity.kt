package ca.snmc.scanner

import android.os.Build
import android.os.Bundle
import android.util.Log
import android.view.View
import android.view.WindowInsets
import android.view.WindowInsetsController
import androidx.appcompat.app.AppCompatActivity
import androidx.lifecycle.Observer
import androidx.lifecycle.ViewModelProvider
import androidx.lifecycle.lifecycleScope
import ca.snmc.scanner.databinding.ActivityMainBinding
import ca.snmc.scanner.models.EventData
import ca.snmc.scanner.utils.TESTING_MODE
import com.microsoft.appcenter.AppCenter
import com.microsoft.appcenter.analytics.Analytics
import com.microsoft.appcenter.crashes.Crashes
import com.microsoft.appcenter.distribute.Distribute
import kotlinx.android.synthetic.main.activity_main.*
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.launch
import kotlinx.coroutines.withContext
import org.kodein.di.KodeinAware
import org.kodein.di.android.kodein
import org.kodein.di.generic.instance

class MainActivity : AppCompatActivity(), KodeinAware {

    override val kodein by kodein()

    private lateinit var viewModel: MainViewModel
    private val MainViewModelFactory: MainViewModelFactory by instance()

    private val permissionsRequestCode = 1001

    private var isFullScreenMode = true

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

            // Monitor the selected event
            viewModel.getSelectedEvent().observe(this@MainActivity, Observer { selectedEventEntity ->
                lifecycleScope.launch {
                    withContext(Dispatchers.IO) {
                        viewModel.updateEventData(selectedEventEntity)
                    }

                    if (selectedEventEntity?.eventId != null) {
                        viewModel.getSelectedEventLiveAttendance(selectedEventEntity.eventId!!).observe(this@MainActivity, Observer {
                            lifecycleScope.launch {
                                withContext(Dispatchers.IO) {
                                    viewModel.updateEventAttendance()
                                }
                            }
                        })
                    }

                }
            })


            viewModel.eventData.observe(this@MainActivity, Observer { eventData ->
                updateSelectedEventIndicator(eventData)
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

    private fun updateSavedScanLogsIndicator(logCount: Int) {
        if (logCount > 0) {
            saved_scan_logs_indicator.text = getString(
                R.string.saved_scan_logs_notification,
                logCount
            )
            saved_scan_logs_indicator.visibility = View.VISIBLE
        } else {
            saved_scan_logs_indicator.text = getString(R.string.saved_scan_logs_notification, 0)
            saved_scan_logs_indicator.visibility = View.GONE
        }
    }

    private fun updateSelectedEventIndicator(eventData: EventData?) {
        lifecycleScope.launch {

            if (eventData?.eventId != null && eventData.eventName != null) {
                if (eventData.eventAttendance != null && eventData.eventCapacity != null) {
                    if (eventData.eventAttendance!! >= eventData.eventCapacity!!) {
                        selected_event_indicator.setBackgroundResource(R.color.warningIndicator)
                    } else {
                        selected_event_indicator.setBackgroundResource(R.color.successIndicator)
                    }
                    selected_event_indicator.text = getString(
                        R.string.selected_event_notification_with_attendance,
                        eventData.eventName,
                        eventData.eventAttendance,
                        eventData.eventCapacity
                    )
                } else {
                    selected_event_indicator.text = getString(
                        R.string.selected_event_notification,
                        eventData.eventName
                    )
                }
                selected_event_indicator.visibility = View.VISIBLE
            } else {
                selected_event_indicator.text = getString(
                    R.string.selected_event_notification,
                    "None")
                selected_event_indicator.visibility = View.GONE
            }
        }
    }

    fun fullscreenMode() {
        isFullScreenMode = true

        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.R) {
            window.setDecorFitsSystemWindows(false)
            window.insetsController?.hide(WindowInsets.Type.statusBars())
            window.insetsController?.hide(WindowInsets.Type.navigationBars())
            window.insetsController?.hide(WindowInsets.Type.systemBars())
            window.insetsController?.systemBarsBehavior = WindowInsetsController.BEHAVIOR_SHOW_TRANSIENT_BARS_BY_SWIPE
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
        isFullScreenMode = false

        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.R) {
            window.setDecorFitsSystemWindows(true)
            window.insetsController?.show(WindowInsets.Type.statusBars())
            window.insetsController?.show(WindowInsets.Type.navigationBars())
            window.insetsController?.show(WindowInsets.Type.systemBars())
        } else {
            window.decorView.systemUiVisibility = (View.SYSTEM_UI_FLAG_FULLSCREEN)
        }
    }

    override fun onWindowFocusChanged(hasFocus: Boolean) {
        super.onWindowFocusChanged(hasFocus)

        if (isFullScreenMode) {
            fullscreenMode()
        } else {
            windowedMode()
        }

    }

    fun logError(exception: Exception, properties: Map<String, String>, attachments: List<String>?) {
        Crashes.trackError(
            exception,
            properties,
            null
        )
    }

}