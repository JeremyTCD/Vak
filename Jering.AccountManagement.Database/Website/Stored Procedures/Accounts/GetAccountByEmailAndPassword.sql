CREATE PROCEDURE [Website].[GetAccountByEmailAndPassword]
	@Email NVARCHAR(256),
	@Password NVARCHAR(256)
AS
BEGIN
	SET NOCOUNT ON;
	SET XACT_ABORT ON;

	SELECT *
	FROM [dbo].[Accounts]
	WHERE [Email] = @Email AND [PasswordHash] = HASHBYTES(N'SHA2_256', @Password + @Email);
END

