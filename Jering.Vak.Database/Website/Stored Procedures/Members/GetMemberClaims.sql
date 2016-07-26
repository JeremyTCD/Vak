CREATE PROCEDURE [Website].[GetMemberClaims]
	@MemberId INT
AS
BEGIN
	SELECT Claims.ClaimId, Claims.Type, Claims.Value
	FROM [dbo].[Members] AS Members 
		INNER JOIN [dbo].[MemberClaims] AS MemberClaims
		ON Members.MemberId = MemberClaims.MemberId
		INNER JOIN [dbo].[Claims] AS Claims
		ON MemberClaims.ClaimId = Claims.ClaimId
	WHERE Members.MemberId = @MemberId  
END

