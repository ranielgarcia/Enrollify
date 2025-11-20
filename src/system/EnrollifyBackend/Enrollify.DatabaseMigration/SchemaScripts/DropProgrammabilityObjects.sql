/* Drop all non-system stored procs */
DECLARE @name VARCHAR(128)
DECLARE @SQL VARCHAR(254)

SELECT @name = (SELECT TOP 1 [name] FROM sysobjects WHERE [type] = 'P' AND category = 0 ORDER BY [name])

WHILE @name IS NOT NULL
BEGIN
	SELECT @SQL = 'DROP PROCEDURE [dbo].['+ RTRIM(@name) +']'
	EXEC (@SQL)
	PRINT 'Dropeed Procedure: ' + @name
	SELECT @name = (SELECT TOP 1 [name] FROM sysobjects WHERE [type] = 'P' AND category = 0 AND [name] > @name ORDER BY [name])
END
GO

/* Drop all views */
DECLARE @name VARCHAR(128)
DECLARE @SQL VARCHAR(254)

SELECT @name = (SELECT TOP 1 [name] FROM sysobjects WHERE [type] = 'V' AND category = 0 ORDER BY [name])

WHILE @name IS NOT NULL
BEGIN
	SELECT @SQL = 'DROP VIEW [dbo].['+ RTRIM(@name) +']'
	EXEC (@SQL)
	PRINT 'Dropeed View: ' + @name
	SELECT @name = (SELECT TOP 1 [name] FROM sysobjects WHERE [type] = 'V' AND category = 0 AND [name] > @name ORDER BY [name])
END
GO

/* Drop all functions */
DECLARE @name VARCHAR(128)
DECLARE @SQL VARCHAR(254)

SELECT @name = (SELECT TOP 1 [name] FROM sysobjects WHERE [type] IN (N'FN', N'IF', N'TF', N'FS', N'FT') AND category=0 ORDER BY [name])

WHILE @name IS NOT NULL
BEGIN
	SELECT @SQL = 'DROP FUNCTION [dbo].['+ RTRIM(@name) +']'
	EXEC (@SQL)
	PRINT 'Dropeed Function: ' + @name
	SELECT @name = (SELECT TOP 1 [name] FROM sysobjects WHERE [type] IN (N'FN', N'IF', N'TF', N'FS', N'FT') AND category=0 AND [name] > @name ORDER BY [name])
END
GO
