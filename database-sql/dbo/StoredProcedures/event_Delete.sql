CREATE OR ALTER PROCEDURE event_delete
(@id int)
AS
DELETE event where Id=@id