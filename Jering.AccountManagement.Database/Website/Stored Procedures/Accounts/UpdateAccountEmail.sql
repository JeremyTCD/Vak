CREATE PROCEDURE [Website].[UpdateAccountEmail]
	@AccountId INT,
	@Email NVARCHAR(256)
AS
BEGIN	
	SET NOCOUNT ON;
	SET XACT_ABORT ON;

	BEGIN TRY
		UPDATE [dbo].[Accounts]
		SET [SecurityStamp] = NEWID(), 
			[Email] = @Email
		WHERE AccountId = @AccountId;

		SELECT @@ROWCOUNT;
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
