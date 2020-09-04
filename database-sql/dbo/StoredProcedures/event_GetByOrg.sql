CREATE OR ALTER PROCEDURE event_GetByOrg
(@orgId INT)
AS
SELECT * from event where orgId=@orgId;