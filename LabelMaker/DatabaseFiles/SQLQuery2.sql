BULK INSERT [dbo].[LabelsLabelNames]
    FROM 'D:\LabelMaker\LabelMaker\Creation Files\LabelsNames.csv'
    WITH
    (
    FIELDTERMINATOR = ',',  --CSV field delimiter
    ROWTERMINATOR = '\n',   --Use to shift the control to next row
    ERRORFILE = 'D:\LabelMaker\LabelMaker\Creation Files\NError.csv',
    TABLOCK
    )