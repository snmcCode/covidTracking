package ca.snmc.scanner.data.providers

import android.content.Context
import android.content.SharedPreferences
import androidx.preference.PreferenceManager
import ca.snmc.scanner.utils.PRODUCTION_MODE
import ca.snmc.scanner.utils.TESTING_MODE
import ca.snmc.scanner.utils.getCurrentDayFromLong

private const val KEY_IS_INTERNET_AVAILABLE = "is_internet available"
private const val KEY_IS_USER_LOGGED_IN = "is_user_logged_in"
private const val KEY_ARE_DOORS_FETCHED = "are_doors_fetched"
private const val KEY_SCANNER_MODE = "scanner_mode"
private const val KEY_ARE_EVENTS_TODAY_FETCHED = "are_events_today_fetched"
private const val KEY_EVENTS_TODAY_RETRIEVAL_TIME = "events_today_retrieval_time"

class PreferenceProvider(
    context: Context
) {

    private val applicationContext = context.applicationContext

    private val preference: SharedPreferences
        get() = PreferenceManager.getDefaultSharedPreferences(applicationContext)

    // User Login

    fun writeUserIsLoggedIn() {
        preference.edit().putBoolean(
            KEY_IS_USER_LOGGED_IN,
            true
        ).apply()
    }

    fun writeUserIsNotLoggedIn() {
        preference.edit().putBoolean(
            KEY_IS_USER_LOGGED_IN,
            false
        ).apply()
    }

    fun readIsUserLoggedIn() : Boolean {
        return preference.getBoolean(KEY_IS_USER_LOGGED_IN, false)
    }

    // Organization Door Retrieval

    fun writeDoorsAreFetched() {
        preference.edit().putBoolean(
            KEY_ARE_DOORS_FETCHED,
            true
        ).apply()
    }

    fun writeDoorsAreNotFetched() {
        preference.edit().putBoolean(
            KEY_ARE_DOORS_FETCHED,
            false
        ).apply()
    }

    fun readAreDoorsFetched() : Boolean {
        return preference.getBoolean(KEY_ARE_DOORS_FETCHED, false)
    }

    // Event Retrieval

    fun writeEventsTodayAreFetched() {
        preference.edit().putBoolean(
            KEY_ARE_EVENTS_TODAY_FETCHED,
            true
        ).apply()
    }

    fun writeEventsTodayAreNotFetched() {
        preference.edit().putBoolean(
            KEY_ARE_EVENTS_TODAY_FETCHED,
            false
        ).apply()
    }

    fun writeEventsTodayRetrievalTime() {
        preference.edit().putLong(
            KEY_EVENTS_TODAY_RETRIEVAL_TIME,
            System.currentTimeMillis()
        ).apply()
    }

    fun readAreEventsTodayFetched() : Boolean {
        val fetchDay = getCurrentDayFromLong(preference.getLong(KEY_EVENTS_TODAY_RETRIEVAL_TIME, 0))
        val currentDay = getCurrentDayFromLong(System.currentTimeMillis())
//        Log.e("Fetch Events Test", "Fetch Day: $fetchDay, Current Day: $currentDay")
        // If the events were not fetched today
        if (fetchDay != currentDay) {
            writeEventsTodayAreNotFetched()
            return false
        }
        return preference.getBoolean(KEY_ARE_EVENTS_TODAY_FETCHED, false)
    }

    // Internet Access

    fun writeInternetIsAvailable() {
        preference.edit().putBoolean(
            KEY_IS_INTERNET_AVAILABLE,
            true
        ).apply()
    }

    fun writeInternetIsNotAvailable() {
        preference.edit().putBoolean(
            KEY_IS_INTERNET_AVAILABLE,
            false
        ).apply()
    }

    fun readIsInternetAvailable() : Boolean {
        return preference.getBoolean(KEY_IS_INTERNET_AVAILABLE, false)
    }

    // Scanner Mode

    fun writeTestingMode() {
        preference.edit().putInt(
            KEY_SCANNER_MODE,
            TESTING_MODE
        ).apply()
    }

    fun writeProductionMode() {
        preference.edit().putInt(
            KEY_SCANNER_MODE,
            PRODUCTION_MODE
        ).apply()
    }

    fun readScannerMode() : Int {
        return preference.getInt(KEY_SCANNER_MODE, PRODUCTION_MODE)
    }

}