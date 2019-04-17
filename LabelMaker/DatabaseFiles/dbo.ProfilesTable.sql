CREATE TABLE [dbo].[Table]
(
	[Id] INT NOT NULL PRIMARY KEY, 
    [Name] NCHAR(20) NOT NULL, 
    [BorderColour] INT NOT NULL, 
    [FontName] NCHAR(30) NOT NULL, 
    [Bold] BIT NOT NULL, 
    [Italic] BIT NOT NULL, 
    [FontColour] INT NOT NULL, 
    [BackgroundColour] NCHAR(10) NOT NULL
)
