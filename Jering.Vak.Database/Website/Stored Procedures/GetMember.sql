CREATE PROCEDURE [Website].[GetMember]
	@MemberId INT
AS
BEGIN
	SELECT *
	FROM [dbo].[Members] 
	WHERE [MemberId]=@MemberId  
END

