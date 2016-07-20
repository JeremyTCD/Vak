CREATE PROCEDURE [Website].[InsertMember]
	@PasswordHash NVARCHAR(MAX),  
	@SecurityStamp NVARCHAR(MAX), 
	@Email NVARCHAR(256),
	@NormalizedEmail NVARCHAR(256)
AS

INSERT INTO [dbo].[Members]
	(PasswordHash, SecurityStamp, Email, NormalizedEmail)
    OUTPUT INSERTED.[MemberId]
    VALUES  (@PasswordHash, @SecurityStamp, @Email, @NormalizedEmail);