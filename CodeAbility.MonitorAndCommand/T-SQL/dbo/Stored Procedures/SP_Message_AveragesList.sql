
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[SP_Message_AveragesList]
	@intChartSpan int = 1
   ,@strDeviceName varchar(50) = null
   ,@strObjectName varchar(50) = null
   ,@strParameterName varchar(50) = null
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @intLast48Hours int = 1;
	DECLARE @intLast7Days int = 2;
	DECLARE @intLast30Days int = 3;
	DECLARE @intLast3Monthes int = 4;
	DECLARE @intLastYear int = 5;

	-- Compute lowestTimeStamp
	DECLARE @lowestTimeStamp date;

	IF @intChartSpan = @intLast48Hours
	BEGIN
		SET @lowestTimeStamp = DATEADD(hour, -48, GetDate());
	END
	ELSE IF @intChartSpan = @intLast7Days
	BEGIN
	    SET @lowestTimeStamp = DATEADD(hour, -148, GetDate());		
	END
	ELSE IF @intChartSpan = @intLast30Days
	BEGIN
		SET @lowestTimeStamp = DATEADD(day, -30, GetDate());
	END 
	ELSE IF @intChartSpan = @intLast3Monthes
	BEGIN
		SET @lowestTimeStamp = DATEADD(day, -90, GetDate());
	END 
	ELSE IF @intChartSpan = @intLastYear
	BEGIN
		SET @lowestTimeStamp = DATEADD(day, -365, GetDate());
	END 

	-- Compute average and standard deviation on the selected span
	DECLARE @average FLOAT = 0;
	DECLARE @standardDeviation FLOAT = 0;

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

	IF @intChartSpan = @intLast48Hours 
	BEGIN 

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

		SELECT * 
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
				 WHERE Value > @average - 2.5 * @standardDeviation 
				   AND Value < @average + 2.5 * @standardDeviation 
				 GROUP BY iT.YEAR, iT.MONTH, iT.DAY, iT.HOUR, q.Quarter) eT
		 ORDER BY eT.YEAR DESC, eT.MONTH DESC, eT.DAY DESC, eT.HOUR DESC, eT.MINUTE DESC;
	
	END 
	IF @intChartSpan = @intLast7Days
	BEGIN 

		SELECT * 
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
				 WHERE Value > @average - 2.5 * @standardDeviation 
				   AND Value < @average + 2.5 * @standardDeviation 
				 GROUP BY iT.YEAR, iT.MONTH, iT.DAY, iT.HOUR) eT
		 ORDER BY eT.YEAR, eT.MONTH, eT.DAY, eT.HOUR;

	END 
	ELSE IF @intChartSpan = @intLast30Days
	BEGIN 

		DECLARE @hours TABLE (Hour int, Time int);

		INSERT INTO @hours (Hour, Time) VALUES (0, 0);
		INSERT INTO @hours (Hour, Time) VALUES (1, 0);

		INSERT INTO @hours (Hour, Time) VALUES (2, 4);
		INSERT INTO @hours (Hour, Time) VALUES (3, 4);
		INSERT INTO @hours (Hour, Time) VALUES (4, 4);
		INSERT INTO @hours (Hour, Time) VALUES (5, 4);

		INSERT INTO @hours (Hour, Time) VALUES (6, 8);
		INSERT INTO @hours (Hour, Time) VALUES (7, 8);
		INSERT INTO @hours (Hour, Time) VALUES (8, 8);
		INSERT INTO @hours (Hour, Time) VALUES (9, 8);

		INSERT INTO @hours (Hour, Time) VALUES (10, 12);
		INSERT INTO @hours (Hour, Time) VALUES (11, 12);
		INSERT INTO @hours (Hour, Time) VALUES (12, 12);
		INSERT INTO @hours (Hour, Time) VALUES (13, 12);

		INSERT INTO @hours (Hour, Time) VALUES (14, 16);
		INSERT INTO @hours (Hour, Time) VALUES (15, 16);
		INSERT INTO @hours (Hour, Time) VALUES (16, 16);
		INSERT INTO @hours (Hour, Time) VALUES (17, 16);

		INSERT INTO @hours (Hour, Time) VALUES (18, 20);
		INSERT INTO @hours (Hour, Time) VALUES (19, 20);
		INSERT INTO @hours (Hour, Time) VALUES (20, 20);
		INSERT INTO @hours (Hour, Time) VALUES (21, 20);

		INSERT INTO @hours (Hour, Time) VALUES (22, 0);
		INSERT INTO @hours (Hour, Time) VALUES (23, 0);

		SELECT * 
		  FROM (SELECT iT.YEAR, iT.MONTH, iT.DAY, q.Time as HOUR, 0 as MINUTE, AVG(iT.Value) as Average
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
				  JOIN @hours q ON q.Hour = iT.HOUR
				 WHERE Value > @average - 2.5 * @standardDeviation 
				   AND Value < @average + 2.5 * @standardDeviation 
				 GROUP BY iT.YEAR, iT.MONTH, iT.DAY, q.Time) eT
		 ORDER BY eT.YEAR DESC, eT.MONTH DESC, eT.DAY DESC, eT.HOUR DESC;

	END 
	ELSE IF @intChartSpan = @intLast3Monthes
	BEGIN 

		DECLARE @halfDays TABLE (Hour int, Time int);

		INSERT INTO @halfDays (Hour, Time) VALUES (0, 0);
		INSERT INTO @halfDays (Hour, Time) VALUES (1, 0);
		INSERT INTO @halfDays (Hour, Time) VALUES (2, 0);
		INSERT INTO @halfDays (Hour, Time) VALUES (3, 0);
		INSERT INTO @halfDays (Hour, Time) VALUES (4, 0);
		INSERT INTO @halfDays (Hour, Time) VALUES (5, 0);
		INSERT INTO @halfDays (Hour, Time) VALUES (6, 0);
		INSERT INTO @halfDays (Hour, Time) VALUES (7, 0);
		INSERT INTO @halfDays (Hour, Time) VALUES (8, 12);
		INSERT INTO @halfDays (Hour, Time) VALUES (9, 12);
		INSERT INTO @halfDays (Hour, Time) VALUES (10, 12);
		INSERT INTO @halfDays (Hour, Time) VALUES (11, 12);
		INSERT INTO @halfDays (Hour, Time) VALUES (12, 12);
		INSERT INTO @halfDays (Hour, Time) VALUES (13, 12);
		INSERT INTO @halfDays (Hour, Time) VALUES (14, 12);
		INSERT INTO @halfDays (Hour, Time) VALUES (15, 12);
		INSERT INTO @halfDays (Hour, Time) VALUES (16, 12);
		INSERT INTO @halfDays (Hour, Time) VALUES (17, 12);
		INSERT INTO @halfDays (Hour, Time) VALUES (18, 12);
		INSERT INTO @halfDays (Hour, Time) VALUES (19, 12);
		INSERT INTO @halfDays (Hour, Time) VALUES (20, 0);
		INSERT INTO @halfDays (Hour, Time) VALUES (21, 0);
		INSERT INTO @halfDays (Hour, Time) VALUES (22, 0);
		INSERT INTO @halfDays (Hour, Time) VALUES (23, 0);

		SELECT * 
		  FROM (SELECT iT.YEAR, iT.MONTH, iT.DAY, q.Time as HOUR, 0 as MINUTE, AVG(iT.Value) as Average
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
				  JOIN @halfDays q ON q.Hour = iT.HOUR
				 WHERE Value > @average - 2.5 * @standardDeviation 
				   AND Value < @average + 2.5 * @standardDeviation 
				 GROUP BY iT.YEAR, iT.MONTH, iT.DAY, q.Time) eT
		 ORDER BY eT.YEAR DESC, eT.MONTH DESC, eT.DAY DESC, eT.HOUR DESC;

	END
	ELSE IF @intChartSpan = @intLastYear
	BEGIN 

		SELECT * 
		  FROM (SELECT iT.YEAR, iT.MONTH, iT.DAY, 0 as HOUR, 0 as MINUTE, AVG(iT.Value) as Average
				  FROM (SELECT datepart(year, [TimeStamp]) as YEAR
							  ,datepart(month, [TimeStamp]) as MONTH
							  ,datepart(day, [TimeStamp]) as DAY
							  ,CAST([Content] AS FLOAT) AS VALUE 
						  FROM [dbo].[TB_Message]
						 WHERE [SendingDevice] = @strDeviceName
						   AND [Name] = @strObjectName
						   AND [Parameter] = @strParameterName
						   AND [TimeStamp] > @lowestTimeStamp) iT
				 WHERE Value > @average - 2.5 * @standardDeviation 
				   AND Value < @average + 2.5 * @standardDeviation 
				 GROUP BY iT.YEAR, iT.MONTH, iT.DAY) eT
		 ORDER BY eT.YEAR DESC, eT.MONTH DESC, eT.DAY DESC;

	END
END