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
        public virtual DbSet<GachaBanner> GachaBanners { get; set; }
        public virtual DbSet<GachaItem> GachaItems { get; set; }
        public virtual DbSet<GachaHistory> GachaHistories { get; set; }
        public virtual DbSet<ShopItem> ShopItems { get; set; }
        public virtual DbSet<ShopPurchase> ShopPurchases { get; set; }
        public virtual DbSet<NPC> NPCs { get; set; }

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
            modelBuilder.Entity<NPC>().HasIndex(n => n.Name).IsUnique();

            //Primary keys
            modelBuilder.Entity<Item>().HasKey(i => i.Id);
            modelBuilder.Entity<Transaction>().HasKey(t => t.Id);
            modelBuilder.Entity<UserItem>().HasKey(ui => new { ui.UserId, ui.ItemId });
            modelBuilder.Entity<GameNews>().HasKey(gn => gn.Id);
            modelBuilder.Entity<GachaBanner>().HasKey(gb => gb.Id);
            modelBuilder.Entity<GachaItem>().HasKey(gi => gi.Id);
            modelBuilder.Entity<GachaHistory>().HasKey(gh => gh.Id);
            modelBuilder.Entity<ShopItem>().HasKey(si => si.Id);
            modelBuilder.Entity<ShopPurchase>().HasKey(sp => sp.Id);
            modelBuilder.Entity<NPC>().HasKey(n => n.Id);

            //Table names
            modelBuilder.Entity<GachaBanner>().ToTable("GachaBanner");
            modelBuilder.Entity<GachaItem>().ToTable("GachaItem");
            modelBuilder.Entity<GachaHistory>().ToTable("GachaHistory");

            //Relationships
            //User - UserItem
            modelBuilder.Entity<UserItem>()
                .HasOne(ui => ui.User)
                .WithMany(u => u.UserItems)
                .HasForeignKey(ui => ui.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            //User - Transaction
            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.User)
                .WithMany(u => u.Transactions)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            //Item - UserItem
            modelBuilder.Entity<UserItem>()
                .HasOne(ui => ui.Item)
                .WithMany(i => i.UserItems)
                .HasForeignKey(ui => ui.ItemId)
                .OnDelete(DeleteBehavior.Cascade);

            // GachaBanner - GachaItem
            modelBuilder.Entity<GachaItem>()
                .HasOne(gi => gi.GachaBanner)
                .WithMany(gb => gb.GachaItems)
                .HasForeignKey(gi => gi.GachaBannerId)
                .OnDelete(DeleteBehavior.Cascade);

            // Item - GachaItem (chỉ 1 relationship)
            modelBuilder.Entity<GachaItem>()
                .HasOne(gi => gi.Item)
                .WithMany()
                .HasForeignKey(gi => gi.ItemId)
                .OnDelete(DeleteBehavior.Restrict);

            // User - GachaHistory
            modelBuilder.Entity<GachaHistory>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(gh => gh.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // GachaBanner - GachaHistory
            modelBuilder.Entity<GachaHistory>()
                .HasOne(gh => gh.GachaBanner)
                .WithMany()
                .HasForeignKey(gh => gh.GachaBannerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Item - GachaHistory
            modelBuilder.Entity<GachaHistory>()
                .HasOne(gh => gh.Item)
                .WithMany()
                .HasForeignKey(gh => gh.ItemId)
                .OnDelete(DeleteBehavior.Restrict);

            //User - ShopPurchase
            modelBuilder.Entity<ShopPurchase>()
                .HasOne(sp => sp.User)
                .WithMany(u => u.ShopPurchases)
                .HasForeignKey(sp => sp.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            //ShopItem - ShopPurchase
            modelBuilder.Entity<ShopPurchase>()
                .HasOne(sp => sp.ShopItem)
                .WithMany(si => si.ShopPurchases)
                .HasForeignKey(sp => sp.ShopItemId)
                .OnDelete(DeleteBehavior.Cascade);

            //Item - ShopItem
            modelBuilder.Entity<ShopItem>()
                .HasOne(si => si.Item)
                .WithMany()
                .HasForeignKey(si => si.ItemId)
                .OnDelete(DeleteBehavior.SetNull);

           

            modelBuilder.Entity<User>()
                .Property(u => u.CurrencyAmount)
                .HasColumnType("decimal(18,2)");

            //Column types for ShopItem and ShopPurchase
            modelBuilder.Entity<ShopItem>()
                .Property(si => si.Price)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<ShopItem>()
                .Property(si => si.CurrencyAmount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<ShopPurchase>()
                .Property(sp => sp.AmountPaid)
                .HasColumnType("decimal(18,2)");
        }
    }
}
