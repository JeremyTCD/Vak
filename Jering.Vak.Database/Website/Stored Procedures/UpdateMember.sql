CREATE PROCEDURE [Website].[UpdateMember]
	@Id INT,
	@Username NVARCHAR(256), 
	@PasswordHash NVARCHAR(MAX),  
	@SecurityStamp NVARCHAR(MAX), 
	@Email NVARCHAR(256),
    @EmailConfirmed BIT, 
	@TwoFactorEnabled BIT, 
	@LockoutEnabled BIT,
	@LockoutEndDateUTC DATETIME2,
	@AccessFailedCount INT
AS
BEGIN
	UPDATE [dbo].[Members] SET 
		Username = @Username, 
		PasswordHash = @PasswordHash, 
		SecurityStamp = @SecurityStamp, 
		Email=@Email, 
		EmailConfirmed=@EmailConfirmed, 
		AccessFailedCount=@AccessFailedCount, 
		LockoutEnabled=@LockoutEnabled, 
		LockoutEndDateUTC=@LockoutEndDateUTC, 
		TwoFactorEnabled=@TwoFactorEnabled 
	WHERE [MemberId] = @Id
END