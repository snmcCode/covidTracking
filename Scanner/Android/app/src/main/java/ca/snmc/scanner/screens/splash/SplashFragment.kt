package ca.snmc.scanner.screens.splash

import android.os.Bundle
import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import androidx.fragment.app.Fragment
import androidx.lifecycle.ViewModelProvider
import androidx.navigation.fragment.findNavController
import ca.snmc.scanner.MainActivity
import ca.snmc.scanner.databinding.SplashFragmentBinding
import ca.snmc.scanner.utils.Coroutines
import ca.snmc.scanner.utils.show
import kotlinx.android.synthetic.main.splash_fragment.*
import kotlinx.coroutines.delay
import org.kodein.di.KodeinAware
import org.kodein.di.android.x.kodein
import org.kodein.di.generic.instance

private const val SPLASH_SCREEN_TIMEOUT = 2000.toLong()
class SplashFragment : Fragment(), KodeinAware {

    override val kodein by kodein()

    private lateinit var viewModel: SplashViewModel
    private val splashViewModelFactory : SplashViewModelFactory by instance()

    override fun onCreateView(
        inflater: LayoutInflater, container: ViewGroup?,
        savedInstanceState: Bundle?
    ): View? {

        (activity as MainActivity).fullscreenMode()

        // Binding object that connects to the layout
        val binding: SplashFragmentBinding = SplashFragmentBinding.inflate(inflater, container, false)

        // ViewModel
        viewModel = ViewModelProvider(this, splashViewModelFactory).get(SplashViewModel::class.java)

        // Set LifecycleOwner on Binding object
        binding.lifecycleOwner = this

        // Return the View at the Root of the Binding object
        return binding.root
    }

    override fun onResume() {
        super.onResume()

        (activity as MainActivity).fullscreenMode()
    }

    private fun navigateToLoginPage() {
        val action = SplashFragmentDirections.actionSplashFragmentToLoginFragment()
        this.findNavController().navigate(action)
    }

    private fun navigateToSettingsPage() {
        val action = SplashFragmentDirections.actionSplashFragmentToSettingsFragment()
        this.findNavController().navigate(action)
    }

    override fun onViewCreated(view: View, savedInstanceState: Bundle?) {
        super.onViewCreated(view, savedInstanceState)
        disableUi()

        Coroutines.main {
            delay(SPLASH_SCREEN_TIMEOUT)
            if (viewModel.isOrgLoggedIn()) {
                navigateToSettingsPage()
            } else {
                navigateToLoginPage()
            }
        }

    }

    private fun disableUi() {
        splash_progress_indicator.show()
    }

}