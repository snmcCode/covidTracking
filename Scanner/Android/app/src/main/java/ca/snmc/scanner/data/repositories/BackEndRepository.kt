package ca.snmc.scanner.data.repositories

import android.util.Log
import androidx.lifecycle.LiveData
import androidx.lifecycle.MutableLiveData
import ca.snmc.scanner.data.db.AppDatabase
import ca.snmc.scanner.data.db.entities.AuthenticationEntity
import ca.snmc.scanner.data.network.BackEndApi
import ca.snmc.scanner.data.network.SafeApiRequest
import ca.snmc.scanner.data.db.entities.OrganizationDoorEntity
import ca.snmc.scanner.data.db.entities.OrganizationEntity
import ca.snmc.scanner.data.preferences.PreferenceProvider
import ca.snmc.scanner.models.OrganizationDoorInfo
import ca.snmc.scanner.utils.*
import ca.snmc.scanner.utils.BackEndApiUtils.generateAuthorization
import ca.snmc.scanner.utils.GetDoorsApiUtils.generateUrl
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext
import java.time.LocalDateTime
import java.time.temporal.ChronoUnit
import java.util.*

private val MINIMUM_INTERVAL = 6

// Used to abstract API calls away from ViewModel, returns Response object to ViewModel
class BackEndRepository(
    private val api: BackEndApi,
    private val db: AppDatabase,
    private val prefs: PreferenceProvider
) : SafeApiRequest() {

    private var organizationDoorInfo : OrganizationDoorInfo? = null
    private val organizationDoors = MutableLiveData<List<OrganizationDoorEntity>>()

    private var url : String? = null
    private var authorization : String? = null

    init {
        // Set Url and Authorization by constantly observing the database values of organization and authentication
        getSavedOrganization().observeForever {
            if (it?.id != null) {
                url = generateUrl(it.id!!)
            }
        }

        getSavedAuthorization().observeForever {
            if (it?.accessToken != null) {
                authorization = generateAuthorization(it.accessToken!!)
            }
        }

        organizationDoors.observeForever {
            saveOrganizationDoors(it)
        }
    }

    private suspend fun fetchOrganizationDoors() {
        if (isFetchNeeded() && isDataAvailable()) {
            val response = apiRequest {
                api.getOrganizationDoors(
                    url = url!!,
                    authorization = authorization!!
                )}
            organizationDoors.postValue(mapOrganizationDoorResponseToOrganizationDoorEntityList(response))
        }
    }

    suspend fun getOrganizationDoors() : LiveData<List<OrganizationDoorEntity>> {
        return withContext(Dispatchers.IO) {
            fetchOrganizationDoors()
            db.getOrganizationDoorDao().getOrganizationDoors()
        }
    }

    fun getSavedOrganization() = db.getOrganizationDao().getOrganization()

    fun getSavedAuthorization() = db.getAuthenticationDao().getAuthentication()

    private fun setUrl(organizationEntity: OrganizationEntity) {
        url = generateUrl(organizationEntity.id!!)
    }

    private fun setAuthorization(authenticationEntity: AuthenticationEntity) {
        authorization = authenticationEntity.accessToken
    }

    private fun saveOrganizationDoors(organizationDoorEntities: List<OrganizationDoorEntity>) {
        Coroutines.io {
            prefs.writeDoorsAreFetched()
            db.getOrganizationDoorDao().saveOrganizationDoors(organizationDoorEntities)
        }
    }

    private fun isFetchNeeded() : Boolean {
        return !prefs.readAreDoorsFetched()
    }

    private fun isDataAvailable() : Boolean {
        return url != null && authorization != null
    }

}