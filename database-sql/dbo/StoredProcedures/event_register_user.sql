CREATE OR ALTER PROCEDURE event_register_user
(@eventId int,@visitorId uniqueidentifier)
AS
BEGIN TRY
BEGIN TRANSACTION
    DECLARE @getLock INT
    exec @getLock=sp_getapplock @Resource='MyAppLock', @LockMode='Exclusive',@LockTimeout=1000 
    IF @getLock < 0 --lock timed out
    BEGIN
        IF @@TRANCOUNT>0
            ROLLBACK TRAN;
        Return;
    Return
    END

    DECLARE @bookingCount INT
    SELECT @bookingCount= dbo.GetEventRegistrationCount(@eventId) --get current booking

    DECLARE @capacity INT,@groupId UNIQUEIDENTIFIER,@targetAudience SMALLINT,@orgID INT
    SELECT @capacity=capacity,@targetAudience=TargetAudience,@orgID=OrgId from [event] where Id=@eventId
    SELECT @groupId=groupId from [event] where Id=@eventId

    IF @bookingCount < @capacity
	BEGIN
    IF @targetAudience IS NOT NULL AND @targetAudience !=0 --raise the error and exit
    BEGIN
        DECLARE @checkStatus BIT
        exec status_checkUser @orgId,@visitorId,@targetAudience,@checkStatus OUT
        IF @checkStatus=0
            THROW 51984, 'WRONG_AUDIENCE',1
    END

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
                BEGIN
                    IF @@TRANCOUNT>0
                        ROLLBACK TRAN;
                    THROW 51983, 'BOOKED_SAME_GROUP',1;
                END
            ELSE
                BEGIN
                IF @@TRANCOUNT>0
                    ROLLBACK TRAN;
                THROW;
                END
        END
    END CATCH
	END
    ELSE
        BEGIN
            IF @@TRANCOUNT>0
                ROLLBACK TRAN;
            THROW 51982, 'EVENT_FULL',1;
        END

    IF @@TRANCOUNT>0
        BEGIN
            exec sp_releaseapplock @Resource='MyAppLock'
            COMMIT TRANSACTION
        END
END TRY
BEGIN CATCH
    IF @@TRANCOUNT>0
        ROLLBACK TRAN;
    THROW;
END CATCH




