CREATE PROCEDURE [Website].[DeleteAccountRole]
	@RoleId INT,
	@AccountId INT
AS
BEGIN
	Delete from [dbo].[AccountRoles] where [RoleId] = @RoleId AND [AccountId] = @AccountId
END