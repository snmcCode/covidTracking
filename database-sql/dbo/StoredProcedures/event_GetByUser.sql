CREATE OR ALTER PROCEDURE event_GetByUser
(@visitorId uniqueidentifier)
AS

Declare @startDate DATETIME2
SET @startDate=(select CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'Eastern Standard Time' AS date))

SELECT e.Id as 'Id' ,o.Name as 'Organization',o.Id as 'OrgId',e.Name as 'Event',CAST(e.DATETIME as DATE) as 'EventDate',dbo.GetEventRegistrationCount(e.Id) as 'BookingCount' 
FROM dbo.visitor_event ve JOIN dbo.event e on ve.eventID=e.Id 
JOIN dbo.organization o ON o.Id=e.OrgId
WHERE ve.VisitorId=@visitorId AND e.[DateTime] >= @startDate
