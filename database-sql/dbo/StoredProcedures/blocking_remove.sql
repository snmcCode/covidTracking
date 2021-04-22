CREATE OR ALTER PROCEDURE blocking_remove
AS
DELETE blocked WHERE DATEDIFF(day,ExpiryDate, GETDATE()) >= 1