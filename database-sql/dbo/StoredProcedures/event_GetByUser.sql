CREATE OR ALTER PROCEDURE event_GetByUser
(@visitorId uniqueidentifier)
AS
SELECT e.Id as 'EventId' ,o.Name as 'Organization',e.Name as 'Event',CAST(e.DATETIME as DATE) as 'EventDate',dbo.GetEventRegistrationCount(e.Id) as 'BookingCount' 
FROM dbo.visitor_event ve JOIN dbo.event e on ve.eventID=e.Id 
JOIN dbo.organization o ON o.Id=e.OrgId
WHERE ve.VisitorId=@visitorId
