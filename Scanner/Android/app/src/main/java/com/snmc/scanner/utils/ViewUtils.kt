package com.snmc.scanner.utils

import android.content.Context
import android.view.View
import android.widget.Button
import android.widget.ProgressBar
import android.widget.Toast

// Used for simplifying function calls on Views

// Toast

fun Context.toast(message: String) {
    Toast.makeText(this, message, Toast.LENGTH_LONG).show()
}

// Progress Bar

fun ProgressBar.show() {
    visibility = View.VISIBLE
}

fun ProgressBar.hide() {
    visibility = View.GONE
}

// Button

fun Button.enable() {
    isEnabled = true
}

fun Button.disable() {
    isEnabled = false
}