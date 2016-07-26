CREATE PROCEDURE [Website].[AddMemberClaim]
	@MemberId INT,
	@ClaimId INT
AS
BEGIN
	SET NOCOUNT ON;
	SET XACT_ABORT ON;

	INSERT INTO [dbo].[MemberClaims] ([MemberId], [ClaimId])
	VALUES (@MemberId, @ClaimId)
END