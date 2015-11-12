-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[SP_Event_Insert]
	 @fromDevice varchar(50)
	,@content varchar(max)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	INSERT INTO [dbo].[TB_Event]
			   ([FromDevice]
			   ,[Content])
		 VALUES
			   (@fromDevice
			   ,@content);
END