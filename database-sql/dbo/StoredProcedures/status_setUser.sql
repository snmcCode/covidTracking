CREATE OR ALTER PROCEDURE status_setUser
(@orgId int,@visitorId UniqueIdentifier,@statusValue INT)
AS
DECLARE @currentStatus INT 
Select @currentStatus=statusValue from dbo.visitor_status where VisitorId=@visitorId AND OrgId=@orgId
IF @currentStatus IS NULL
    INSERT INTO dbo.visitor_status(VisitorId,OrgId,StatusValue) VALUES(@visitorId,@orgId,@statusValue)
ELSE
    UPDATE dbo.visitor_status SET StatusValue |= @statusValue WHERE VisitorId=@visitorId AND OrgId=@orgId
