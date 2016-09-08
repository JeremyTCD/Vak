CREATE PROCEDURE [Website].[CheckEmailAndPasswordExist]
	@Email NVARCHAR(256),
	@Password NVARCHAR(256)
AS
BEGIN
	SET NOCOUNT ON;
	SET XACT_ABORT ON;

	SELECT
	   CASE WHEN EXISTS(SELECT * FROM [Accounts] WHERE [Email] = @Email AND [PasswordHash] = HASHBYTES(N'SHA2_256', @Password + CONVERT(char(36), [Salt])))
	   THEN 1 
	   ELSE 0 
	   END 
END

