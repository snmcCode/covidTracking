CREATE OR ALTER FUNCTION GetEventRegistrationCount(@eventId INT)
RETURNS INT AS
BEGIN
DECLARE @countregisters INT
SELECT @countregisters = COUNT(Id) FROM dbo.visitor_event 
Where EventId=@eventId
RETURN @countregisters
END