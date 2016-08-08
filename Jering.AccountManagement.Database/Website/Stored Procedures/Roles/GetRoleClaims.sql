CREATE PROCEDURE [Website].[GetRoleClaims]
	@RoleId INT
AS
BEGIN
	SET NOCOUNT ON;
	SET XACT_ABORT ON;

	SELECT Claims.*
	--Try using exists to perform right semi joins, see if it is more performant
	FROM [dbo].[Roles] AS Roles
		INNER JOIN [dbo].[RoleClaims] AS RoleClaims
		ON Roles.RoleId = RoleClaims.RoleId
		INNER JOIN [dbo].[Claims] AS Claims
		ON RoleClaims.ClaimId = Claims.ClaimId
	WHERE Roles.RoleId = @RoleId  
END

