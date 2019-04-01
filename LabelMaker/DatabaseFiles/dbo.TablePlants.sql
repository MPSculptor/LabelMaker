CREATE TABLE [dbo].[TablePlants] (
    [Id]           INT            IDENTITY (1, 1) NOT NULL,
    [GenusCross]   VARCHAR (1)    NULL,
    [Genus]        VARCHAR (50)   NULL,
    [SpeciesCross] VARCHAR (1)    NULL,
    [Species]      VARCHAR (50)   NULL,
    [Variety]      VARCHAR (100)   NULL,
    [Common]       VARCHAR (100)   NULL,
    [SKU]         VARchar (20) NULL,
    [Desc]         VARCHAR (500)  NULL,
    [PotSize]      VARCHAR (10)   NULL,
    [ColourQueue]  BIT            DEFAULT ((1)) NOT NULL,
    [Barcode]      CHAR (13)      NULL,
    [Picture1]     VARCHAR (255)  NULL,
    [Picture2]     VARCHAR (255)  NULL,
    [Picture3]     VARCHAR (255)  NULL,
    [Picture4]     VARCHAR (255)  NULL,
    [AGM]          BIT            DEFAULT ((0)) NOT NULL,
    [LabelColour]  VARCHAR (50)   DEFAULT ('Default') NOT NULL,
    [Hide]         BIT            DEFAULT ((0)) NOT NULL,
    [notes]        VARCHAR (255)  NULL,
    [LabelStock]   BIT            DEFAULT ((0)) NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'for + or x only', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'TablePlants', @level2type = N'COLUMN', @level2name = N'GenusCross';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'for x only', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'TablePlants', @level2type = N'COLUMN', @level2name = N'SpeciesCross';

