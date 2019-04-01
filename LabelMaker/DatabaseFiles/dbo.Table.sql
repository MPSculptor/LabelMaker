CREATE TABLE [dbo].[TablePlants]
(
	[Id] INT NOT NULL PRIMARY KEY, 
    [GenusCross] VARCHAR(1) NULL , 
    [Genus] VARCHAR(50) NULL, 
    [SpeciesCross] VARCHAR(1) NULL ,
	[Species] VARCHAR(50) NULL, 
    [Variety] VARCHAR(50) NULL,
	[Common] VARCHAR(50) NULL,
	[Code] VARBINARY(20) NULL,
	[Desc] VARCHAR(500) NULL,    
	[PotSize] VARCHAR(10) NULL, 
    [ColourQueue] BIT NOT NULL DEFAULT 1, 
    [Barcode] CHAR(13) NULL, 
    [Picture1] VARCHAR(255) NULL,
	[Picture2] VARCHAR(255) NULL,
	[Picture3] VARCHAR(255) NULL,
	[Picture4] VARCHAR(255) NULL,
 
     
     
    
)
