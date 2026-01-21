CREATE TABLE [dbo].[CrystalReport](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[ReportName] [nvarchar](50) NULL,
	[ReportPathFile] [nvarchar](254) NULL,
	[FTWebCrystalReportURL] [nvarchar](max) NULL,
	[ReportGroupingName] [nvarchar](50) NULL,
	[IsLayout] [bit] NOT NULL,
 CONSTRAINT [PK_CrystalReport] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CrystalReportParam]    Script Date: 1/20/2026 5:03:26 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CrystalReportParam](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[CrReportId] [bigint] NOT NULL,
	[ParamCode] [nvarchar](50) NOT NULL,
	[ParamName] [nvarchar](50) NOT NULL,
	[ParamType] [nvarchar](50) NULL,
	[ParamSql] [nvarchar](max) NULL,
 CONSTRAINT [PK_CrystalReportParam] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CrystalReportSetup]    Script Date: 1/20/2026 5:03:26 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CrystalReportSetup](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[CrServer] [nvarchar](200) NOT NULL,
	[CrDatabase] [nvarchar](200) NOT NULL,
	[CrDBUser] [nvarchar](50) NOT NULL,
	[CrDBPwd] [nvarchar](200) NOT NULL,
	[CrURL] [nvarchar](max) NULL,
	[CrOutputFolder] [nvarchar](max) NULL,
 CONSTRAINT [PK_CrystalReportSetup] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE [dbo].[CrystalReport] ADD  DEFAULT ((0)) FOR [IsLayout]
GO
ALTER TABLE [dbo].[CrystalReportParam]  WITH CHECK ADD FOREIGN KEY([CrReportId])
REFERENCES [dbo].[CrystalReport] ([Id])
ON DELETE CASCADE
GO
/****** Object:  StoredProcedure [dbo].[LoadCrConn_sp]    Script Date: 1/20/2026 5:03:26 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create procedure
[dbo].[LoadCrConn_sp] 
@docid int
AS
begin
	select top 1 T1.CrServer, T1.CrDatabase, T1.CrDBUser, T1.CrDBPwd 
	from CrystalReportSetup T1 
end
GO
/****** Object:  StoredProcedure [dbo].[LoadCrConnByDB_sp]    Script Date: 1/20/2026 5:03:26 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[LoadCrConnByDB_sp] 
@DBName nvarchar(50)
AS
begin
	SELECT * FROM SAPDB WHERE DbName = @DBName
	
end
GO
/****** Object:  StoredProcedure [dbo].[LoadCrReportSetup]    Script Date: 1/20/2026 5:03:26 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
 CREATE PROCEDURE [dbo].[LoadCrReportSetup]
 AS
 BEGIN
	SELECT TOP 1 CrURL FROM CrystalReportSetup
 END
GO
/****** Object:  StoredProcedure [dbo].[LoadORPTById_sp]    Script Date: 1/20/2026 5:03:26 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


create PROCEDURE [dbo].[LoadORPTById_sp]
	@OID int
AS
begin
	SELECT T0.Id AS [OID], T0.ReportName, T0.ReportPathFile
	 FROM CrystalReport T0 
	 WHERE T0.Id = @OID

end
GO
/****** Object:  StoredProcedure [dbo].[LoadORPTById_sp_afterlayoutprint]    Script Date: 1/20/2026 5:03:26 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create PROCEDURE [dbo].[LoadORPTById_sp_afterlayoutprint]
	@OID int,
	@docid int
AS
begin
	declare @reportname nvarchar(100)

	SELECT @reportname = T0.ReportName
	 FROM CrystalReport T0 
	 WHERE T0.Id = @OID

	 --update transaction data by @docid base on @reportname
end
GO
/****** Object:  StoredProcedure [dbo].[LoadRPT1ByHdr_sp]    Script Date: 1/20/2026 5:03:26 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


create PROCEDURE [dbo].[LoadRPT1ByHdr_sp]
	@CrReport int -- header oid
as
begin
	SELECT T0.Id AS [OID], T0.CrReportId AS [CrReport], T0.ParamCode, T0.ParamName
	, T0.ParamType -- 0 = string/ 1 = date
	, T0.ParamSQL
	 FROM CrystalReportParam T0 WHERE T0.CrReportId = @CrReport
end
GO
/****** Object:  StoredProcedure [dbo].[LoadRPT1ById_sp]    Script Date: 1/20/2026 5:03:26 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[LoadRPT1ById_sp]
	@OID int
as
begin
	SELECT T0.Id AS [OID], T0.CrReportId AS [CrReport], T0.ParamCode, T0.ParamName
	, T0.ParamType -- 0 = string/ 1 = date
	, T0.ParamSQL
	 FROM CrystalReportParam T0 WHERE T0.Id = @OID
end
GO
