CREATE USER [snmtrackingapi]
	FROM EXTERNAL PROVIDER


GO

GRANT CONNECT TO [snmtrackingapi];
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