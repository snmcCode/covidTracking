CREATE OR ALTER PROCEDURE event_register_user
(@eventId int,@visitorId uniqueidentifier)
AS
INSERT INTO dbo.visitor_event(EventId,VisitorId)
VALUES(@eventId,@visitorId)
