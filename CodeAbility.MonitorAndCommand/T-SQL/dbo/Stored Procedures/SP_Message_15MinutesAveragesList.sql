

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[SP_Message_15MinutesAveragesList]
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

	DECLARE @quarters TABLE (Minute int, Quarter int);

	INSERT INTO @quarters (Minute, Quarter) VALUES (0, 0);
	INSERT INTO @quarters (Minute, Quarter) VALUES (1, 0);
	INSERT INTO @quarters (Minute, Quarter) VALUES (2, 0);
	INSERT INTO @quarters (Minute, Quarter) VALUES (3, 0);
	INSERT INTO @quarters (Minute, Quarter) VALUES (4, 0);
	INSERT INTO @quarters (Minute, Quarter) VALUES (5, 0);
	INSERT INTO @quarters (Minute, Quarter) VALUES (6, 0);
	INSERT INTO @quarters (Minute, Quarter) VALUES (7, 0);
	INSERT INTO @quarters (Minute, Quarter) VALUES (8, 0);
	INSERT INTO @quarters (Minute, Quarter) VALUES (9, 0);
	INSERT INTO @quarters (Minute, Quarter) VALUES (10, 0);
	INSERT INTO @quarters (Minute, Quarter) VALUES (11, 0);
	INSERT INTO @quarters (Minute, Quarter) VALUES (12, 0);
	INSERT INTO @quarters (Minute, Quarter) VALUES (13, 0);
	INSERT INTO @quarters (Minute, Quarter) VALUES (14, 0);
	INSERT INTO @quarters (Minute, Quarter) VALUES (15, 15);
	INSERT INTO @quarters (Minute, Quarter) VALUES (16, 15);
	INSERT INTO @quarters (Minute, Quarter) VALUES (17, 15);
	INSERT INTO @quarters (Minute, Quarter) VALUES (18, 15);
	INSERT INTO @quarters (Minute, Quarter) VALUES (19, 15);
	INSERT INTO @quarters (Minute, Quarter) VALUES (20, 15);
	INSERT INTO @quarters (Minute, Quarter) VALUES (21, 15);
	INSERT INTO @quarters (Minute, Quarter) VALUES (22, 15);
	INSERT INTO @quarters (Minute, Quarter) VALUES (23, 15);
	INSERT INTO @quarters (Minute, Quarter) VALUES (24, 15);
	INSERT INTO @quarters (Minute, Quarter) VALUES (25, 15);
	INSERT INTO @quarters (Minute, Quarter) VALUES (26, 15);
	INSERT INTO @quarters (Minute, Quarter) VALUES (27, 15);
	INSERT INTO @quarters (Minute, Quarter) VALUES (28, 15);
	INSERT INTO @quarters (Minute, Quarter) VALUES (29, 15);
	INSERT INTO @quarters (Minute, Quarter) VALUES (30, 30);
	INSERT INTO @quarters (Minute, Quarter) VALUES (31, 30);
	INSERT INTO @quarters (Minute, Quarter) VALUES (32, 30);
	INSERT INTO @quarters (Minute, Quarter) VALUES (33, 30);
	INSERT INTO @quarters (Minute, Quarter) VALUES (34, 30);
	INSERT INTO @quarters (Minute, Quarter) VALUES (35, 30);
	INSERT INTO @quarters (Minute, Quarter) VALUES (36, 30);
	INSERT INTO @quarters (Minute, Quarter) VALUES (37, 30);
	INSERT INTO @quarters (Minute, Quarter) VALUES (38, 30);
	INSERT INTO @quarters (Minute, Quarter) VALUES (39, 30);
	INSERT INTO @quarters (Minute, Quarter) VALUES (40, 30);
	INSERT INTO @quarters (Minute, Quarter) VALUES (41, 30);
	INSERT INTO @quarters (Minute, Quarter) VALUES (42, 30);
	INSERT INTO @quarters (Minute, Quarter) VALUES (43, 30);
	INSERT INTO @quarters (Minute, Quarter) VALUES (44, 30);
	INSERT INTO @quarters (Minute, Quarter) VALUES (45, 45);
	INSERT INTO @quarters (Minute, Quarter) VALUES (46, 45);
	INSERT INTO @quarters (Minute, Quarter) VALUES (47, 45);
	INSERT INTO @quarters (Minute, Quarter) VALUES (48, 45);
	INSERT INTO @quarters (Minute, Quarter) VALUES (49, 45);
	INSERT INTO @quarters (Minute, Quarter) VALUES (50, 45);
	INSERT INTO @quarters (Minute, Quarter) VALUES (51, 45);
	INSERT INTO @quarters (Minute, Quarter) VALUES (52, 45);
	INSERT INTO @quarters (Minute, Quarter) VALUES (53, 45);
	INSERT INTO @quarters (Minute, Quarter) VALUES (54, 45);
	INSERT INTO @quarters (Minute, Quarter) VALUES (55, 45);
	INSERT INTO @quarters (Minute, Quarter) VALUES (56, 45);
	INSERT INTO @quarters (Minute, Quarter) VALUES (57, 45);
	INSERT INTO @quarters (Minute, Quarter) VALUES (58, 45);
	INSERT INTO @quarters (Minute, Quarter) VALUES (59, 45);

	SELECT TOP (@intNumberOfMessages) 
	       * 
	  FROM (SELECT iT.YEAR, iT.MONTH, iT.DAY, iT.HOUR, q.Quarter AS MINUTE, AVG(iT.Value) as Average
			  FROM (SELECT datepart(year, [TimeStamp]) as YEAR
						  ,datepart(month, [TimeStamp]) as MONTH
						  ,datepart(day, [TimeStamp]) as DAY
						  ,datepart(hour, [TimeStamp]) AS HOUR
						  ,datepart(minute, [TimeStamp]) as MINUTE
						  ,CAST([Content] AS FLOAT) AS VALUE 
					  FROM [dbo].[TB_Message]
					 WHERE [SendingDevice] = @strDeviceName
					   AND [Name] = @strObjectName
			           AND [Parameter] = @strParameterName
					   AND [TimeStamp] > @lowestTimeStamp) iT
			  JOIN @quarters q ON q.Minute = iT.MINUTE
			 WHERE Value > @average - @standardDeviation 
			   AND Value < @average + @standardDeviation 
			 GROUP BY iT.YEAR, iT.MONTH, iT.DAY, iT.HOUR, q.Quarter) eT
	 ORDER BY eT.YEAR DESC, eT.MONTH DESC, eT.DAY DESC, eT.HOUR DESC, eT.MINUTE DESC;
	
END