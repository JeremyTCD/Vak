CREATE PROCEDURE [Accounts].[CreateRole]
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
			THROW 51000, N'Role already exists.', 1;
		ELSE 
			THROW
    END CATCH;
END