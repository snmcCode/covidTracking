/****** Object:  Table [dbo].[blocked]    Script Date: 2021-04-21 10:52:32 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[blocked](
	[VisitorId] [uniqueidentifier] NOT NULL,
    [PhoneNumber] VARCHAR(15) NOT NULL,
	[OrgId] [int] NOT NULL,
	[BlockDate] [date] NOT NULL,
	[ExpiryDate] [date] NULL
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[blocked] ADD  CONSTRAINT [DF_blocked_BlockDate]  DEFAULT (getdate()) FOR [BlockDate]
GO

