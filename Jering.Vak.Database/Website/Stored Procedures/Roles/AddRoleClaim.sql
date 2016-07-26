CREATE PROCEDURE [Website].[AddRoleClaim]
	@RoleId INT,
	@ClaimId INT
AS
BEGIN
	INSERT INTO [dbo].[RoleClaims] ([RoleId], [ClaimId])
	VALUES (@RoleId, @ClaimId)
END