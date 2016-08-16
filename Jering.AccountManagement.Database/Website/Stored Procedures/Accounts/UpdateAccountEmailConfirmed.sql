CREATE PROCEDURE [Website].[UpdateAccountEmailConfirmed]
	@AccountId INT
AS
BEGIN	
	SET NOCOUNT ON;
	SET XACT_ABORT ON;

	UPDATE [dbo].[Accounts]
	SET [SecurityStamp] = CASE WHEN [EmailConfirmed] = 1 THEN [SecurityStamp] ELSE NEWID() END, 
		[EmailConfirmed] = 1
	WHERE AccountId = @AccountId;

	SELECT @@ROWCOUNT;
END
