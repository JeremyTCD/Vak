CREATE PROCEDURE [Website].[CheckDisplayNameInUse]
	@DisplayName NVARCHAR(256)
AS
BEGIN
	SET NOCOUNT ON;
	SET XACT_ABORT ON;

	SELECT
	   CASE WHEN EXISTS(SELECT * FROM [Accounts] WHERE [DisplayName] = @DisplayName)
	   THEN 1 
	   ELSE 0 
	   END 
END

