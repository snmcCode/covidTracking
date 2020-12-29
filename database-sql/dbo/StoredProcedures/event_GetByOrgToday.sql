CREATE OR ALTER PROCEDURE event_GetByOrgToday
(@orgId INT)
AS
Declare @startDate DATETIME2
Declare @endDate DATETIME

SET @startDate=(select CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'Eastern Standard Time' AS date))
SET @endDate=DateAdd(Second,-1,DateAdd(Day,1,@startDate))

Declare @today as Date= (select CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'Eastern Standard Time' AS date))
SELECT id,Name,Hall,[DateTime],Capacity from [event] where orgId=@orgId AND [DateTime] >= @startDate AND [DateTime] <= @endDate;