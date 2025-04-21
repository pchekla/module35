-- Проверка существования и добавление колонки Image
IF COL_LENGTH('dbo.AspNetUsers', 'Image') IS NULL
BEGIN
    ALTER TABLE dbo.AspNetUsers ADD Image NVARCHAR(MAX) NULL;
    PRINT 'Колонка Image добавлена';
END
ELSE
BEGIN
    PRINT 'Колонка Image уже существует';
END

-- Проверка существования и добавление колонки Status
IF COL_LENGTH('dbo.AspNetUsers', 'Status') IS NULL
BEGIN
    ALTER TABLE dbo.AspNetUsers ADD Status NVARCHAR(MAX) NULL;
    PRINT 'Колонка Status добавлена';
END
ELSE
BEGIN
    PRINT 'Колонка Status уже существует';
END

-- Проверка существования и добавление колонки About
IF COL_LENGTH('dbo.AspNetUsers', 'About') IS NULL
BEGIN
    ALTER TABLE dbo.AspNetUsers ADD About NVARCHAR(MAX) NULL;
    PRINT 'Колонка About добавлена';
END
ELSE
BEGIN
    PRINT 'Колонка About уже существует';
END

-- Обновление существующих записей со значениями по умолчанию
UPDATE dbo.AspNetUsers
SET Image = 'https://via.placeholder.com/500',
    Status = N'Ура! Я в соцсети!',
    About = N'Информация обо мне.'
WHERE Image IS NULL OR Status IS NULL OR About IS NULL; 