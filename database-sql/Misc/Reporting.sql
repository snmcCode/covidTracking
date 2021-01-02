
DECLARE @startDate DATETIME2(7)
DECLARE @endDate DATETIME2(7)

SET @startDate='2021-01-02 00:00:00'
SET @endDate='2021-01-02 23:59:59'

--Summary

SELECT e.Name as 'Prayer',e.[DateTime],count(ve.id) as 'Count'
FROM [event] e join visitor_event ve on e.Id=ve.eventId 
Where e.[DateTime] BETWEEN @startDate AND @endDate
GROUP BY e.Name,e.[DateTime] 

--Details 
SELECT v.firstName,dbo.udfLastPortionOfId(v.Id) as 'ID',e.Name as 'Prayer',e.[DateTime] as prayerTime,ve.[DateTime] as RegistrationTime
FROM [event] e JOIN visitor_event ve ON e.Id=ve.eventId  JOIN  visitor v on v.Id=ve.visitorId
Where e.[DateTime] BETWEEN @startDate AND @endDate
order by e.[DateTime]