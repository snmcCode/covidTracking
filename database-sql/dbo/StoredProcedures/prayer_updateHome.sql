CREATE OR ALTER PROCEDURE prayer_updateHome @domainName varchar(100)='checkin.mysnmc.ca',
@fajr CHAR(5),@shorooq CHAR(5),@dhuhr CHAR(5),@asr CHAR(5),@maghreb CHAR(5),@isha CHAR(5)
AS

DECLARE @valueString VARCHAR(2000)='<div style="text-align: left;font-weight: bold">
Prayer times <br/>
<ul style="text-align: left">
<li>Fajr   : '+ @fajr +' </li>
<li>Sunrise   : '+ @shorooq +' </li>
<li>Dhuhr  : '+ @dhuhr +' </li>
<li>Asr    : '+ @asr +'</li>
<li>Maghreb: '+ @maghreb +' </li>
<li>Isha   : '+ @isha +' </li>
</ul></div>'

update setting set Value=@valueString
where [key]='homeAnnouncement' and [domain]='checkin.mysnmc.ca'