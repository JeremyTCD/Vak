CREATE PROCEDURE [Accounts].[GetAccountByEmail]
	@Email NVARCHAR(256)
AS
BEGIN
	SET NOCOUNT ON;
	SET XACT_ABORT ON;

	SELECT *
	FROM [dbo].[Accounts]
	WHERE [Email] = @Email;

	IF @@ROWCOUNT = 0
	BEGIN;
		THROW 51000, N'Invalid Email', 1;
	END;
END

