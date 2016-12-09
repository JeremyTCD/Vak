CREATE PROCEDURE [Website].[CreateAccount]
	@PasswordHash NVARCHAR(84),  
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

			INSERT INTO [dbo].[Accounts] ([PasswordHash], [Email])
			OUTPUT INSERTED.*
			VALUES (@PasswordHash, @Email)
		COMMIT
	END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
		DECLARE @errorNumber INT = ERROR_NUMBER();

		IF @errorNumber = 2627 
			THROW 51000, N'An account with this email already exists.', 1;
		ELSE 
			THROW
    END CATCH;
END