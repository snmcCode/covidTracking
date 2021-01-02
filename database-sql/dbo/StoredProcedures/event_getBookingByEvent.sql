CREATE OR ALTER PROCEDURE event_GetBookingByEvent
(@id int)
AS
SELECT v.FirstName,dbo.udfLastPortionOfId(v.Id) as 'VisitorIdShort',ve.[DateTime] as RegistrationTime
FROM [event] e JOIN visitor_event ve ON e.Id=ve.eventId  JOIN  visitor v on v.Id=ve.visitorId
WHERE e.Id=@id