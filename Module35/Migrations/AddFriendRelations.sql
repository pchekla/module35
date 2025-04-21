-- Создание таблицы для хранения отношений между друзьями
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[FriendRelations]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[FriendRelations] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [UserId] NVARCHAR(450) NOT NULL,
        [FriendId] NVARCHAR(450) NOT NULL,
        [RequestDate] DATETIME2 NOT NULL,
        [IsAccepted] BIT NOT NULL,
        [AcceptedDate] DATETIME2 NULL,
        CONSTRAINT [PK_FriendRelations] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_FriendRelations_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]),
        CONSTRAINT [FK_FriendRelations_AspNetUsers_FriendId] FOREIGN KEY ([FriendId]) REFERENCES [dbo].[AspNetUsers] ([Id])
    );
    
    PRINT 'Таблица FriendRelations создана успешно.';
END
ELSE
BEGIN
    PRINT 'Таблица FriendRelations уже существует.';
END

-- Создание индексов для улучшения производительности
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_FriendRelations_UserId' AND object_id = OBJECT_ID(N'[dbo].[FriendRelations]'))
BEGIN
    CREATE INDEX [IX_FriendRelations_UserId] ON [dbo].[FriendRelations] ([UserId] ASC);
    PRINT 'Индекс IX_FriendRelations_UserId создан успешно.';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_FriendRelations_FriendId' AND object_id = OBJECT_ID(N'[dbo].[FriendRelations]'))
BEGIN
    CREATE INDEX [IX_FriendRelations_FriendId] ON [dbo].[FriendRelations] ([FriendId] ASC);
    PRINT 'Индекс IX_FriendRelations_FriendId создан успешно.';
END

-- Создание уникального индекса для предотвращения дублирования отношений
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_FriendRelations_UserId_FriendId' AND object_id = OBJECT_ID(N'[dbo].[FriendRelations]'))
BEGIN
    CREATE UNIQUE INDEX [IX_FriendRelations_UserId_FriendId] ON [dbo].[FriendRelations] ([UserId] ASC, [FriendId] ASC);
    PRINT 'Уникальный индекс IX_FriendRelations_UserId_FriendId создан успешно.';
END 