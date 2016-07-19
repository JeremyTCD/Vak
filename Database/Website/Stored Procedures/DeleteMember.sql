CREATE PROCEDURE [Website].[DeleteMember]
	@Id INT
AS
BEGIN
	Delete from [dbo].[Members] where [MemberId] = @Id
END