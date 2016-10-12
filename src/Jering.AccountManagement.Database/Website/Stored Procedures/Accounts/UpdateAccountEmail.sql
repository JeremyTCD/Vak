CREATE PROCEDURE [Website].[UpdateAccountEmail]
	@AccountId INT,
	@Email NVARCHAR(256)
AS
BEGIN	
	SET NOCOUNT ON;
	SET XACT_ABORT ON;

	BEGIN TRY
		BEGIN TRANSACTION
			
			IF EXISTS(SELECT * FROM [dbo].[Accounts] WHERE [AlternativeEmail] = @Email)
				THROW 51000, 'EmailInUse', 1;

			--Need to ensure that isolation level prevents email from being written to an alt email 
			--between these statements

			UPDATE [dbo].[Accounts]
			SET [SecurityStamp] = NEWID(), 
				[Email] = @Email,
				[EmailVerified] = 0,
				[TwoFactorEnabled] = 0
			WHERE AccountId = @AccountId;

			SELECT @@ROWCOUNT;
		COMMIT
	END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
		DECLARE @errorNumber INT = ERROR_NUMBER();

		IF @errorNumber = 2627 
			THROW 51000, N'EmailInUse', 1;
		ELSE 
			THROW
    END CATCH;
END
