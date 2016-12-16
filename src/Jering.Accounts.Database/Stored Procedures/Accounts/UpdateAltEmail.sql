CREATE PROCEDURE [Accounts].[UpdateAltEmail]
	@AccountId INT,
	@AltEmail NVARCHAR(256),
	@RowVersion ROWVERSION = NULL
AS
BEGIN	
	SET NOCOUNT ON;
	SET XACT_ABORT ON;

	UPDATE [dbo].[Accounts]
	SET [AltEmail] = @AltEmail
	OUTPUT INSERTED.[RowVersion]
	WHERE [AccountId] = @AccountId AND (@RowVersion IS NULL OR [RowVersion]=@RowVersion);

	IF @@ROWCOUNT = 0
	BEGIN;
		THROW 51000, N'Invalid RowVersion or AccountId', 1;
	END;
END;
