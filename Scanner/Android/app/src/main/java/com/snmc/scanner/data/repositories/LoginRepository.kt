package com.snmc.scanner.data.repositories

import com.snmc.scanner.data.db.AppDatabase
import com.snmc.scanner.data.db.entities.Organization
import com.snmc.scanner.data.network.LoginApi
import com.snmc.scanner.data.network.SafeApiRequest
import com.snmc.scanner.data.network.responses.LoginResponse
import com.snmc.scanner.models.LoginInfo

// Used to abstract API calls away from ViewModel, returns Response object to ViewModel
class LoginRepository(
    private val api: LoginApi,
    private val db: AppDatabase
) : SafeApiRequest() {

    suspend fun scannerLogin(loginInfo: LoginInfo) : LoginResponse {
        return apiRequest { api.scannerLogin(loginInfo) }
    }

    suspend fun saveOrganization(organization: Organization) = db.getOrganizationDao().upsert(organization)

    fun getSavedOrganization() = db.getOrganizationDao().getOrganization()

}