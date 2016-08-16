CREATE PROCEDURE [Website].[UpdateAccountTwoFactorEnabled]
	@AccountId INT,
	@TwoFactorEnabled BIT
AS
BEGIN	
	SET NOCOUNT ON;
	SET XACT_ABORT ON;

	UPDATE [dbo].[Accounts]
	SET [SecurityStamp] = CASE WHEN [TwoFactorEnabled] = @TwoFactorEnabled THEN [SecurityStamp] ELSE NEWID() END, 
		[TwoFactorEnabled] = @TwoFactorEnabled		
	WHERE [AccountId] = @AccountId;

	SELECT @@ROWCOUNT;
END
