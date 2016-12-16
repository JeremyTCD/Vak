CREATE PROCEDURE [Accounts].[DeleteAccount]
	@AccountId INT,
	@RowVersion ROWVERSION = NULL
AS
BEGIN
	SET NOCOUNT ON;
	SET XACT_ABORT ON;

	Delete from [dbo].[Accounts] 
	where [AccountId] = @AccountId AND (@RowVersion IS NULL OR [RowVersion]=@RowVersion)

	IF @@ROWCOUNT = 0
	BEGIN;
		THROW 51000, N'Invalid RowVersion or AccountId', 1;
	END;
END