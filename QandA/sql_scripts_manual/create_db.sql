USE master
GO
IF NOT EXISTS (
 SELECT name
 FROM sys.databases
 WHERE name = N'TutorialDB'
)
 CREATE DATABASE [TutorialDB];
GO
IF SERVERPROPERTY('ProductVersion') > '12'
 ALTER DATABASE [TutorialDB] SET QUERY_STORE=ON;
GO