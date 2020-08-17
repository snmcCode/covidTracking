CREATE USER [snmtrackingapi]
	FROM EXTERNAL PROVIDER

GO

CREATE USER [snmtrackingapi-anonymous]
	FROM EXTERNAL PROVIDER
GO
GRANT CONNECT TO [snmtrackingapi];
GO
GRANT CONNECT TO [snmtrackingapi-anonymous]
GO

GRANT EXECUTE ON OBJECT::dbo.[RegisterUser] TO [snmtrackingapi];
GO
GRANT EXECUTE ON OBJECT::dbo.[GetUser] TO [snmtrackingapi];
GO
GRANT EXECUTE ON OBJECT::dbo.[UpdateUser] TO [snmtrackingapi];
GO
GRANT EXECUTE ON OBJECT::dbo.[getOrganization] TO [snmtrackingapi];
GO
GRANT EXECUTE ON OBJECT::dbo.[InsertOrganization] TO [snmtrackingapi];
GO
GRANT EXECUTE ON OBJECT::dbo.[UpdateOrganization] TO [snmtrackingapi];
GO
GRANT EXECUTE ON OBJECT::dbo.[DeleteOrganization] TO [snmtrackingapi];
GO
GRANT EXECUTE ON OBJECT::dbo.[orgAddCredentials] TO [snmtrackingapi];
GO
GRANT EXECUTE ON OBJECT::dbo.[orgGetDoors] TO [snmtrackingapi]
GO
GRANT EXECUTE ON OBJECT::dbo.[orgCheckCredentials] TO [snmtrackingapi-anonymous]
GO
GRANT EXECUTE ON OBJECT::dbo.[settings_Get] TO [snmtrackingapi]
