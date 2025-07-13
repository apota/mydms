using DMS.FinancialManagement.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace DMS.FinancialManagement.Infrastructure.Data
{
    public class FinancialDbContext : DbContext
    {
        public FinancialDbContext(DbContextOptions<FinancialDbContext> options) : base(options)
        {
        }

        public DbSet<ChartOfAccount> ChartOfAccounts { get; set; } = null!;
        public DbSet<JournalEntry> JournalEntries { get; set; } = null!;
        public DbSet<JournalLineItem> JournalLineItems { get; set; } = null!;
        public DbSet<FinancialPeriod> FinancialPeriods { get; set; } = null!;
        public DbSet<Budget> Budgets { get; set; } = null!;
        public DbSet<BudgetLine> BudgetLines { get; set; } = null!;
        public DbSet<TaxCode> TaxCodes { get; set; } = null!;
        public DbSet<Invoice> Invoices { get; set; } = null!;
        public DbSet<Payment> Payments { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure ChartOfAccount entity
            modelBuilder.Entity<ChartOfAccount>(entity =>
            {
                entity.ToTable("chart_of_accounts");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.AccountCode).IsRequired().HasMaxLength(20);
                entity.Property(e => e.AccountName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.CreatedAt).IsRequired();

                // Self-referencing relationship
                entity.HasOne(e => e.ParentAccount)
                    .WithMany(e => e.ChildAccounts)
                    .HasForeignKey(e => e.ParentAccountId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired(false);
            });

            // Configure JournalEntry entity
            modelBuilder.Entity<JournalEntry>(entity =>
            {
                entity.ToTable("journal_entries");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.EntryNumber).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Reference).HasMaxLength(100);
                entity.Property(e => e.CreatedAt).IsRequired();

                // Relationship with FinancialPeriod
                entity.HasOne(e => e.FinancialPeriod)
                    .WithMany(p => p.JournalEntries)
                    .HasForeignKey(e => e.FinancialPeriodId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired(false);
            });

            // Configure JournalLineItem entity
            modelBuilder.Entity<JournalLineItem>(entity =>
            {
                entity.ToTable("journal_line_items");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.DebitAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.CreditAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.CreatedAt).IsRequired();

                // Relationship with JournalEntry
                entity.HasOne(e => e.JournalEntry)
                    .WithMany(j => j.LineItems)
                    .HasForeignKey(e => e.JournalEntryId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                // Relationship with ChartOfAccount
                entity.HasOne(e => e.Account)
                    .WithMany(a => a.JournalLineItems)
                    .HasForeignKey(e => e.AccountId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired();
            });

            // Configure FinancialPeriod entity
            modelBuilder.Entity<FinancialPeriod>(entity =>
            {
                entity.ToTable("financial_periods");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.ClosedBy).HasMaxLength(100);
                entity.Property(e => e.CreatedAt).IsRequired();
                
                // Add unique constraint for fiscal year and period number
                entity.HasIndex(e => new { e.FiscalYear, e.PeriodNumber }).IsUnique();
            });

            // Configure Budget entity
            modelBuilder.Entity<Budget>(entity =>
            {
                entity.ToTable("budgets");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.ApprovedBy).HasMaxLength(100);
                entity.Property(e => e.CreatedAt).IsRequired();
            });

            // Configure BudgetLine entity
            modelBuilder.Entity<BudgetLine>(entity =>
            {
                entity.ToTable("budget_lines");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.PlannedAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Notes).HasMaxLength(500);
                entity.Property(e => e.CreatedAt).IsRequired();

                // Relationship with Budget
                entity.HasOne(e => e.Budget)
                    .WithMany(b => b.BudgetLines)
                    .HasForeignKey(e => e.BudgetId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                // Relationship with ChartOfAccount
                entity.HasOne(e => e.Account)
                    .WithMany(a => a.BudgetLines)
                    .HasForeignKey(e => e.AccountId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired();

                // Relationship with FinancialPeriod
                entity.HasOne(e => e.FinancialPeriod)
                    .WithMany(p => p.BudgetLines)
                    .HasForeignKey(e => e.FinancialPeriodId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired();
            });

            // Configure TaxCode entity
            modelBuilder.Entity<TaxCode>(entity =>
            {
                entity.ToTable("tax_codes");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.Code).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Rate).HasColumnType("decimal(5,2)");
                entity.Property(e => e.CreatedAt).IsRequired();
            });            // Configure Invoice entity
            modelBuilder.Entity<Invoice>(entity =>
            {
                entity.ToTable("invoices");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.InvoiceNumber).IsRequired().HasMaxLength(20);
                entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.PaidAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TaxAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.PaymentTerms).HasMaxLength(100);
                entity.Property(e => e.Notes).HasMaxLength(500);
                entity.Property(e => e.CreatedAt).IsRequired();
                
                // Relationship with TaxCode
                entity.HasOne(e => e.TaxCode)
                    .WithMany(t => t.Invoices)
                    .HasForeignKey(e => e.TaxCodeId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired(false);
            });

            // Configure Payment entity
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.ToTable("payments");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.PaymentNumber).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Reference).HasMaxLength(100);
                entity.Property(e => e.ProcessedBy).HasMaxLength(100);
                entity.Property(e => e.CreatedAt).IsRequired();

                // Relationship with Invoice
                entity.HasOne(e => e.Invoice)
                    .WithMany(i => i.Payments)
                    .HasForeignKey(e => e.InvoiceId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired(false);
            });
        }
    }
}
