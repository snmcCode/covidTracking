package ca.snmc.scanner.utils


object AuthApiUtils {
    fun getGrantType() : String {
        return "client_credentials"
    }

    fun getScope(scopePrefix: String) : String {
        val scopeSuffix : String = getScopeSuffix()

        return "$scopePrefix/$scopeSuffix"
    }

    private fun getScopeSuffix() : String {
        return ".default"
    }
}

object GetDoorsApiUtils {
    fun generateUrl(id: Int) : String {
        return "organization/${id}/doors"
    }
}

object BackEndApiUtils {
    fun generateAuthorization(accessToken: String) : String {
        return "Bearer $accessToken"
    }
}