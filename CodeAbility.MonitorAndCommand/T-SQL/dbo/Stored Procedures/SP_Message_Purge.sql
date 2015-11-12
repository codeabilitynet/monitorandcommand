-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[SP_Message_Purge]
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	DELETE  
	  FROM [dbo].[TB_Message];

	--CHECKIDENT not supported on Windows Azure
	--DBCC CHECKIDENT ('TB_Message', RESEED, 0);
END