CREATE OR ALTER PROCEDURE event_register_user
(@eventId int,@visitorId uniqueidentifier)
AS

DECLARE @bookingCount INT
SELECT @bookingCount= dbo.GetEventRegistrationCount(@eventId)

DECLARE @capacity INT
SELECT @capacity=capacity from event where Id=@eventId

IF @bookingCount < @capacity
    INSERT INTO dbo.visitor_event(EventId,VisitorId)
    VALUES(@eventId,@visitorId)
ELSE
    THROW 51982, 'EVENT_FULL',1
