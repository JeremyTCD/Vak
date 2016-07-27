CREATE PROCEDURE [Website].[AddAccountRole]
	@AccountId INT,
	@RoleId INT
AS
BEGIN
	INSERT INTO [dbo].[AccountRoles] ([AccountId], [RoleId])
	VALUES (@AccountId, @RoleId)
END