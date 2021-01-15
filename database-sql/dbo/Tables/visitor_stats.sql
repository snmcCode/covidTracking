/****** Object:  Table [dbo].[visitor_status]    Script Date: 2021-01-13 11:49:27 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[visitor_status](
	[VisitorId] [uniqueidentifier] NOT NULL,
	[OrgId] [int] NOT NULL,
	[StatusValue] [Int] NOT NULL
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[visitor_status]  WITH CHECK ADD  CONSTRAINT [FK_visitor_status_organization] FOREIGN KEY([OrgId])
REFERENCES [dbo].[organization] ([Id])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[visitor_status] CHECK CONSTRAINT [FK_visitor_status_organization]
GO

ALTER TABLE [dbo].[visitor_status]  WITH CHECK ADD  CONSTRAINT [FK_visitor_status_visitor] FOREIGN KEY([VisitorId])
REFERENCES [dbo].[visitor] ([Id])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[visitor_status] CHECK CONSTRAINT [FK_visitor_status_visitor]

GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_visitor_status_Visitor_Org] ON [dbo].[visitor_status]
(
	[VisitorId] ASC,
	[OrgId] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO