CREATE PROCEDURE [Website].[UpdateAccountEmailConfirmed]
	@AccountId INT
AS
BEGIN	
	SET NOCOUNT ON;
	SET XACT_ABORT ON;

	UPDATE [dbo].[Accounts]
	SET [EmailVerified] = 1
	WHERE AccountId = @AccountId;

	SELECT @@ROWCOUNT;
END
