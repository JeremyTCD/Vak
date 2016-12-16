CREATE PROCEDURE [Accounts].[DeleteRole]
	@RoleId INT
AS
BEGIN
	SET NOCOUNT ON;
	SET XACT_ABORT ON;

	Delete from [dbo].[Roles] where [RoleId] = @RoleId
	
	SELECT @@ROWCOUNT;
END