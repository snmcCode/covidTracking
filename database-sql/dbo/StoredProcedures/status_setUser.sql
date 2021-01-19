CREATE OR ALTER PROCEDURE status_setUser
(@orgId int,@visitorId UniqueIdentifier,@statusValue INT)
AS
DECLARE @currentStatus INT 
IF @statusValue is NULL OR @statusValue =0
    DELETE dbo.visitor_status WHERE VisitorId=@visitorId AND OrgId=@orgId;
ELSE
    UPDATE dbo.visitor_status SET StatusValue = @statusValue WHERE VisitorId=@visitorId AND OrgId=@orgId;
