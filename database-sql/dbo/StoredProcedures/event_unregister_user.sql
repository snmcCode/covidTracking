CREATE OR ALTER PROCEDURE event_unregister_user
(@eventId int,@visitorId uniqueidentifier)
AS

    Delete dbo.visitor_event
    Where EventId=@eventId and VisitorId=@visitorId
