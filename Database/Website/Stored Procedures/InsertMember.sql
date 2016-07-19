CREATE PROCEDURE [Website].[InsertMember]
	@Id INT,
	@Username NVARCHAR(256), 
	@PasswordHash NVARCHAR(MAX),  
	@SecurityStamp NVARCHAR(MAX), 
	@Email NVARCHAR(256),
    @EmailConfirmed BIT, 
	@TwoFactorEnabled BIT, 
	@LockoutEnabled BIT,
	@LockoutEndDateUtc DATETIME2,
	@AccessFailedCount INT
AS

INSERT INTO [dbo].[Members]
	(Username, PasswordHash, SecurityStamp, Email, EmailConfirmed, TwoFactorEnabled, LockoutEnabled)
    OUTPUT INSERTED.[MemberId]
    VALUES  (@Username, @PasswordHash, @SecurityStamp, @Email, @EmailConfirmed, @TwoFactorEnabled, @LockoutEnabled);