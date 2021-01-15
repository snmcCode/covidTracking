CREATE OR ALTER PROCEDURE event_Update
(@id INT,@eventName NVARCHAR(100),
@eventDateTime DATETIME2(7),@hall NVARCHAR(50),
@capacity TINYINT,@isPrivate BIT,@targetAudience SMALLINT=null)
AS
UPDATE event
SET Name = COALESCE(@eventName,Name),
DateTime=COALESCE(@eventDateTime,DateTime),
Hall=COALESCE(@hall,Hall),
Capacity=COALESCE(@capacity,Capacity),
IsPrivate=COALESCE(@isPrivate,ISPrivate),
TargetAudience=COALESCE(@targetAudience,TargetAudience)
WHERE Id=@id