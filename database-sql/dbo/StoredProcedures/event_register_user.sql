CREATE OR ALTER PROCEDURE event_register_user
(@eventId int,@visitorId uniqueidentifier)
AS
SET TRANSACTION ISOLATION LEVEL SERIALIZABLE
BEGIN TRANSACTION
    DECLARE @bookingCount INT
    SELECT @bookingCount= dbo.GetEventRegistrationCount(@eventId)

    DECLARE @capacity INT,@groupId UNIQUEIDENTIFIER
    SELECT @capacity=capacity from [event] where Id=@eventId
    SELECT @groupId=groupId from [event] where Id=@eventId

    IF @bookingCount < @capacity
        BEGIN TRY
                INSERT INTO dbo.visitor_event(EventId,VisitorId,EventGroupId)
                VALUES(@eventId,@visitorId,@groupId)
        END TRY
        BEGIN CATCH
            IF ERROR_NUMBER() = 2601
            BEGIN 
                DECLARE @count as INT 
                Select @count=COUNT(*) FROM dbo.visitor_event where EventId=@eventId AND VisitorId=@visitorId
                IF @count = 0
                    IF @@TRANCOUNT>0
                        ROLLBACK TRAN;
                    THROW 51983, 'BOOKED_SAME_GROUP',1

            END
        END CATCH
    ELSE
        THROW 51982, 'EVENT_FULL',1
IF @@TRANCOUNT>0
    COMMIT TRANSACTION
    