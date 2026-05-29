using LegendPay.Models.Data.Tables;
using Microsoft.EntityFrameworkCore;

namespace LegendPay.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<UserAccount> UserAccounts { get; set; }
        public DbSet<AdminAccount> AdminAccounts { get; set; }

        public DbSet<Wallet> Wallets { get; set; }
        public DbSet<WalletTransaction> WalletTransactions { get; set; }

        public DbSet<Bill> Bills { get; set; }
        public DbSet<Receipt> Receipts { get; set; }

        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<Reminder> Reminders { get; set; }

        public DbSet<ScheduledPayment> ScheduledPayments { get; set; }

        public DbSet<Beneficiary> Beneficiaries { get; set; }

        public DbSet<LegendPoint> LegendPoints { get; set; }
        public DbSet<LegendPointTransaction> LegendPointTransactions { get; set; }
        public DbSet<LegendPointSettings> LegendPointSettings { get; set; }
        public DbSet<FloatAccount> FloatAccounts { get; set; }
        public DbSet<FloatTransaction> FloatTransactions { get; set; }

        public DbSet<Notification> Notifications { get; set; }

        public DbSet<SupportChat> SupportChats { get; set; }
        public DbSet<SupportMessage> SupportMessages { get; set; }
        public DbSet<SpendingRecord> SpendingRecords { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Wallet>()
                .HasIndex(w => w.UserAccountId)
                .IsUnique();

            modelBuilder.Entity<LegendPoint>()
                .HasIndex(lp => lp.UserAccountId)
                .IsUnique();

            modelBuilder.Entity<AdminAccount>()
                .HasIndex(a => a.Email)
                .IsUnique();

            modelBuilder.Entity<SpendingRecord>()
                .HasIndex(s => new { s.UserAccountId, s.BillerCategory, s.Month, s.Year })
                .IsUnique();

            modelBuilder.Entity<FloatAccount>()
                .HasIndex(f => f.Id)
                .IsUnique();

            modelBuilder.Entity<LegendPointSettings>()
                .HasIndex(l => l.Id)
                .IsUnique();

            modelBuilder.Entity<Receipt>()
                .HasOne(r => r.UserAccount)
                .WithMany(u => u.Receipts)
                .HasForeignKey(r => r.UserAccountId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<SupportChat>()
                .HasOne(s => s.UserAccount)
                .WithMany(u => u.SupportChats)
                .HasForeignKey(s => s.UserAccountId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<LegendPointTransaction>()
                .HasOne(l => l.UserAccount)
                .WithMany()
                .HasForeignKey(l => l.UserAccountId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Reminder>()
                .HasOne(r => r.UserAccount)
                .WithMany()
                .HasForeignKey(r => r.UserAccountId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}