CREATE PROCEDURE [Website].[DeleteAccountClaim]
	@ClaimId INT,
	@AccountId INT
AS
BEGIN
	SET NOCOUNT ON;
	SET XACT_ABORT ON;

	BEGIN TRAN
		Delete from [dbo].[AccountClaims] where [ClaimId] = @ClaimId AND [AccountId] = @AccountId

		UPDATE [dbo].[Accounts]
		SET SecurityStamp = NEWID()
		WHERE AccountId = @AccountId;
	COMMIT
END