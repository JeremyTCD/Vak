CREATE PROCEDURE [Website].[UpdateAccountEmailConfirmed]
	@AccountId INT
AS
BEGIN	
	SET NOCOUNT ON;
	SET XACT_ABORT ON;

	UPDATE [dbo].[Accounts]
	SET [EmailConfirmed] = 1, [SecurityStamp] = NEWID()
	WHERE AccountId = @AccountId;

	SELECT @@ROWCOUNT;
END
