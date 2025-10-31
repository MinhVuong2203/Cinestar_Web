using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Web.Models;

namespace Web.Data;

public partial class CineStarContext : DbContext
{
    public CineStarContext(DbContextOptions<CineStarContext> options)
        : base(options)
    {
    }

    public virtual DbSet<CinemaBranch> CinemaBranches { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<EmployeeChange> EmployeeChanges { get; set; }

    public virtual DbSet<Invoice> Invoices { get; set; }

    public virtual DbSet<InvoiceProduct> InvoiceProducts { get; set; }

    public virtual DbSet<InvoiceTicket> InvoiceTickets { get; set; }

    public virtual DbSet<Language> Languages { get; set; }

    public virtual DbSet<Movie> Movies { get; set; }

    public virtual DbSet<MovieProduct> MovieProducts { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<Room> Rooms { get; set; }

    public virtual DbSet<Seat> Seats { get; set; }

    public virtual DbSet<ShowTime> ShowTimes { get; set; }

    public virtual DbSet<TextTranslation> TextTranslations { get; set; }

    public virtual DbSet<Ticket> Tickets { get; set; }

    public virtual DbSet<WorkShift> WorkShifts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CinemaBranch>(entity =>
        {
            entity.HasKey(e => e.BranchID).HasName("PK__CinemaBr__A1682FA5D69D1A8A");

            entity.ToTable("CinemaBranch", tb => tb.HasTrigger("trg_CinemaBranch_Insert"));

            entity.Property(e => e.CloseHour).HasDefaultValue(new TimeOnly(23, 0, 0));
            entity.Property(e => e.OpenHour).HasDefaultValue(new TimeOnly(8, 0, 0));
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.CustomerID).HasName("PK__Customer__A4AE64B891174FA8");

            entity.Property(e => e.CustomerID).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Point).HasDefaultValue(0m);
            entity.Property(e => e.RegisterDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.VipLevel).HasDefaultValue(0);
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.EmployeeID).HasName("PK__Employee__7AD04FF1763C5341");

            entity.Property(e => e.EmployeeID).HasDefaultValueSql("(newid())");
            entity.Property(e => e.RegisterDate).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Branch).WithMany(p => p.Employees)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK__Employee__Branch__51300E55");
        });

        modelBuilder.Entity<EmployeeChange>(entity =>
        {
            entity.HasKey(e => e.ChangeID).HasName("PK__Employee__0E05C5B7BFF1528D");

            entity.ToTable("EmployeeChange", tb => tb.HasTrigger("trg_EmployeeChange_Insert"));

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Employee).WithMany(p => p.EmployeeChanges).HasConstraintName("FK__EmployeeC__Emplo__607251E5");
        });

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(e => e.InvoiceID).HasName("PK__Invoice__D796AAD5C6FFD76C");

            entity.Property(e => e.InvoiceID).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Discount).HasDefaultValue(0m);
            entity.Property(e => e.IssueDate).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Branch).WithMany(p => p.Invoices)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK__Invoice__BranchI__2CBDA3B5");

            entity.HasOne(d => d.Customer).WithMany(p => p.Invoices)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK__Invoice__Custome__2BC97F7C");

            entity.HasOne(d => d.Employee).WithMany(p => p.Invoices)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK__Invoice__Employe__2AD55B43");
        });

        modelBuilder.Entity<InvoiceProduct>(entity =>
        {
            entity.HasKey(e => e.InvoiceProductID).HasName("PK__InvoiceP__D032D0A97534A82C");

            entity.ToTable("InvoiceProduct", tb => tb.HasTrigger("trg_InvoiceProduct_Insert"));

            entity.Property(e => e.Quantity).HasDefaultValue(1);

            entity.HasOne(d => d.Invoice).WithMany(p => p.InvoiceProducts).HasConstraintName("FK__InvoicePr__Invoi__3DE82FB7");

            entity.HasOne(d => d.Product).WithMany(p => p.InvoiceProducts).HasConstraintName("FK__InvoicePr__Produ__3EDC53F0");
        });

        modelBuilder.Entity<InvoiceTicket>(entity =>
        {
            entity.HasKey(e => e.InvoiceTicketID).HasName("PK__InvoiceT__137C3B0FDA6038B7");

            entity.ToTable("InvoiceTicket", tb => tb.HasTrigger("trg_InvoiceTicket_Insert"));

            entity.Property(e => e.Quantity).HasDefaultValue(1);

            entity.HasOne(d => d.Invoice).WithMany(p => p.InvoiceTickets).HasConstraintName("FK__InvoiceTi__Invoi__3552E9B6");

            entity.HasOne(d => d.Ticket).WithMany(p => p.InvoiceTickets).HasConstraintName("FK__InvoiceTi__Ticke__36470DEF");
        });

        modelBuilder.Entity<Language>(entity =>
        {
            entity.HasKey(e => e.LanguageCode).HasName("PK__Language__8B8C8A35C5F2354C");
        });

        modelBuilder.Entity<Movie>(entity =>
        {
            entity.HasKey(e => e.MovieID).HasName("PK__Movie__4BD2943ABFB7F6F4");

            entity.ToTable("Movie", tb => tb.HasTrigger("trg_Movie_Insert"));
        });

        modelBuilder.Entity<MovieProduct>(entity =>
        {
            entity.HasKey(e => e.MovieProductID).HasName("PK__MoviePro__9FE82F3210F82290");

            entity.ToTable("MovieProduct", tb => tb.HasTrigger("trg_MovieProduct_Insert"));

            entity.HasOne(d => d.Movie).WithMany(p => p.MovieProducts).HasConstraintName("FK__MovieProd__Movie__1E6F845E");

            entity.HasOne(d => d.Product).WithMany(p => p.MovieProducts).HasConstraintName("FK__MovieProd__Produ__1F63A897");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentID).HasName("PK__Payment__9B556A58F94120A7");

            entity.Property(e => e.PaymentID).HasDefaultValueSql("(newid())");
            entity.Property(e => e.PaymentTime).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Invoice).WithMany(p => p.Payments).HasConstraintName("FK__Payment__Invoice__467D75B8");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.ProductID).HasName("PK__Product__B40CC6EDDDAEE7DC");

            entity.ToTable("Product", tb => tb.HasTrigger("trg_Product_Insert"));
        });

        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasKey(e => e.RoomID).HasName("PK__Room__328639191381BE1D");

            entity.ToTable("Room", tb => tb.HasTrigger("trg_Room_Insert"));

            entity.HasOne(d => d.Branch).WithMany(p => p.Rooms)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK__Room__BranchID__7FEAFD3E");
        });

        modelBuilder.Entity<Seat>(entity =>
        {
            entity.HasKey(e => e.SeatID).HasName("PK__Seat__311713D31302E3FC");

            entity.ToTable("Seat", tb => tb.HasTrigger("trg_Seat_Insert"));

            entity.HasOne(d => d.Room).WithMany(p => p.Seats).HasConstraintName("FK__Seat__RoomID__05A3D694");
        });

        modelBuilder.Entity<ShowTime>(entity =>
        {
            entity.HasKey(e => e.ShowTimeID).HasName("PK__ShowTime__DF1BC9FF3F0FFA6B");

            entity.ToTable("ShowTime", tb => tb.HasTrigger("trg_ShowTime_Insert"));

            entity.HasOne(d => d.Movie).WithMany(p => p.ShowTimes).HasConstraintName("FK__ShowTime__MovieI__0B5CAFEA");

            entity.HasOne(d => d.Room).WithMany(p => p.ShowTimes).HasConstraintName("FK__ShowTime__RoomID__0C50D423");
        });

        modelBuilder.Entity<TextTranslation>(entity =>
        {
            entity.HasKey(e => new { e.TextKey, e.LanguageCode }).HasName("PK__TextTran__0ADD24C698E715FD");

            entity.HasOne(d => d.LanguageCodeNavigation).WithMany(p => p.TextTranslations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TextTrans__Langu__395884C4");
        });

        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.HasKey(e => e.TicketID).HasName("PK__Ticket__712CC627A77D0104");

            entity.Property(e => e.TicketID).HasDefaultValueSql("(newid())");

            entity.HasOne(d => d.Seat).WithMany(p => p.Tickets)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Ticket__SeatID__13F1F5EB");

            entity.HasOne(d => d.ShowTime).WithMany(p => p.Tickets)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Ticket__ShowTime__12FDD1B2");
        });

        modelBuilder.Entity<WorkShift>(entity =>
        {
            entity.HasKey(e => e.ShiftID).HasName("PK__WorkShif__C0A838E1C39041D2");

            entity.ToTable("WorkShift", tb => tb.HasTrigger("trg_WorkShift_Insert"));

            entity.HasOne(d => d.Branch).WithMany(p => p.WorkShifts)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK__WorkShift__Branc__69FBBC1F");

            entity.HasOne(d => d.Employee).WithMany(p => p.WorkShifts).HasConstraintName("FK__WorkShift__Emplo__690797E6");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
