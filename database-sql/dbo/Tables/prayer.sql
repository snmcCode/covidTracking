CREATE TABLE [dbo].[prayer]
(
    orgId INT NOT NULL,
    [dateDay] DATE NOT NULL,
    fajr CHAR(5) NULL,
    fajrIqama CHAR(5) NULL,
    dhuhr CHAR(5) NULL,
    dhuhrIqama CHAR(5) NULL,
    asr CHAR(5) NULL,
    asrIqama CHAR(5) NULL,
    maghreb CHAR(5) NULL,
    maghrebIqama CHAR(5) NULL,
    isha CHAR(5) NULL,
    ishaIqama CHAR(5) NULL,
    taraweeh CHAR(5) NULL,
    CONSTRAINT [FK_organization_prayer] FOREIGN KEY ([orgId]) REFERENCES [dbo].[organization] ([Id])
)