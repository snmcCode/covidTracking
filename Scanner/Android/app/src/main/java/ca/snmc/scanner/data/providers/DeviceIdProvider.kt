package ca.snmc.scanner.data.providers

import android.annotation.SuppressLint
import android.content.Context
import android.provider.Settings

class DeviceIdProvider(
    private val context: Context
) {

    @SuppressLint("HardwareIds")
    fun getDeviceId() : String {
        return Settings.Secure.getString(context.contentResolver, Settings.Secure.ANDROID_ID)
    }

}