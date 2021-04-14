CREATE OR ALTER PROCEDURE [dbo].[prayer_getToday] @orgId int
AS
DECLARE @localDate DATE
SET @localDate=DATEADD(day,1,(select CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'Eastern Standard Time' AS date)))

SELECT * FROM dbo.prayer WHERE orgId=@orgId AND dateDay=@localDate 