CREATE OR ALTER PROCEDURE event_CheckUserBooking
(@visitorId uniqueIdentifier ,@eventId int,@isBooked bit OUT)
AS
IF EXISTS(SELECT id FROM visitor_event WHERE VisitorId=@visitorId AND EventId=@eventId)
    SET @isBooked=1
ELSE
    SET @isBooked=0