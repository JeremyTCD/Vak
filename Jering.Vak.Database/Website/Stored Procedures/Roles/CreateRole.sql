CREATE PROCEDURE [Website].[CreateRole]
	@Name NVARCHAR(256)
AS
BEGIN
	SET NOCOUNT ON;
	SET XACT_ABORT ON;

	BEGIN TRY
		INSERT INTO [dbo].[Roles] ([Name])
		OUTPUT INSERTED.*
		VALUES (@Name)
	END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
		DECLARE @errorNumber INT = ERROR_NUMBER();

		IF @errorNumber = 2627 
			BEGIN
				DECLARE @errorMessage NVARCHAR(MAX) = FORMATMESSAGE(N'A role with name "%s" already exists.', @Name);
				THROW 51000, @errorMessage, 1;
			END
		ELSE 
			THROW 51000, N'An unexpected error occurred.', 1;
    END CATCH;
END