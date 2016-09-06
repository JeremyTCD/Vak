CREATE PROCEDURE [Website].[UpdateAccountPasswordHash]
	@AccountId INT,
	@Password NVARCHAR(256)
AS
BEGIN	
	SET NOCOUNT ON;
	SET XACT_ABORT ON;

	DECLARE @PasswordHash BINARY(32) = 0;

	UPDATE [dbo].[Accounts]
	SET @PasswordHash = HASHBYTES(N'SHA2_256', @Password + [Email]),
		[SecurityStamp] = CASE WHEN [PasswordHash] = @PasswordHash THEN [SecurityStamp] ELSE NEWID() END, 
		[PasswordLastChanged] = CASE WHEN [PasswordHash] = @PasswordHash THEN [PasswordLastChanged] ELSE GETUTCDATE() END,  	
		[PasswordHash] = @PasswordHash
	WHERE AccountId = @AccountId;

	SELECT @@ROWCOUNT;
END
