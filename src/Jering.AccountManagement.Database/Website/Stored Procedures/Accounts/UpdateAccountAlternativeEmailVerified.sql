CREATE PROCEDURE [Website].[UpdateAccountAlternativeEmailVerified]
	@AccountId INT,
	@AlternativeEmailVerified BIT
AS
BEGIN	
	SET NOCOUNT ON;
	SET XACT_ABORT ON;

	UPDATE [dbo].[Accounts]
	SET [AlternativeEmailVerified] = @AlternativeEmailVerified
	WHERE AccountId = @AccountId;

	SELECT @@ROWCOUNT;
END
