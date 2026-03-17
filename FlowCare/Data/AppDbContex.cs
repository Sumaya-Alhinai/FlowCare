using Microsoft.EntityFrameworkCore;
using FlowCare.Models;

namespace FlowCare.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        
        // DbSets

        public DbSet<Branch> Branches { get; set; }
        public DbSet<ServiceType> ServiceTypes { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Staff> Staff { get; set; }
        public DbSet<Slot> Slots { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<StaffServiceType> StaffServiceTypes { get; set; }
        public DbSet<Config> Configs { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // USER

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.ToTable("users");

                entity.HasIndex(u => u.Email).IsUnique();

                entity.Property(u => u.Role)
                      .HasConversion<string>();

                entity.Property(u => u.Username)
                      .IsRequired()
                      .HasMaxLength(100);
            });

            // BRANCH

            modelBuilder.Entity<Branch>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.HasMany(b => b.Staff)
                      .WithOne(s => s.Branch)
                      .HasForeignKey(s => s.BranchId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(b => b.ServiceTypes)
                      .WithOne(st => st.Branch)
                      .HasForeignKey(st => st.BranchId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // SLOT

            modelBuilder.Entity<Slot>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(s => s.IsDeleted)
                      .HasDefaultValue(false);

                entity.Property(s => s.DeletedAt)
                      .IsRequired(false);

                entity.HasQueryFilter(s => !s.IsDeleted);

                entity.HasIndex(s => new { s.BranchId, s.ServiceTypeId, s.StartTime });

                entity.HasOne(s => s.Branch)
                      .WithMany(b => b.Slots)
                      .HasForeignKey(s => s.BranchId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(s => s.ServiceType)
                      .WithMany(st => st.Slots)
                      .HasForeignKey(s => s.ServiceTypeId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // APPOINTMENT

            modelBuilder.Entity<Appointment>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.HasIndex(a => a.SlotId).IsUnique();

                entity.HasOne(a => a.Customer)
                      .WithMany(c => c.Appointments)
                      .HasForeignKey(a => a.CustomerId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.Staff)
                      .WithMany(s => s.Appointments)
                      .HasForeignKey(a => a.StaffId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.Slot)
                      .WithOne(s => s.Appointment)
                      .HasForeignKey<Appointment>(a => a.SlotId)
                      .IsRequired(false)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            
            // SERVICE TYPE

            modelBuilder.Entity<ServiceType>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.HasIndex(st => new { st.BranchId, st.Name });
            });

            // CUSTOMER

            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.HasIndex(c => c.Email).IsUnique();
            });

            // STAFF

            modelBuilder.Entity<Staff>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.HasOne(s => s.Branch)
                      .WithMany(b => b.Staff)
                      .HasForeignKey(s => s.BranchId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // AUDIT LOG
            

            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.ToTable("audit_logs");

                entity.Property(a => a.CreatedAt)
                      .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne(a => a.User)
                      .WithMany()
                      .HasForeignKey(a => a.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(a => a.CreatedAt);
            });

            // STAFF SERVICE TYPE (MANY TO MANY)

            modelBuilder.Entity<StaffServiceType>(entity =>
            {
                entity.HasKey(x => new { x.StaffId, x.ServiceTypeId });

                entity.HasOne(x => x.Staff)
                      .WithMany(s => s.StaffServiceTypes)
                      .HasForeignKey(x => x.StaffId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.ServiceType)
                      .WithMany()
                      .HasForeignKey(x => x.ServiceTypeId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}