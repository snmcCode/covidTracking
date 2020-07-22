package com.snmc.scanner.utils

import android.content.Context
import android.opengl.Visibility
import android.view.View
import android.widget.Button
import android.widget.ProgressBar
import android.widget.TextView
import android.widget.Toast
import com.google.android.material.textfield.TextInputLayout

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

// TextInputLayout

fun TextInputLayout.showError(errorMessage: String) {
    error = errorMessage
    isErrorEnabled = true
}

fun TextInputLayout.hideError() {
    error = null
    isErrorEnabled = false
}

// TextView

fun TextView.showError(errorMessage: String) {
    text = errorMessage
    visibility = View.VISIBLE
}

fun TextView.hideError() {
    text = null
    visibility = View.GONE
}