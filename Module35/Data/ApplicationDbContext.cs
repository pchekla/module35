using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Module35.Models;

namespace Module35.Data;

public class ApplicationDbContext : IdentityDbContext<User>
{
    public DbSet<FriendRelation> FriendRelations { get; set; }
    
    // Добавляем DbSet для UserDto для работы с FromSqlRaw
    public DbSet<UserDto> UserDtos { get; set; }
    
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
        Database.EnsureCreated();
        CreateFriendRelationsTable();
    }
    
    // Метод для создания таблицы FriendRelations
    private void CreateFriendRelationsTable()
    {
        try
        {
            // Проверяем существование таблицы путем запроса
            var testQuery = FriendRelations.FirstOrDefault();
        }
        catch
        {
            // Если таблица не существует, создаем ее с помощью SQL
            Database.ExecuteSqlRaw(@"
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
                    
                    CREATE INDEX [IX_FriendRelations_UserId] ON [dbo].[FriendRelations] ([UserId] ASC);
                    CREATE INDEX [IX_FriendRelations_FriendId] ON [dbo].[FriendRelations] ([FriendId] ASC);
                END
            ");
        }
    }
    
    // Дополнительная конфигурация модели
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        // Настраиваем отношения между пользователями и запросами в друзья
        builder.Entity<FriendRelation>()
            .HasOne(fr => fr.User)
            .WithMany(u => u.SentFriendRequests)
            .HasForeignKey(fr => fr.UserId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.Entity<FriendRelation>()
            .HasOne(fr => fr.Friend)
            .WithMany(u => u.ReceivedFriendRequests)
            .HasForeignKey(fr => fr.FriendId)
            .OnDelete(DeleteBehavior.Restrict);
            
        // Настройка UserDto как сущности без ключа, поскольку она используется только для запросов
        builder.Entity<UserDto>().HasNoKey();
    }
} 