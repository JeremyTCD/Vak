CREATE PROCEDURE [Website].[GetAccountByEmail]
	@Email NVARCHAR(256)
AS
BEGIN
	SET NOCOUNT ON;
	SET XACT_ABORT ON;

	SELECT *
	FROM [dbo].[Accounts]
	WHERE [dbo].[Accounts].[Email] = @Email;
END

