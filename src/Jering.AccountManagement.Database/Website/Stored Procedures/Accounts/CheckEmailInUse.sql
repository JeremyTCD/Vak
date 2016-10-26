﻿CREATE PROCEDURE [Website].[CheckEmailInUse]
	@Email NVARCHAR(256)
AS
BEGIN
	SET NOCOUNT ON;
	SET XACT_ABORT ON;

	SELECT
	   CASE WHEN EXISTS(SELECT * FROM [Accounts] WHERE [Email] = @Email OR [AlternativeEmail] = @Email)
	   THEN 1 
	   ELSE 0 
	   END 
END
