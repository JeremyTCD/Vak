CREATE PROCEDURE [Website].[GetMemberRoles]
	@MemberId INT
AS
BEGIN
	SELECT Roles.RoleId, Roles.Name
	FROM [dbo].[Members] AS Members 
		INNER JOIN [dbo].[MemberRoles] AS MemberRoles
		ON Members.MemberId = MemberRoles.MemberId
		INNER JOIN [dbo].[Roles] AS Roles
		ON MemberRoles.RoleId = Roles.RoleId
	WHERE Members.MemberId = @MemberId  
END

