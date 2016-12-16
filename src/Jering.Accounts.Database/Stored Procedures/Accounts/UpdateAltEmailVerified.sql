CREATE PROCEDURE [Accounts].[UpdateAltEmailVerified]
	@AccountId INT,
	@AltEmailVerified BIT,
	@RowVersion ROWVERSION = NULL
AS
BEGIN	
	SET NOCOUNT ON;
	SET XACT_ABORT ON;

	UPDATE [dbo].[Accounts]
	SET [AltEmailVerified] = @AltEmailVerified
	WHERE AccountId = @AccountId AND (@RowVersion IS NULL OR [RowVersion]=@RowVersion);

	IF @@ROWCOUNT = 0
	BEGIN;
		THROW 51000, N'Invalid RowVersion or AccountId', 1;
	END;
END
