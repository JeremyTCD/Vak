CREATE PROCEDURE [Website].[DeleteAccountClaim]
	@ClaimId INT,
	@AccountId INT
AS
BEGIN
	SET NOCOUNT ON;
	SET XACT_ABORT ON;

	BEGIN TRAN
		Delete from [dbo].[AccountClaims] where [ClaimId] = @ClaimId AND [AccountId] = @AccountId;
		DECLARE @rowCount INT = @@ROWCOUNT;
		SELECT @rowCount;
		IF @rowCount > 0
		BEGIN
			UPDATE [dbo].[Accounts]
			SET [SecurityStamp] = NEWID()
			WHERE [AccountId] = @AccountId;
		END
	COMMIT
END