
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[SP_Message_FilteredList]
	@intNumberOfMessages int
   ,@strDeviceName varchar(50) = null
   ,@strObjectName varchar(50) = null
   ,@strParameterName varchar(50) = null
   ,@intRowInterval int = 1
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT TOP (@intNumberOfMessages) * 
	  FROM (SELECT ROW_NUMBER() OVER (ORDER BY [Id] DESC) AS rowNumber, *
			  FROM [dbo].[TB_Message]
			 WHERE (@strDeviceName IS NULL OR [SendingDevice] = @strDeviceName)
			   AND (@strObjectName IS NULL OR [Name] = @strObjectName)
			   AND (@strParameterName IS NULL OR [Parameter] = @strParameterName)) M 
	 WHERE rowNumber % @intRowInterval = 0
 ORDER BY rowNumber ASC;
END