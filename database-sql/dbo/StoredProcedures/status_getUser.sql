CREATE OR ALTER PROCEDURE status_getUser
(@orgId int,@visitorId UniqueIdentifier)
AS
DECLARE @currentStatus INT 
SELECT @currentStatus=StatusValue FROM dbo.visitor_status WHERE orgId=@orgId AND visitorId=@visitorId 
IF @currentStatus IS NULL
    SET @currentStatus=0


Select @currentStatus as statusValue;