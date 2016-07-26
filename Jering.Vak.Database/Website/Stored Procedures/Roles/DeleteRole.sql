CREATE PROCEDURE [Website].[DeleteRole]
	@RoleId INT
AS
BEGIN
	Delete from [dbo].[Roles] where [RoleId] = @RoleId
END