CREATE PROCEDURE [Website].[UpdateAccountDisplayName]
	@AccountId INT,
	@DisplayName NVARCHAR(256)
AS
BEGIN	
	SET NOCOUNT ON;
	SET XACT_ABORT ON;

	BEGIN TRY
		UPDATE [dbo].[Accounts]
		SET [DisplayName] = @DisplayName
		WHERE [AccountId] = @AccountId;

		SELECT @@ROWCOUNT;
	END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
		DECLARE @errorNumber INT = ERROR_NUMBER();

		IF @errorNumber = 2601 
			THROW 51000, N'DisplayNameInUse', 1;
		ELSE 
			THROW
    END CATCH;
END
