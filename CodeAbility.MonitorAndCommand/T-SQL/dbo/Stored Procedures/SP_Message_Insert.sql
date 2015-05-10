-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE SP_Message_Insert
	@sendingDevice varchar(50)
	,@receivingDevice varchar(50)
	,@fromDevice varchar(50)
	,@toDevice varchar(50)
	,@contentType varchar(50)
	,@name varchar(50)
	,@parameter varchar(50)
	,@content varchar(50)
	,@timeStamp datetime
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	INSERT INTO [dbo].[TB_Message]
			   ([SendingDevice]
			   ,[ReceivingDevice]
			   ,[FromDevice]
			   ,[ToDevice]
			   ,[ContentType]
			   ,[Name]
			   ,[Parameter]
			   ,[Content]
			   ,[TimeStamp])
		 VALUES
			   (@sendingDevice
			   ,@receivingDevice
			   ,@fromDevice
			   ,@toDevice
			   ,@contentType
			   ,@name
			   ,@parameter
			   ,@content
			   ,@timeStamp)
END
