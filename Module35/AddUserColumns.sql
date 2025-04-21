-- Скрипт для добавления колонок в таблицу AspNetUsers

-- Добавление колонки Image
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AspNetUsers') AND name = 'Image')
BEGIN
    ALTER TABLE AspNetUsers ADD Image NVARCHAR(MAX) NULL;
    PRINT 'Колонка Image добавлена';
END
ELSE
BEGIN
    PRINT 'Колонка Image уже существует';
END

-- Добавление колонки Status
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AspNetUsers') AND name = 'Status')
BEGIN
    ALTER TABLE AspNetUsers ADD Status NVARCHAR(MAX) NULL;
    PRINT 'Колонка Status добавлена';
END
ELSE
BEGIN
    PRINT 'Колонка Status уже существует';
END

-- Добавление колонки About
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AspNetUsers') AND name = 'About')
BEGIN
    ALTER TABLE AspNetUsers ADD About NVARCHAR(MAX) NULL;
    PRINT 'Колонка About добавлена';
END
ELSE
BEGIN
    PRINT 'Колонка About уже существует';
END

-- Обновление существующих записей значениями по умолчанию
UPDATE AspNetUsers
SET 
    Image = 'https://via.placeholder.com/500',
    Status = N'Ура! Я в соцсети!',
    About = N'Информация обо мне.'
WHERE Image IS NULL OR Status IS NULL OR About IS NULL; 