CREATE PROCEDURE [Website].[AddAccountRole]
	@AccountId INT,
	@RoleId INT
AS
BEGIN
	SET NOCOUNT ON;
	SET XACT_ABORT ON;

	BEGIN TRY
		BEGIN TRAN
			INSERT INTO [dbo].[AccountRoles] ([AccountId], [RoleId])
			VALUES (@AccountId, @RoleId);
			DECLARE @rowCount INT = @@ROWCOUNT;
			SELECT @rowCount;
			IF @rowCount > 0
			BEGIN
				UPDATE [dbo].[Accounts]
				SET SecurityStamp = NEWID()
				WHERE AccountId = @AccountId;
			END
		COMMIT
	END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
		DECLARE @errorNumber INT = ERROR_NUMBER();
		DECLARE @errorMessage NVARCHAR(MAX);

		IF @errorNumber = 2627 
			THROW 51000, N'Account already has role.', 1;
		ELSE IF @errorNumber = 547
			THROW 51000, N'Account or role does not exist.', 1;
		ELSE
			THROW
    END CATCH;
END