CREATE TYPE EventsTableType AS TABLE(eventId INT)  

GO
CREATE OR ALTER PROCEDURE event_group(@events EventsTableType READONLY)
AS
Declare @count as int
Declare @groupID as UNIQUEIDENTIFIER

Select @count=count(*) from @events
IF @count>0
BEGIN
    BEGIN TRAN
    SELECT @groupID=NewId();
    UPDATE dbo.event SET GroupId=@groupID  FROM dbo.event e join @events e1 on e.Id=e1.eventId

    UPDATE dbo.visitor_event SET eventGroupId=@groupId FROM dbo.visitor_event ve JOIN @events e1 on ve.eventId=e1.eventId  

    COMMIT TRAN
END