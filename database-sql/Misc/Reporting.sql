--Summary

SELECT e.Name as 'Prayer',e.[DateTime],count(ve.id) as 'Count'
FROM [event] e join visitor_event ve on e.Id=ve.eventId 
Where e.[DateTime] BETWEEN '2020-12-30 00:00:00' AND '2020-12-30 23:59:59'
GROUP BY e.Name,e.[DateTime] 

--Details 
SELECT v.firstName,dbo.udfLastPortionOfId(v.Id) as 'ID',e.Name as 'Prayer',e.[DateTime]
FROM [event] e JOIN visitor_event ve ON e.Id=ve.eventId  JOIN  visitor v on v.Id=ve.visitorId
Where e.[DateTime] BETWEEN '2020-12-30 00:00:00' AND '2020-12-30 23:59:59'
order by e.[DateTime]