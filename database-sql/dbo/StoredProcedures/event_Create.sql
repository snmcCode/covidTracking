CREATE OR ALTER PROCEDURE event_Create
(@orgId INT,@eventName NVARCHAR(100),
@eventDateTime DATETIME2(7),@hall NVARCHAR(50),
@capacity TINYINT)
AS
INSERT dbo.event(OrgID,Name,DateTime,Hall,Capacity)
VALUES(@orgId,@eventName,@eventDateTime,@hall,@capacity)