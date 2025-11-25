CREATE TABLE [dbo].[ExcelConfig](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ExcelName] [nvarchar](100) NOT NULL,
	[ExcelPathFile] [nvarchar](200) NULL,
	[ExcelWebUrl] [nvarchar](max) NULL,
	[Active] [bit] NOT NULL DEFAULT (1))

	CREATE TABLE [dbo].[ExcelSheet](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ExcelId] [int] NOT NULL,
	[SheetName] [nvarchar](100) NOT NULL,
	[Query] [nvarchar](max) NOT NULL)



CREATE PROCEDURE LoadExcelConfigById_sp
@Id int
AS
BEGIN
	SELECT Id, ExcelName, ExcelPathFile, ExcelWebUrl, Active FROM ExcelConfig WHERE Id = @Id
END


CREATE PROCEDURE LoadExcelSheetsById_sp
@ExcelId int
AS
BEGIN
	SELECT Id, ExcelId, SheetName, Query FROM ExcelSheet WHERE ExcelId = @ExcelId
END
