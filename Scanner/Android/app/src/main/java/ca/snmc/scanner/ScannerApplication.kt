package ca.snmc.scanner

import android.app.Application
import ca.snmc.scanner.data.db.AppDatabase
import ca.snmc.scanner.data.network.AuthenticateApi
import ca.snmc.scanner.data.network.BackEndApi
import ca.snmc.scanner.data.network.LoginApi
import ca.snmc.scanner.data.network.NetworkConnectionInterceptor
import ca.snmc.scanner.data.preferences.PreferenceProvider
import ca.snmc.scanner.data.repositories.AuthenticateRepository
import ca.snmc.scanner.data.repositories.BackEndRepository
import ca.snmc.scanner.data.repositories.LoginRepository
import ca.snmc.scanner.screens.login.LoginViewModelFactory
import ca.snmc.scanner.screens.settings.SettingsViewModelFactory
import ca.snmc.scanner.screens.splash.SplashViewModelFactory
import org.kodein.di.Kodein
import org.kodein.di.KodeinAware
import org.kodein.di.android.x.androidXModule
import org.kodein.di.generic.bind
import org.kodein.di.generic.instance
import org.kodein.di.generic.provider
import org.kodein.di.generic.singleton

class ScannerApplication : Application(), KodeinAware {

    // Creates Instances of All Classes to Allow for Dependency Management
    override val kodein = Kodein.lazy {
        import(androidXModule(this@ScannerApplication))

        // Interceptors
        bind() from singleton { NetworkConnectionInterceptor(instance()) }

        // Apis
        bind() from singleton { LoginApi(
            baseUrl = getLoginBaseUrl(),
            networkConnectionInterceptor = instance()
        ) }
        bind() from singleton { AuthenticateApi(
            baseUrl = getAuthenticateBaseUrl(),
            networkConnectionInterceptor = instance()
        ) }
        bind() from singleton { BackEndApi(
            baseUrl = getBackEndBaseUrl(),
            networkConnectionInterceptor = instance()
        ) }

        // Room Database
        bind() from singleton { AppDatabase(instance()) }

        // Preferences
        bind() from singleton { PreferenceProvider(instance()) }

        // Repositories
        bind() from singleton { LoginRepository(instance(), instance()) }
        bind() from singleton { AuthenticateRepository(instance(), instance()) }
        bind() from singleton { BackEndRepository(instance(), instance()) }

        // View Model Factories
        bind() from provider { SplashViewModelFactory(instance(), instance())}
        bind() from provider { LoginViewModelFactory(instance(), instance(), instance(), instance()) }
        bind() from provider { SettingsViewModelFactory(instance(), instance(), instance()) }
    }

    // Needed for Init
    private fun getLoginBaseUrl(): String {
        return "${getString(R.string.login_base_url)}/"
    }

    // Needed for Init
    private fun getAuthenticateBaseUrl(): String {
        val tenantId : String = getTenantId()

        return "${getString(R.string.authentication_base_url)}/$tenantId/"
    }

    private fun getBackEndBaseUrl(): String {
        return "${getString(R.string.backend_base_url_api)}/"
    }

    // Needed for Init
    private fun getTenantId() : String {
        return getString(R.string.tenant_id)
    }

}