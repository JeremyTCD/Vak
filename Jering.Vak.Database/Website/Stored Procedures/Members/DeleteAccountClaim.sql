CREATE PROCEDURE [Website].[DeleteAccountClaim]
	@ClaimId INT,
	@AccountId INT
AS
BEGIN
	Delete from [dbo].[AccountClaims] where [ClaimId] = @ClaimId AND [AccountId] = @AccountId
END