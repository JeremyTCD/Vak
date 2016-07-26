CREATE PROCEDURE [Website].[DeleteClaim]
	@ClaimId INT
AS
BEGIN
	SET NOCOUNT ON;
	SET XACT_ABORT ON;

	Delete from [dbo].[Claims] where [ClaimId] = @ClaimId
END