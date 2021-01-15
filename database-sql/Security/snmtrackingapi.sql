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
GO
GRANT EXECUTE ON OBJECT::dbo.[getUserForLogVisit] TO [snmtrackingapi]
GO
GRANT EXECUTE ON OBJECT::dbo.[event_Create] TO [snmtrackingapi]
GO
GRANT EXECUTE ON OBJECT::dbo.[event_delete] TO [snmtrackingapi]
GO
GRANT EXECUTE ON OBJECT::dbo.[event_GetByOrg] TO [snmtrackingapi]
GO
GRANT EXECUTE ON OBJECT::dbo.[event_Update] TO [snmtrackingapi]
GO
GRANT EXECUTE ON OBJECT::dbo.[event_GetByOrgToday] TO [snmtrackingapi]
GO
GRANT EXECUTE ON OBJECT::dbo.[event_GetByUser] TO [snmtrackingapi]
GO
GRANT EXECUTE ON OBJECT::dbo.[event_register_user] TO [snmtrackingapi]
GO
GRANT EXECUTE ON OBJECT::dbo.[event_group] TO [snmtrackingapi]
GO
GRANT EXECUTE ON TYPE::dbo.EventsTableType TO [snmtrackingapi]
GO
GRANT EXECUTE ON OBJECT::dbo.event_unregister_user TO [snmtrackingapi]
GO 
GRANT EXECUTE ON OBJECT::dbo.event_GetBookingByEvent TO [snmtrackingapi]
GO
GRANT EXECUTE ON OBJECT::dbo.event_CheckUserBooking TO [snmtrackingapi]
GO
GRANT EXECUTE ON OBJECT::dbo.status_Get TO [snmtrackingapi]
GO
GRANT EXECUTE ON OBJECT::dbo.status_CheckUser TO [snmtrackingapi]
GO
GRANT EXECUTE ON OBJECT::dbo.status_Create TO [snmtrackingapi]
GO
GRANT EXECUTE ON OBJECT::dbo.status_setUser TO [snmtrackingapi]






