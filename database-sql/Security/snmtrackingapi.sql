CREATE USER [snmtrackingapi]
	FROM EXTERNAL PROVIDER


GO

GRANT CONNECT TO [snmtrackingapi];
GO

GRANT EXECUTE ON OBJECT::dbo.[RegisterUser] TO [snmtrackingapi];
GO
GRANT EXECUTE ON OBJECT::dbo.[GetUser] TO [snmtrackingapi];
GO
