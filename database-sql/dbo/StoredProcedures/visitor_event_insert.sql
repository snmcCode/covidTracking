CREATE OR ALTER PROCEDURE visitor_event_insert
(@visitorId UNIQUEIDENTIFIER,@eventId INT)
AS
INSERT dbo.visitor_event(EventId,VisitorId)
VALUES (@eventId,@visitorId)