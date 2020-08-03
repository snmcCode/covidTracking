package ca.snmc.scanner.utils

import android.widget.Spinner
import androidx.databinding.BindingAdapter
import ca.snmc.scanner.utils.extensions.SpinnerExtensions
import ca.snmc.scanner.utils.extensions.SpinnerExtensions.setSpinnerEntries
import ca.snmc.scanner.utils.extensions.SpinnerExtensions.setSpinnerValue

class SpinnerBindings {

    @BindingAdapter("entries")
    fun Spinner.setEntries(entries: List<Any>?) {
        setSpinnerEntries(entries)
    }

    @BindingAdapter("onItemSelected")
    fun Spinner.setItemSelectedListener(itemSelectedListener: SpinnerExtensions.ItemSelectedListener?) {
        setItemSelectedListener(itemSelectedListener)
    }

    @BindingAdapter("newValue")
    fun Spinner.setNewValue(newValue: Any?) {
        setSpinnerValue(newValue)
    }

}