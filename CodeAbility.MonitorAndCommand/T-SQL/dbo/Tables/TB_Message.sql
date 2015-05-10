CREATE TABLE [dbo].[TB_Message] (
    [SendingDevice]   VARCHAR (50) NULL,
    [ReceivingDevice] VARCHAR (50) NULL,
    [FromDevice]      VARCHAR (50) NULL,
    [ToDevice]        VARCHAR (50) NULL,
    [ContentType]     VARCHAR (50) NULL,
    [Name]            VARCHAR (50) NULL,
    [Parameter]       VARCHAR (50) NULL,
    [Content]         VARCHAR (50) NULL,
    [TimeStamp]       DATETIME     NULL
);

