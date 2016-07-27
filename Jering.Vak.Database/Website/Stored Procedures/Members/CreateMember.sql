CREATE PROCEDURE [Website].[CreateMember]
	@Password NVARCHAR(256),  
	@Email NVARCHAR(256)
AS
BEGIN
	BEGIN TRY
		DECLARE @PasswordHash BINARY(32) = HASHBYTES(N'SHA2_256', @Password + @Email);

		INSERT INTO [dbo].[Members] ([PasswordHash], [Email])
		OUTPUT INSERTED.*
		VALUES (@PasswordHash, @Email)
	END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
		DECLARE @errorNumber INT = ERROR_NUMBER();

		IF @errorNumber = 2627 
			THROW 51000, N'An account with this email already exists.', 1;
		ELSE 
			THROW 52000, N'An unexpected error occurred.', 1;
    END CATCH;
END