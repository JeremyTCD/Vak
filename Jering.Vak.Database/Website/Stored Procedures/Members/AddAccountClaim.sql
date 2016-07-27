CREATE PROCEDURE [Website].[AddAccountClaim]
	@AccountId INT,
	@ClaimId INT
AS
BEGIN
	SET NOCOUNT ON;
	SET XACT_ABORT ON;

	BEGIN TRY
		INSERT INTO [dbo].[AccountClaims] ([AccountId], [ClaimId])
		VALUES (@AccountId, @ClaimId)
	END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
		DECLARE @errorNumber INT = ERROR_NUMBER();
		DECLARE @errorMessage NVARCHAR(MAX);

		IF @errorNumber = 2627 
			THROW 51000, N'Account already has claim.', 1;
		ELSE IF @errorNumber = 547
			THROW 51000, N'Account or claim does not exist.', 1;
		ELSE
			THROW 52000, N'An unexpected error occurred.', 1;
    END CATCH;
END