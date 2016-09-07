CREATE PROCEDURE [Website].[CreateAccount]
	@Password NVARCHAR(256),  
	@Email NVARCHAR(256)
AS
BEGIN
	SET NOCOUNT ON;
	SET XACT_ABORT ON;

	BEGIN TRY
		DECLARE @Salt UNIQUEIDENTIFIER = NEWID();

		INSERT INTO [dbo].[Accounts] ([PasswordHash], [Salt], [Email])
		OUTPUT INSERTED.*
		VALUES (HASHBYTES(N'SHA2_256', @Password + CONVERT(char(36), @Salt)), @Salt, @Email)
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