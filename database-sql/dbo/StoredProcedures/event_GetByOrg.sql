CREATE OR ALTER PROCEDURE event_GetByOrg
(@orgId INT,@startDate DATETIME2 = NULL, @endDate DATETIME2 = '2060-12-31')
AS
IF @startDate is NULL
    SET @startDate=(select CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'Eastern Standard Time' AS date))
IF @endDate IS NOT NULL
    SET @endDate=DateAdd(Second,-1,DateAdd(Day,1,@endDate))
SELECT e.*,dbo.GetEventRegistrationCount(e.Id) as 'BookingCount' FROM event e 
where orgId=@orgId AND [DateTime] >= @startDate AND [DateTime] <= @endDate