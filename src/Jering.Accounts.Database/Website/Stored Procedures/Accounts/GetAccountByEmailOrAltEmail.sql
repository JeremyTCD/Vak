CREATE PROCEDURE [Website].[GetAccountByEmailOrAltEmail]
	@Email NVARCHAR(256)
AS
BEGIN
	SET NOCOUNT ON;
	SET XACT_ABORT ON;

	SELECT *
	FROM [dbo].[Accounts]
	WHERE [Email] = @Email OR [AltEmail] = @Email;
END

