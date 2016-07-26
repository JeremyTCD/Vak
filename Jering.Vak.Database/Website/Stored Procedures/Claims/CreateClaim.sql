CREATE PROCEDURE [Website].[CreateClaim]
	@Type NVARCHAR(MAX),  
	@Value NVARCHAR(MAX)
AS
BEGIN
	SET NOCOUNT ON;
	SET XACT_ABORT ON;

	BEGIN TRY
		INSERT INTO [dbo].[Claims] ([Type], [Value])
		OUTPUT INSERTED.*
		VALUES (@Type, @Value)
	END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
		DECLARE @errorNumber INT = ERROR_NUMBER();

		IF @errorNumber = 2627 
			BEGIN
				DECLARE @errorMessage NVARCHAR(MAX) = FORMATMESSAGE(N'A claim with type "%s" and value "%s" already exists.', @Type, @Value);
				THROW 51000, @errorMessage, 1;
			END
		ELSE 
			THROW 51000, N'An unexpected error occurred.', 1;
    END CATCH;
END