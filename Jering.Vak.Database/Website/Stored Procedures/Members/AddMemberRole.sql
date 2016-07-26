CREATE PROCEDURE [Website].[AddMemberRole]
	@MemberId INT,
	@RoleId INT
AS
BEGIN
	INSERT INTO [dbo].[MemberRoles] ([MemberId], [RoleId])
	VALUES (@MemberId, @RoleId)
END