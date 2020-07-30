package ca.snmc.scanner.data.preferences

import android.content.Context
import android.content.SharedPreferences
import androidx.preference.PreferenceManager
import java.util.*

private const val KEY_IS_INTERNET_AVAILABLE = "is_internet available"
private const val KEY_IS_USER_LOGGED_IN = "is_user_logged_in"
private const val KEY_IS_USER_AUTHENTICATED = "is_user_authenticated"
private const val KEY_AUTH_TOKEN_EXPIRY_TIME = "auth_token_expiry_time"
private const val KEY_ARE_DOORS_FETCHED = "are_doors_fetched"

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

}