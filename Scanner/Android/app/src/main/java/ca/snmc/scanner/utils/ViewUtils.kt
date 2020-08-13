package ca.snmc.scanner.utils

import android.content.Context
import android.view.View
import android.widget.*
import androidx.constraintlayout.widget.ConstraintLayout
import com.google.android.material.textfield.TextInputLayout

// Used for simplifying function calls on Views

// Toast

fun Context.toast(message: String) {
    Toast.makeText(this, message, Toast.LENGTH_SHORT).show()
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

// ConstraintLayout

fun ConstraintLayout.show() {
    visibility = View.VISIBLE
}

fun ConstraintLayout.hide() {
    visibility = View.GONE
}

// LinearLayout

fun LinearLayout.show() {
    visibility = View.VISIBLE
}

fun LinearLayout.hide() {
    visibility = View.GONE
}

// ImageView

fun ImageView.show() {
    visibility = View.VISIBLE
}

fun ImageView.hide() {
    visibility = View.GONE
}

// TextView

fun TextView.show() {
    visibility = View.VISIBLE
}

fun TextView.hide() {
    visibility = View.GONE
}