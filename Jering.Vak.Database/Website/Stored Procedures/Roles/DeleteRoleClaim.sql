CREATE PROCEDURE [Website].[DeleteRoleClaim]
	@RoleId INT,
	@ClaimId INT
AS
BEGIN
	SET NOCOUNT ON;
	SET XACT_ABORT ON;

	Delete from [dbo].[RoleClaims] where [RoleId] = @RoleId AND [ClaimId] = @ClaimId
END