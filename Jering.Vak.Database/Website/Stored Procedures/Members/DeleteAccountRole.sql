CREATE PROCEDURE [Website].[DeleteAccountRole]
	@RoleId INT,
	@AccountId INT
AS
BEGIN
	SET NOCOUNT ON;
	SET XACT_ABORT ON;

	BEGIN TRAN
		Delete from [dbo].[AccountRoles] where [RoleId] = @RoleId AND [AccountId] = @AccountId

		UPDATE [dbo].[Accounts]
		SET SecurityStamp = NEWID()
		WHERE AccountId = @AccountId;
	COMMIT
END