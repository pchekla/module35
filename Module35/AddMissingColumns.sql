-- Скрипт для добавления необходимых колонок в таблицу AspNetUsers

-- Проверяем, существует ли колонка Image
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AspNetUsers]') AND name = 'Image')
BEGIN
    ALTER TABLE [dbo].[AspNetUsers] ADD [Image] NVARCHAR(MAX) NULL;
    PRINT 'Колонка Image добавлена';
END
ELSE
BEGIN
    PRINT 'Колонка Image уже существует';
END

-- Проверяем, существует ли колонка Status
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AspNetUsers]') AND name = 'Status')
BEGIN
    ALTER TABLE [dbo].[AspNetUsers] ADD [Status] NVARCHAR(MAX) NULL;
    PRINT 'Колонка Status добавлена';
END
ELSE
BEGIN
    PRINT 'Колонка Status уже существует';
END

-- Проверяем, существует ли колонка About
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AspNetUsers]') AND name = 'About')
BEGIN
    ALTER TABLE [dbo].[AspNetUsers] ADD [About] NVARCHAR(MAX) NULL;
    PRINT 'Колонка About добавлена';
END
ELSE
BEGIN
    PRINT 'Колонка About уже существует';
END 