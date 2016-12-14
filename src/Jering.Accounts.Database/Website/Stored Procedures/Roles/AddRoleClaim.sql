CREATE PROCEDURE [Website].[AddRoleClaim]
	@RoleId INT,
	@ClaimId INT
AS
BEGIN
	SET NOCOUNT ON;
	SET XACT_ABORT ON;

	BEGIN TRY
		INSERT INTO [dbo].[RoleClaims] ([RoleId], [ClaimId])
		VALUES (@RoleId, @ClaimId);

		SELECT @@ROWCOUNT;
	END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
		DECLARE @errorNumber INT = ERROR_NUMBER();
		DECLARE @errorMessage NVARCHAR(MAX);

		IF @errorNumber = 2627 
			THROW 51000, N'Role already has claim.', 1;
		ELSE IF @errorNumber = 547
			THROW 51000, N'Role or claim does not exist.', 1;
		ELSE
			THROW
    END CATCH;
END