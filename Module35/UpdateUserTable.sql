-- Проверка существования колонок и их добавление, если их нет
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('AspNetUsers') AND name = 'Image')
BEGIN
    ALTER TABLE AspNetUsers ADD Image NVARCHAR(MAX) NULL;
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('AspNetUsers') AND name = 'Status')
BEGIN
    ALTER TABLE AspNetUsers ADD Status NVARCHAR(MAX) NULL;
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('AspNetUsers') AND name = 'About')
BEGIN
    ALTER TABLE AspNetUsers ADD About NVARCHAR(MAX) NULL;
END

-- Обновление существующих записей значениями по умолчанию
UPDATE [dbo].[AspNetUsers]
SET 
    [Image] = 'https://via.placeholder.com/500',
    [Status] = 'Ура! Я в соцсети!',
    [About] = 'Информация обо мне.'
WHERE [Image] IS NULL OR [Status] IS NULL OR [About] IS NULL; 