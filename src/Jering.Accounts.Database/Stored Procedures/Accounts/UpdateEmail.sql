CREATE PROCEDURE [Accounts].[UpdateEmail]
	@AccountId INT,
	@Email NVARCHAR(256),
	@RowVersion ROWVERSION = NULL
AS
BEGIN	
	SET NOCOUNT ON;
	SET XACT_ABORT ON;

	UPDATE [dbo].[Accounts]
	SET [Email] = @Email,
		[SecurityStamp] = NEWID(),
		[EmailVerified] = 0,
		[TwoFactorEnabled] = 0
	OUTPUT INSERTED.[RowVersion], INSERTED.[SecurityStamp]
	WHERE [AccountId] = @AccountId AND (@RowVersion IS NULL OR [RowVersion]=@RowVersion);

	IF @@ROWCOUNT = 0
	BEGIN;
		THROW 51000, N'Invalid RowVersion or AccountId', 1;
	END;
END;
