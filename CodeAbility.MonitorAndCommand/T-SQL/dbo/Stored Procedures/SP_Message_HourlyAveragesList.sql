

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[SP_Message_HourlyAveragesList]
	@intNumberOfMessages int
   ,@strDeviceName varchar(50) = null
   ,@strObjectName varchar(50) = null
   ,@strParameterName varchar(50) = null
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @average FLOAT = 0;
	DECLARE @standardDeviation FLOAT = 0;
	DECLARE @lowestTimeStamp date = DATEADD(day, -8, GetDate());

	SELECT @average = AVG(CAST([Content] AS FLOAT)) 
	  FROM [dbo].[TB_Message]
	 WHERE [SendingDevice] = @strDeviceName
	   AND [Name] = @strObjectName
	   AND [Parameter] = @strParameterName
	   AND [TimeStamp] > @lowestTimeStamp
	   AND (CAST([Content] AS FLOAT) > 0 And CAST([Content] AS FLOAT) <= 100);

	SELECT @standardDeviation = STDEV(CAST([Content] AS FLOAT))
	  FROM [dbo].[TB_Message]
	 WHERE [SendingDevice] = @strDeviceName
	   AND [Name] = @strObjectName
	   AND [Parameter] = @strParameterName
	   AND [TimeStamp] > @lowestTimeStamp
	   AND (CAST([Content] AS FLOAT) > 0 And CAST([Content] AS FLOAT) <= 100);

	SELECT TOP (@intNumberOfMessages) 
	       * 
	  FROM (SELECT iT.YEAR, iT.MONTH, iT.DAY, iT.HOUR, 0 as MINUTE, AVG(iT.Value) as Average
			  FROM (SELECT datepart(year, [TimeStamp]) as YEAR
						  ,datepart(month, [TimeStamp]) as MONTH
						  ,datepart(day, [TimeStamp]) as DAY
						  ,datepart(hour, [TimeStamp]) AS HOUR
						  ,CAST([Content] AS FLOAT) AS VALUE 
					  FROM [dbo].[TB_Message]
					 WHERE [SendingDevice] = @strDeviceName
					   AND [Name] = @strObjectName
			           AND [Parameter] = @strParameterName
					   AND [TimeStamp] > @lowestTimeStamp) iT
			 WHERE Value > @average - @standardDeviation 
			   AND Value < @average + @standardDeviation 
			 GROUP BY iT.YEAR, iT.MONTH, iT.DAY, iT.HOUR) eT
	 ORDER BY eT.YEAR, eT.MONTH, eT.DAY, eT.HOUR;
	 
END