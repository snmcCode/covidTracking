CREATE OR ALTER PROCEDURE prayer_updateHome @domainName varchar(100)='checkin.mysnmc.ca',
@fajr CHAR(5),@fajrIqama CHAR(5),@shorooq CHAR(5),@dhuhrIqama CHAR(5),@asrIqama CHAR(5),@maghrebIqama CHAR(5),@ishaIqama CHAR(5)
AS

DECLARE @valueString VARCHAR(2000)='<div style="text-align: left;font-weight: bold">
Prayer times <br/>
<ul style="text-align: left">
<li>Fajr Athan : '+ @fajr +' </li>
<li>Fajr  : '+ @fajrIqama +' </li>
<li>Sunrise   : '+ @shorooq +' </li>
<li>Dhuhr  : '+ @dhuhrIqama +' </li>
<li>Asr    : '+ @asrIqama +'</li>
<li>Maghreb: '+ @maghrebIqama +' </li>
<li>Isha   : '+ @ishaIqama +' </li>
</ul></div>'

update setting set Value=@valueString
where [key]='homeAnnouncement' and [domain]='checkin.mysnmc.ca'