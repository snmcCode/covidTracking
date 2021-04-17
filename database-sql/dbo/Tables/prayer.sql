
CREATE TABLE [dbo].[prayer](
	[orgId] [int] NOT NULL,
	[dateDay] [date] NOT NULL,
	[fajr] [char](5) NULL,
	[fajrIqama] [char](5) NULL,
	[shorooq] [char](5) NULL,
	[dhuhr] [char](5) NULL,
	[dhuhrIqama] [char](5) NULL,
	[asr] [char](5) NULL,
	[asrIqama] [char](5) NULL,
	[maghreb] [char](5) NULL,
	[maghrebIqama] [char](5) NULL,
	[isha] [char](5) NULL,
	[ishaIqama] [char](5) NULL,
	[taraweeh] [char](5) NULL,
 CONSTRAINT [PK_prayer] PRIMARY KEY CLUSTERED 
(
	[orgId] ASC,
	[dateDay] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[prayer]  ADD  CONSTRAINT [FK_organization_prayer] FOREIGN KEY([orgId])
REFERENCES [dbo].[organization] ([Id])
GO