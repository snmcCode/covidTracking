CREATE OR ALTER FUNCTION dbo.udfLastPortionOfId(@uniqueId varchar(100))  
RETURNS varchar(20)   
AS   
-- Returns the stock level for the product.  
BEGIN  
    DECLARE @ret varchar(20) ;  
    DECLARE @c char(1) = N'-';


    WITH baseTable
    AS(
    Select value,ix = ROW_NUMBER() OVER (ORDER BY CHARINDEX(@c + value + @c, @c + @uniqueId + @c))
    FROM STRING_SPLIT(CAST(@uniqueId as varchar(100)),'-') s
    )Select @ret=Lower(value) from baseTable where ix=5


    RETURN @ret;  
END;