package ca.snmc.scanner.utils

import androidx.lifecycle.LiveData
import androidx.lifecycle.MediatorLiveData
import ca.snmc.scanner.data.db.entities.AuthenticationEntity
import ca.snmc.scanner.data.db.entities.OrganizationEntity

class CombinedOrgAuthData(
    ldOrganization: LiveData<OrganizationEntity>,
    ldAuthentication: LiveData<AuthenticationEntity>
) : MediatorLiveData<Pair<OrganizationEntity, AuthenticationEntity>>() {

    private var organization: OrganizationEntity = OrganizationEntity()
    private var authentication: AuthenticationEntity = AuthenticationEntity()

    init {
        value = Pair(organization, authentication)

        addSource(ldOrganization) {
            if (it != null) organization = it
            value = Pair(organization, authentication)
        }

        addSource(ldAuthentication) {
            if (it != null) authentication = it
            value = Pair(organization, authentication)
        }
    }

}