using IotDashboard.Domain.Entities;
using IotDashboard.Infrastructure.AuditServices;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IotDashboard.Infrastructure.Persistence
{
    public class AppDBContext : IdentityDbContext<User, Role, long,
        IdentityUserClaim<long>, IdentityUserRole<long>, IdentityUserLogin<long>,
        IdentityRoleClaim<long>, UserToken>
    {
        private readonly ICurrentUserService _currentUserService;

        public AppDBContext(DbContextOptions<AppDBContext> options, ICurrentUserService currentUserService)
            : base(options)
        {
            _currentUserService = currentUserService;
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable(name: "User");
                entity.Property(x => x.Modules)
                    .HasColumnType("bigint[]")
                    .HasDefaultValueSql("'{}'::bigint[]");
                entity.HasQueryFilter(x => x.CustomerId == _currentUserService.GetCustomerId());
                
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("Role").HasKey(x => x.Id);
            });
            modelBuilder.Entity<IdentityUserRole<long>>(entity =>
            {
                entity.ToTable("UserRoles").
                    //in case you chagned the TKey type
                    HasKey(key => new { key.UserId, key.RoleId });
            });

            modelBuilder.Entity<IdentityUserClaim<long>>(entity =>
            {
                entity.ToTable("UserClaims").HasKey(x => x.Id);

            });

            modelBuilder.Entity<IdentityUserLogin<long>>(entity =>
            {
                entity.ToTable("UserLogins").HasKey(key => new { key.ProviderKey, key.LoginProvider });
                //in case you chagned the TKey type
                //entity.HasKey(key => new { key.ProviderKey, key.LoginProvider });       
            });

            modelBuilder.Entity<IdentityRoleClaim<long>>(entity =>
            {
                entity.ToTable("RoleClaims").HasKey(x => x.Id);
            });

            modelBuilder.Entity<UserToken>(entity =>
            {
                entity.ToTable("UserTokens").HasKey(key => new { key.UserId, key.LoginProvider, key.Name });
                //in case you chagned the TKey type
                // entity.HasKey(key => new { key.UserId, key.LoginProvider, key.Name });

            });

            modelBuilder.Entity<Customer>(entity =>
            {
                entity.ToTable("Customers").HasKey(x => x.Id);
                entity.HasIndex(x => x.Slug).IsUnique();
            });

            modelBuilder.Entity<Subscription>(entity =>
            {
                entity.ToTable("Subscriptions").HasKey(x => x.Id);
                entity.Property(x => x.Status).HasMaxLength(50).IsRequired();
                entity.Property(x => x.Notes).HasMaxLength(1000);
                entity.HasIndex(x => x.CustomerId).IsUnique();
                entity.HasOne(x => x.Customer)
                    .WithOne(x => x.Subscription)
                    .HasForeignKey<Subscription>(x => x.CustomerId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasQueryFilter(x => x.CustomerId == _currentUserService.GetCustomerId());

            });

            modelBuilder.Entity<Location>(entity =>
            {
                entity.ToTable("Locations").HasKey(x => x.Id);
                entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
                entity.Property(x => x.Code).HasMaxLength(50).IsRequired();
                entity.Property(x => x.Level).IsRequired();
                entity.HasIndex(x => new { x.CustomerId, x.Code }).IsUnique();
                entity.HasOne(x => x.Customer)
                    .WithMany()
                    .HasForeignKey(x => x.CustomerId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(x => x.Parent)
                    .WithMany(x => x.Children)
                    .HasForeignKey(x => x.ParentId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasQueryFilter(x => x.CustomerId == _currentUserService.GetCustomerId());
            });

            modelBuilder.Entity<Tenant>(entity =>
            {
                entity.ToTable("Tenants").HasKey(x => x.Id);
                entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
                entity.Property(x => x.Code).HasMaxLength(50).IsRequired();
                entity.Property(x => x.Status).HasMaxLength(50).IsRequired();
                entity.HasIndex(x => new { x.CustomerId, x.Code }).IsUnique();
                entity.HasOne(x => x.Customer)
                    .WithMany()
                    .HasForeignKey(x => x.CustomerId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasQueryFilter(x => x.CustomerId == _currentUserService.GetCustomerId());
            });

            modelBuilder.Entity<Site>(entity =>
            {
                entity.ToTable("Sites").HasKey(x => x.Id);
                entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
                entity.Property(x => x.Code).HasMaxLength(50).IsRequired();
                entity.Property(x => x.Status).HasMaxLength(50).IsRequired();
                entity.Property(x => x.Coordinates).HasMaxLength(100).IsRequired();
                entity.HasIndex(x => new { x.CustomerId, x.Code }).IsUnique();
                entity.HasOne(x => x.Customer)
                    .WithMany()
                    .HasForeignKey(x => x.CustomerId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(x => x.Region)
                    .WithMany()
                    .HasForeignKey(x => x.RegionId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(x => x.SubRegion)
                    .WithMany()
                    .HasForeignKey(x => x.SubRegionId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(x => x.Zone)
                    .WithMany()
                    .HasForeignKey(x => x.ZoneId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasQueryFilter(x => x.CustomerId == _currentUserService.GetCustomerId());
            });

            modelBuilder.Entity<Device>(entity =>
            {
                entity.ToTable("Devices").HasKey(x => x.Id);
                entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
                entity.Property(x => x.Code).HasMaxLength(50).IsRequired();
                entity.Property(x => x.Status).HasMaxLength(50).IsRequired();
                entity.Property(x => x.InstallationDate).IsRequired();
                entity.Property(x => x.MqttHost).HasMaxLength(255).IsRequired();
                entity.Property(x => x.MqttClientId).HasMaxLength(100).IsRequired();
                entity.Property(x => x.MqttUsername).HasMaxLength(100);
                entity.Property(x => x.MqttPassword).HasMaxLength(255).IsRequired();
                entity.Property(x => x.RmsSubscribeTopic).HasMaxLength(255).IsRequired();
                entity.Property(x => x.AiSubscribeTopic).HasMaxLength(255).IsRequired();
                entity.Property(x => x.RectifierBrand).HasMaxLength(100).IsRequired();
                entity.Property(x => x.RectifierCapacity).HasMaxLength(100).IsRequired();
                entity.Property(x => x.BatteryBrand).HasMaxLength(100).IsRequired();
                entity.Property(x => x.BatteryCapacity).HasMaxLength(100).IsRequired();
                entity.Property(x => x.SolarBrand).HasMaxLength(100).IsRequired();
                entity.Property(x => x.SolarCapacity).HasMaxLength(100).IsRequired();
                entity.Property(x => x.GeneratorBrand).HasMaxLength(100).IsRequired();
                entity.Property(x => x.GeneratorCapacity).HasMaxLength(100).IsRequired();
                entity.Property(x => x.RmsSerialNumber).HasMaxLength(100).IsRequired();
                entity.Property(x => x.SimCardNumber).HasMaxLength(50).IsRequired();
                entity.HasIndex(x => new { x.CustomerId, x.Code }).IsUnique();
                entity.HasIndex(x => x.SiteId).IsUnique();
                entity.HasOne(x => x.Customer)
                    .WithMany()
                    .HasForeignKey(x => x.CustomerId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(x => x.Site)
                    .WithMany()
                    .HasForeignKey(x => x.SiteId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasQueryFilter(x => x.CustomerId == _currentUserService.GetCustomerId());
            });

            modelBuilder.Entity<Lookup>(entity =>
            {
                entity.ToTable("Lookups").HasKey(x => x.Id);
                entity.Property(x => x.Category).HasMaxLength(50).IsRequired();
                entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
                entity.Property(x => x.Value).HasMaxLength(100).IsRequired();
                entity.HasIndex(x => x.Category);

                // Seed default lookups
                entity.HasData(
                    // Location Type Lookups
                    new Lookup { Id = 1, Category = "LocationType", Name = "Region", Value = "1", Order = 1, CreatedBy = 1, CreatedOn = DateTime.UtcNow, IsActive = true },
                    new Lookup { Id = 2, Category = "LocationType", Name = "SubRegion", Value = "2", Order = 2, CreatedBy = 1, CreatedOn = DateTime.UtcNow, IsActive = true },
                    new Lookup { Id = 3, Category = "LocationType", Name = "Zone", Value = "3", Order = 3, CreatedBy = 1, CreatedOn = DateTime.UtcNow, IsActive = true },
                    
                    // Status Lookups
                    new Lookup { Id = 4, Category = "Status", Name = "Active", Value = "Active", Order = 1, CreatedBy = 1, CreatedOn = DateTime.UtcNow, IsActive = true },
                    new Lookup { Id = 5, Category = "Status", Name = "Inactive", Value = "Inactive", Order = 2, CreatedBy = 1, CreatedOn = DateTime.UtcNow, IsActive = true },
                    new Lookup { Id = 6, Category = "Status", Name = "Suspended", Value = "Suspended", Order = 3, CreatedBy = 1, CreatedOn = DateTime.UtcNow, IsActive = true },
                    
                    // Subscription Status Lookups
                    new Lookup { Id = 7, Category = "SubscriptionStatus", Name = "Active", Value = "Active", Order = 1, CreatedBy = 1, CreatedOn = DateTime.UtcNow, IsActive = true },
                    new Lookup { Id = 8, Category = "SubscriptionStatus", Name = "Inactive", Value = "Inactive", Order = 2, CreatedBy = 1, CreatedOn = DateTime.UtcNow, IsActive = true },
                    new Lookup { Id = 9, Category = "SubscriptionStatus", Name = "Expired", Value = "Expired", Order = 3, CreatedBy = 1, CreatedOn = DateTime.UtcNow, IsActive = true }
                );
            });

            modelBuilder.Entity<TelemetryMessage>(entity =>
            {
                entity.ToTable("TelemetryMessages").HasKey(x => x.Id);
                entity.Property(x => x.TenantId).HasMaxLength(100).IsRequired();
                entity.Property(x => x.SiteId).HasMaxLength(100).IsRequired();
                entity.Property(x => x.DeviceId).HasMaxLength(100).IsRequired();
                entity.Property(x => x.Topic).HasMaxLength(255).IsRequired();
                entity.Property(x => x.ReceivedAtUtc).IsRequired();
                entity.Property(x => x.DecodedPayloadJson).HasColumnType("jsonb").IsRequired();
                entity.Property(x => x.DecodeError).HasMaxLength(2000);
                entity.HasIndex(x => new { x.TenantId, x.SiteId, x.DeviceId, x.ReceivedAtUtc });
                entity.HasIndex(x => x.ReceivedAtUtc);
            });

            modelBuilder.Entity<DeviceTelemetryLatest>(entity =>
            {
                entity.ToTable("DeviceTelemetryLatest").HasKey(x => x.Id);
                entity.Property(x => x.TenantId).HasMaxLength(100).IsRequired();
                entity.Property(x => x.SiteId).HasMaxLength(100).IsRequired();
                entity.Property(x => x.DeviceId).HasMaxLength(100).IsRequired();
                entity.Property(x => x.ReceivedAtUtc).IsRequired();
                entity.Property(x => x.SummaryPayloadJson).HasColumnType("jsonb").IsRequired();
                entity.Property(x => x.DecodeError).HasMaxLength(2000);
                entity.HasIndex(x => x.DeviceId).IsUnique();
                entity.HasIndex(x => x.SiteId);
            });
        }

        public DbSet<Weather> Weathers { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<Site> Sites { get; set; }
        public DbSet<Device> Devices { get; set; }
        public DbSet<Lookup> Lookups { get; set; }
        public DbSet<TelemetryMessage> TelemetryMessages { get; set; }
        public DbSet<DeviceTelemetryLatest> DeviceTelemetryLatest { get; set; }

        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            SetAuditFields();
            return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        private void SetAuditFields()
        {
            var modifiedEntries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);
            foreach (var entity in modifiedEntries)
            {
                if (entity.Entity is Tenant tenant)
                {
                    tenant.CustomerId = _currentUserService.GetCustomerId();
                }

                if (entity.Entity is Site site)
                {
                    site.CustomerId = _currentUserService.GetCustomerId();
                }

                if (entity.Entity is Device device)
                {
                    device.CustomerId = _currentUserService.GetCustomerId();
                }

                if (entity.Entity is BaseEntity bentity)
                {
                    switch (entity.State)
                    {
                        case EntityState.Added:
                            bentity.CreatedBy = _currentUserService.GetLoggedInUserId();
                            bentity.CreatedOn = DateTime.UtcNow;
                            break;
                        case EntityState.Modified:
                            bentity.ModifiedBy = _currentUserService.GetLoggedInUserId();
                            bentity.ModifiedOn = DateTime.UtcNow;
                            break;
                    }
                }
            }
        }
    }
}
