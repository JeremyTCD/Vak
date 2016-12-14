CREATE PROCEDURE [Website].[UpdateAccountAltEmailVerified]
	@AccountId INT,
	@AltEmailVerified BIT
AS
BEGIN	
	SET NOCOUNT ON;
	SET XACT_ABORT ON;

	UPDATE [dbo].[Accounts]
	SET [AltEmailVerified] = @AltEmailVerified
	WHERE AccountId = @AccountId;

	SELECT @@ROWCOUNT;
END
