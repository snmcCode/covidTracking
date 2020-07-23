package com.snmc.scanner

import android.app.Application
import com.snmc.scanner.data.db.AppDatabase
import com.snmc.scanner.data.network.AuthenticateApi
import com.snmc.scanner.data.network.BackEndApi
import com.snmc.scanner.data.network.LoginApi
import com.snmc.scanner.data.network.NetworkConnectionInterceptor
import com.snmc.scanner.data.repositories.AuthenticateRepository
import com.snmc.scanner.data.repositories.LoginRepository
import com.snmc.scanner.screens.login.LoginViewModelFactory
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

        // Repositories
        bind() from singleton { LoginRepository(instance(), instance()) }
        bind() from singleton { AuthenticateRepository(instance(), instance()) }

        // View Model Factories
        bind() from provider { LoginViewModelFactory(instance(), instance(), instance()) }
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
        return "${getString(R.string.backend_base_url)}/"
    }

    // Needed for Init
    private fun getTenantId() : String {
        return getString(R.string.tenant_id)
    }

}