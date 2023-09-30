CREATE DATABASE [Demo]
GO

USE [Demo]
GO

/****** Object:  Table [dbo].[Raw]    Script Date: 27.09.2023 21:30:10 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Account](
	[Id] [nvarchar](50) NOT NULL,
	[Uri] [nvarchar](2000) NOT NULL,
	[AccountName] [nvarchar](50) NOT NULL
) ON [PRIMARY]
GO


CREATE TABLE [dbo].[Raw](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[json] [nvarchar](max) NULL,
	[status] [int] NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[Raw] ADD  CONSTRAINT [DF_Raw_status]  DEFAULT ((0)) FOR [status]
GO


CREATE TABLE [dbo].[Person](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[FirstName] [nvarchar](50) NOT NULL,
	[LastName] [nvarchar](50) NOT NULL,
	[Gender] [nvarchar](50) NOT NULL,
	[BirthDate] [date] NOT NULL
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[Person] ADD  CONSTRAINT [DF_Person_CreatedOn]  DEFAULT (getdate()) FOR [CreatedOn]
GO


CREATE TABLE [dbo].[Log] (
    [Id] [int] IDENTITY (1, 1) NOT NULL,
    [Date] [datetime] NOT NULL,
    [Thread] [varchar] (255) NOT NULL,
    [Level] [varchar] (50) NOT NULL,
    [Logger] [varchar] (255) NOT NULL,
    [Message] [varchar] (4000) NOT NULL,
    [Exception] [varchar] (2000) NULL
)
go