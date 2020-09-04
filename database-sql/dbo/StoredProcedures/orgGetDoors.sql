CREATE OR ALTER PROCEDURE [dbo].[orgGetDoors]
		@orgId int
AS
	Select * from dbo.organizationDoor where orgID=@orgId
RETURN 0
