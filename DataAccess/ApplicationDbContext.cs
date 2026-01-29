using BussinessObjects.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DataAccess
{
    public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public virtual DbSet<Item> Items { get; set; }
        public virtual DbSet<UserItem> UserItems { get; set; }
        public virtual DbSet<GameNews> GameNews { get; set; }
        public virtual DbSet<Transaction> Transactions { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.EnableSensitiveDataLogging();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //Unique Constrains
            modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
            modelBuilder.Entity<User>().HasIndex(u => u.UserName).IsUnique();
            modelBuilder.Entity<Item>().HasIndex(i => i.Name).IsUnique();
            modelBuilder.Entity<Transaction>().HasIndex(t => t.OrderCode).IsUnique();
            modelBuilder.Entity<GameNews>().HasIndex(gn => gn.Title).IsUnique();

            //Primary keys
            modelBuilder.Entity<Item>().HasKey(i => i.Id);
            modelBuilder.Entity<Transaction>().HasKey(t => t.Id);
            modelBuilder.Entity<UserItem>().HasKey(ui => new { ui.UserId, ui.ItemId });
            modelBuilder.Entity<GameNews>().HasKey(gn => gn.Id);

            //Relationship
            //User - UserItem
            modelBuilder.Entity<UserItem>()
                .HasOne(ui => ui.User)
                .WithMany(u => u.UserItems)
                .HasForeignKey(ui => ui.UserId);
            
            //User - Transaction
            modelBuilder.Entity<Transaction>()
                .HasOne(ui => ui.User)
                .WithMany(t => t.Transactions)
                .HasForeignKey(t => t.UserId);

            //Item - UserItem
            modelBuilder.Entity<UserItem>()
                .HasOne(ui => ui.Item)
                .WithMany(i => i.UserItems)
                .HasForeignKey(ui => ui.ItemId);
        }
    }
}
