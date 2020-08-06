package ca.snmc.scanner.data.network.responses

import ca.snmc.scanner.models.OrganizationDoor

// TODO: This is just an ArrayList currently. We may change the architecture of the BackEnd in the
// TODO: future such that it always returns a JSON object rather than a string or an array, so this
// TODO: will be used for mapping in such a case
class OrganizationDoorsResponse : ArrayList<OrganizationDoor>()