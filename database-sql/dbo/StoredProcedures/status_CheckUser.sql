CREATE OR ALTER PROCEDURE status_CheckUser
(@orgId int,@visitorId UniqueIdentifier,@statusValue INT,@checkResult bit OUT)
AS

DECLARE @currentStatus INT
SELECT @currentStatus=statusValue from dbo.visitor_status where VisitorId=@visitorId AND OrgId=@orgId

IF @currentStatus IS NULL
    SET @checkResult=0;
ELSE IF @currentStatus & @statusValue = @statusValue
    SET @checkResult=1;
ELSE
    SET @checkResult=0;