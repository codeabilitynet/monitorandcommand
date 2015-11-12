CREATE TABLE [dbo].[TB_Event] (
    [Id]         BIGINT        IDENTITY (1, 1) NOT NULL,
    [FromDevice] VARCHAR (50)  NULL,
    [Content]    VARCHAR (MAX) NULL,
    [TimeStamp]  DATETIME      CONSTRAINT [DF_TB_Event_TimeStamp] DEFAULT (getdate()) NULL
);

