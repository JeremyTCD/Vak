CREATE PROCEDURE [Website].[DeleteRoleClaim]
	@RoleId INT,
	@ClaimId INT
AS
BEGIN
	Delete from [dbo].[RoleClaims] where [RoleId] = @RoleId AND [ClaimId] = @ClaimId
END