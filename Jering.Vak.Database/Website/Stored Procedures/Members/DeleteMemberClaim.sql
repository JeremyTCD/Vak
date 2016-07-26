CREATE PROCEDURE [Website].[DeleteMemberClaim]
	@ClaimId INT,
	@MemberId INT
AS
BEGIN
	Delete from [dbo].[MemberClaims] where [ClaimId] = @ClaimId AND [MemberId] = @MemberId
END