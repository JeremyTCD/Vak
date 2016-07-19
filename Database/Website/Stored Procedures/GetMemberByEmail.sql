CREATE PROCEDURE [Website].[GetMemberByEmail]
	@Email NVARCHAR(256)
AS
BEGIN
	SELECT [Username], [PasswordHash], [SecurityStamp], [Email], [EmailConfirmed], [TwoFactorEnabled], [LockoutEnabled], [MemberId] AS Id 
	FROM [dbo].[Members] 
	WHERE [Email]=@Email  
END

