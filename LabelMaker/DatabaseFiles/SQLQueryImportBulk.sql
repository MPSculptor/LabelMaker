BULK INSERT TablePlants
    FROM 'D:\LabelMaker\LabelMaker\Creation Files\ErrorRows.csv'
    WITH
    (
    FIELDTERMINATOR = ',',  --CSV field delimiter
    ROWTERMINATOR = '\n',   --Use to shift the control to next row
    ERRORFILE = 'D:\LabelMaker\LabelMaker\Creation Files\ErrorRows2.csv',
    TABLOCK
    )