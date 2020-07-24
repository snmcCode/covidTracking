package ca.snmc.scanner.screens.splash

import androidx.lifecycle.ViewModelProviders
import android.os.Bundle
import androidx.fragment.app.Fragment
import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import ca.snmc.scanner.MainActivity
import ca.snmc.scanner.R
import ca.snmc.scanner.utils.disable
import ca.snmc.scanner.utils.show
import kotlinx.android.synthetic.main.login_fragment.*
import kotlinx.android.synthetic.main.splash_fragment.*
import org.kodein.di.KodeinAware
import org.kodein.di.android.x.kodein

class SplashFragment : Fragment(), SplashListener, KodeinAware {

    override val kodein by kodein()

    private lateinit var viewModel: SplashViewModel

    override fun onCreateView(
        inflater: LayoutInflater, container: ViewGroup?,
        savedInstanceState: Bundle?
    ): View? {
        // Binding object that connects to the layout
        val binding: SplashFragmentBinding = SplashFragmentBinding.inflate(inflater, container, false)
        return inflater.inflate(R.layout.splash_fragment, container, false)
    }

    override fun onActivityCreated(savedInstanceState: Bundle?) {
        super.onActivityCreated(savedInstanceState)
        (activity as MainActivity).hideNavBar()
    }

    override fun onViewCreated(view: View, savedInstanceState: Bundle?) {
        super.onViewCreated(view, savedInstanceState)
        disableUi()
    }

    private fun disableUi() {
        splash_progress_indicator.show()
    }

}