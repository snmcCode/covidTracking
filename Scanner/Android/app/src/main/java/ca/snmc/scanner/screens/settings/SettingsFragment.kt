 package ca.snmc.scanner.screens.settings

import android.os.Bundle
import android.util.Log
import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.widget.ArrayAdapter
import androidx.fragment.app.Fragment
import androidx.lifecycle.Observer
import androidx.lifecycle.ViewModelProvider
import ca.snmc.scanner.R
import ca.snmc.scanner.data.db.entities.OrganizationDoorEntity
import ca.snmc.scanner.databinding.SettingsFragmentBinding
import ca.snmc.scanner.utils.Coroutines
import ca.snmc.scanner.utils.toast
import kotlinx.android.synthetic.main.settings_fragment.*
import org.kodein.di.KodeinAware
import org.kodein.di.android.x.kodein
import org.kodein.di.generic.instance

 class SettingsFragment : Fragment(), SettingsListener, KodeinAware {

     override val kodein by kodein()
     private val settingsViewModelFactory : SettingsViewModelFactory by instance()

     override fun onCreateView(
         inflater: LayoutInflater, container: ViewGroup?,
         savedInstanceState: Bundle?
     ): View? {

         val binding : SettingsFragmentBinding = SettingsFragmentBinding.inflate(inflater, container, false)

         // ViewModel
         val viewModel = ViewModelProvider(this, settingsViewModelFactory).get(SettingsViewModel::class.java)

         // Wait for Doors to load
         Coroutines.main {
             val organizationDoors = viewModel.organizationDoors.await()
             organizationDoors.observe(viewLifecycleOwner, Observer {
                 requireActivity().toast(it.size.toString())
                 setSpinnerData(it)
             })
         }

         // Set ViewModel on Binding object
         binding.viewmodel = viewModel

         // Set LifecycleOwner on Binding object
         binding.lifecycleOwner = this

         // Fragment implements methods defined in LoginListener which are called by ViewModel
         viewModel.settingsListener = this

         // Return the View at the Root of the Binding object
         return binding.root
    }

     private fun setSpinnerData(organizationDoors: List<OrganizationDoorEntity>) {
         val doorNames: List<String> = organizationDoors.map { it.doorName }
         val arrayAdapter : ArrayAdapter<String> = ArrayAdapter(requireContext(), R.layout.support_simple_spinner_dropdown_item)
         arrayAdapter.setDropDownViewResource(R.layout.support_simple_spinner_dropdown_item)
         organizationSpinner.adapter = arrayAdapter
     }

}