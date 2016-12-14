CREATE PROCEDURE [Website].[UpdateAccountEmailVerified]
	@AccountId INT,
	@EmailVerified BIT
AS
BEGIN	
	SET NOCOUNT ON;
	SET XACT_ABORT ON;

	UPDATE [dbo].[Accounts]
	SET [EmailVerified] = @EmailVerified
	WHERE AccountId = @AccountId;

	SELECT @@ROWCOUNT;
END
