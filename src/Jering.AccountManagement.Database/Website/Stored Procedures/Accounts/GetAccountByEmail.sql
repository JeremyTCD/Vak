CREATE PROCEDURE [Website].[GetAccountByEmail]
	@Email NVARCHAR(256)
AS
BEGIN
	SET NOCOUNT ON;
	SET XACT_ABORT ON;

	SELECT [AccountId], [DisplayName], [PasswordLastChanged], [SecurityStamp], [Email], [EmailVerified], [AlternativeEmail], [AlternativeEmailVerified], [TwoFactorEnabled]
	FROM [dbo].[Accounts]
	WHERE [Email] = @Email;
END

