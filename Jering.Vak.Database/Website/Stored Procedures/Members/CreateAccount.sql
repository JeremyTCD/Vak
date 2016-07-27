CREATE PROCEDURE [Website].[CreateAccount]
	@Password NVARCHAR(256),  
	@Email NVARCHAR(256)
AS
BEGIN
	SET NOCOUNT ON;
	SET XACT_ABORT ON;

	BEGIN TRY
		DECLARE @PasswordHash BINARY(32) = HASHBYTES(N'SHA2_256', @Password + @Email);

		INSERT INTO [dbo].[Accounts] ([PasswordHash], [Email], [SecurityStamp])
		OUTPUT INSERTED.[AccountId], INSERTED.[Email], CONVERT(NVARCHAR(64), INSERTED.[SecurityStamp]) AS SecurityStamp
		VALUES (@PasswordHash, @Email, NEWID())
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