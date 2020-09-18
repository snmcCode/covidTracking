CREATE OR ALTER PROCEDURE event_GetByOrgToday
(@orgId INT)
AS
Declare @today as Date= (select CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'Eastern Standard Time' AS date))
SELECT id,Name,Hall from [event] where orgId=@orgId and [DateTime]=@today;