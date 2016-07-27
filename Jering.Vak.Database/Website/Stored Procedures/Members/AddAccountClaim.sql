CREATE PROCEDURE [Website].[AddAccountClaim]
	@AccountId INT,
	@ClaimId INT
AS
BEGIN
	SET NOCOUNT ON;
	SET XACT_ABORT ON;

	INSERT INTO [dbo].[AccountClaims] ([AccountId], [ClaimId])
	VALUES (@AccountId, @ClaimId)
END