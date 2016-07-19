CREATE PROCEDURE [Website].[GetMember]
	@Id INT
AS
BEGIN
	SELECT [Username], [PasswordHash], [SecurityStamp], [Email], [EmailConfirmed], [TwoFactorEnabled], [AccessFailedCount], [LockoutEndDateUTC], [LockoutEnabled], [MemberId] AS Id 
	FROM [dbo].[Members] 
	WHERE [MemberId]=@Id  
END

