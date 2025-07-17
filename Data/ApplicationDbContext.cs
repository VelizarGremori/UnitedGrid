namespace UnitedGrid.Data;

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UnitedGrid.Models.Chat;
using UnitedGrid.Models.Chat.Groups;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    //TODO Разделить контексты и написать сервисы поверх запросов в базу из контроллеров, но не сегодня..

    public DbSet<Message> Messages { get; set; }

    public DbSet<Group> Groups { get; set; }

    public DbSet<GroupUser> GroupUsers { get; set; }

    public DbSet<GroupMessage> GroupMessages { get; set; }

    public DbSet<MessageRead> MessageReads { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<BaseMessage>()
           .HasOne(m => m.Sender)
           .WithMany()
           .HasForeignKey(m => m.SenderId)
           .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<BaseMessage>().UseTptMappingStrategy();

        builder.Entity<Message>()
            .HasOne(m => m.Receiver)
            .WithMany()
            .HasForeignKey(m => m.ReceiverId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<GroupUser>()
            .HasKey(gu => new { gu.GroupId, gu.UserId });

        builder.Entity<GroupUser>()
            .HasOne(gu => gu.Group)
            .WithMany(g => g.Members)
            .HasForeignKey(gu => gu.GroupId);

        builder.Entity<GroupUser>()
            .HasOne(gu => gu.User)
            .WithMany()
            .HasForeignKey(gu => gu.UserId);


        builder.Entity<MessageRead>()
            .HasKey(r => new { r.GroupMessageId, r.UserId });

        builder.Entity<MessageRead>()
            .HasOne(r => r.Message)
            .WithMany(m => m.ReadBy)
            .HasForeignKey(r => r.GroupMessageId);

        builder.Entity<MessageRead>()
            .HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId);
    }
}