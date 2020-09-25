CREATE OR ALTER PROCEDURE event_GetByOrg
(@orgId INT)
AS
SELECT e.*,dbo.GetEventRegistrationCount(e.Id) as 'BookingCount' from event e where orgId=@orgId