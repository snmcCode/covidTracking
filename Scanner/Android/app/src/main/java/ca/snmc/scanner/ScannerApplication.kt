package ca.snmc.scanner

import android.app.Application
import android.content.Context
import ca.snmc.scanner.data.db.AppDatabase
import ca.snmc.scanner.data.network.NetworkConnectionInterceptor
import ca.snmc.scanner.data.network.apis.production.AuthenticateProductionApi
import ca.snmc.scanner.data.network.apis.production.BackEndProductionApi
import ca.snmc.scanner.data.network.apis.production.LoginProductionApi
import ca.snmc.scanner.data.network.apis.testing.AuthenticateTestingApi
import ca.snmc.scanner.data.network.apis.testing.BackEndTestingApi
import ca.snmc.scanner.data.network.apis.testing.LoginTestingApi
import ca.snmc.scanner.data.providers.DeviceIdProvider
import ca.snmc.scanner.data.providers.LocationProvider
import ca.snmc.scanner.data.providers.PreferenceProvider
import ca.snmc.scanner.data.providers.VisitLogFileProvider
import ca.snmc.scanner.data.repositories.*
import ca.snmc.scanner.screens.login.LoginViewModelFactory
import ca.snmc.scanner.screens.scanner.ScannerViewModelFactory
import ca.snmc.scanner.screens.settings.SettingsViewModelFactory
import ca.snmc.scanner.screens.splash.SplashViewModelFactory
import com.google.android.gms.location.LocationServices
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
        // Production
        bind() from singleton {
            LoginProductionApi(
                baseUrl = getLoginBaseUrlProduction(),
                networkConnectionInterceptor = instance()
            )
        }
        bind() from singleton {
            AuthenticateProductionApi(
                baseUrl = getAuthenticateBaseUrlProduction(),
                networkConnectionInterceptor = instance()
            )
        }
        bind() from singleton {
            BackEndProductionApi(
                baseUrl = getBackEndBaseUrlProduction(),
                networkConnectionInterceptor = instance()
            )
        }
        // Testing
        bind() from singleton {
            LoginTestingApi(
                baseUrl = getLoginBaseUrlTesting(),
                networkConnectionInterceptor = instance()
            )
        }
        bind() from singleton {
            AuthenticateTestingApi(
                baseUrl = getAuthenticateBaseUrlTesting(),
                networkConnectionInterceptor = instance()
            )
        }
        bind() from singleton {
            BackEndTestingApi(
                baseUrl = getBackEndBaseUrlTesting(),
                networkConnectionInterceptor = instance()
            )
        }

        // Room Database
        bind() from singleton { AppDatabase(instance()) }

        // Providers
        bind() from provider { LocationServices.getFusedLocationProviderClient(instance<Context>()) }
        bind() from singleton { PreferenceProvider(instance()) }
        bind() from singleton { LocationProvider(instance(), instance()) }
        bind() from singleton { DeviceIdProvider(instance()) }
        bind() from singleton { VisitLogFileProvider(instance(), getVisitLogsFileName()) }

        // Repositories
        bind() from singleton { LoginRepository(instance(), instance(), instance()) }
        bind() from singleton { AuthenticateRepository(instance(), instance(), instance()) }
        bind() from singleton { BackEndRepository(instance(), instance(), instance()) }
        bind() from singleton { DeviceInformationRepository(instance(), instance(), instance()) }
        bind() from singleton { DeviceIORepository(instance()) }

        // View Model Factories
        bind() from provider { MainViewModelFactory(instance(), instance(), instance(), instance()) }
        bind() from provider { SplashViewModelFactory(instance(), instance()) }
        bind() from provider { LoginViewModelFactory(instance(), instance(), instance(), instance(), instance()) }
        bind() from provider { SettingsViewModelFactory(instance(), instance(), instance(), instance(), instance(), instance()) }
        bind() from provider { ScannerViewModelFactory(instance(), instance(), instance(), instance(), instance(), instance(), instance()) }
    }

    // Production

    private fun getLoginBaseUrlProduction(): String {
        return "${getString(R.string.login_base_url_production)}/"
    }

    private fun getAuthenticateBaseUrlProduction(): String {
        val tenantId : String = getTenantId()

        return "${getString(R.string.authentication_base_url_production)}/$tenantId/"
    }

    private fun getBackEndBaseUrlProduction(): String {
        return "${getString(R.string.backend_base_url_api_production)}/"
    }

    // Testing

    private fun getLoginBaseUrlTesting(): String {
        return "${getString(R.string.login_base_url_testing)}/"
    }

    private fun getAuthenticateBaseUrlTesting(): String {
        val tenantId : String = getTenantId()

        return "${getString(R.string.authentication_base_url_testing)}/$tenantId/"
    }

    private fun getBackEndBaseUrlTesting(): String {
        return "${getString(R.string.backend_base_url_api_testing)}/"
    }

    // Needed for Init
    private fun getTenantId() : String {
        return getString(R.string.tenant_id)
    }

    private fun getVisitLogsFileName() : String {
        return getString(R.string.visit_logs_file_name)
    }

}