CREATE PROCEDURE [Website].[GetMemberByUsername]
	@Username NVARCHAR(256)
AS
BEGIN
	SELECT [Username], [PasswordHash], [SecurityStamp], [Email], [EmailConfirmed], [TwoFactorEnabled], [LockoutEnabled], [MemberId] AS Id 
	FROM [dbo].[Members] 
	WHERE [Username]=@Username  
END

