using Microsoft.EntityFrameworkCore;

namespace Jering.VectorArtKit.DatabaseInterface
{
    public partial class VakDbContext : DbContext
    {
        // By convention, each entity will be setup to map to a table with the same name as 
        // the DbSet<TEntity> property that exposes the entity on the derived context
        public virtual DbSet<VakAccount> Accounts { get; set; }
        //public virtual DbSet<AccountClaims> AccountClaims { get; set; }
        //public virtual DbSet<AccountRoles> AccountRoles { get; set; }
        //public virtual DbSet<Claim> Claims { get; set; }
        //public virtual DbSet<RoleClaims> RoleClaims { get; set; }
        //public virtual DbSet<Role> Roles { get; set; }

        public VakDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<VakAccount>(entity =>
            {
                entity
                    .Property(e => e.AccountId)
                    .ValueGeneratedOnAdd();

                entity
                    .HasKey(e => e.AccountId)
                    .HasName("PK_dbo_Accounts");

                entity
                    .Property(e => e.RowVersion)
                    .ValueGeneratedOnAddOrUpdate()
                    .IsConcurrencyToken();
            });

            //modelBuilder.Entity<AccountClaims>(entity =>
            //{
            //    entity.HasKey(e => new { e.ClaimId, e.AccountId })
            //        .HasName("PK_dbo_AccountClaims");

            //    entity.HasIndex(e => e.AccountId)
            //        .HasName("IX_dbo_AccountClaims_AccountID");

            //    entity.HasIndex(e => e.ClaimId)
            //        .HasName("IX_dbo_AccountClaims_ClaimID");

            //    entity.HasOne(d => d.Account)
            //        .WithMany(p => p.AccountClaims)
            //        .HasForeignKey(d => d.AccountId)
            //        .HasConstraintName("FK_dbo_AccountClaims_dbo_Accounts");

            //    entity.HasOne(d => d.Claim)
            //        .WithMany(p => p.AccountClaims)
            //        .HasForeignKey(d => d.ClaimId)
            //        .HasConstraintName("FK_dbo_AccountClaims_dbo_Claims");
            //});

            //modelBuilder.Entity<AccountRoles>(entity =>
            //{
            //    entity.HasKey(e => new { e.AccountId, e.RoleId })
            //        .HasName("PK_dbo_AccountRoles");

            //    entity.HasIndex(e => e.AccountId)
            //        .HasName("IX_dbo_AccountRoles_AccountID");

            //    entity.HasIndex(e => e.RoleId)
            //        .HasName("IX_dbo_AccountRoles_RoleID");

            //    entity.HasOne(d => d.Account)
            //        .WithMany(p => p.AccountRoles)
            //        .HasForeignKey(d => d.AccountId)
            //        .HasConstraintName("FK_dbo_AccountRoles_dbo_Accounts");

            //    entity.HasOne(d => d.Role)
            //        .WithMany(p => p.AccountRoles)
            //        .HasForeignKey(d => d.RoleId)
            //        .HasConstraintName("FK_dbo_AccountRoles_dbo_Roles");
            //});

            //modelBuilder.Entity<Claim>(entity =>
            //{
            //    entity.HasKey(e => e.ClaimId)
            //        .HasName("PK_dbo_Claims");

            //    entity.HasIndex(e => new { e.Type, e.Value })
            //        .HasName("UQ_dbo_Claims_Type_Value")
            //        .IsUnique();

            //    entity.Property(e => e.Type)
            //        .IsRequired()
            //        .HasMaxLength(256);

            //    entity.Property(e => e.Value)
            //        .IsRequired()
            //        .HasMaxLength(256);
            //});

            //modelBuilder.Entity<RoleClaims>(entity =>
            //{
            //    entity.HasKey(e => new { e.ClaimId, e.RoleId })
            //        .HasName("PK_dbo_RoleClaims");

            //    entity.HasIndex(e => e.ClaimId)
            //        .HasName("IX_dbo_RoleClaims_ClaimId");

            //    entity.HasIndex(e => e.RoleId)
            //        .HasName("IX_dbo_RoleClaims_RoleId");

            //    entity.HasOne(d => d.Claim)
            //        .WithMany(p => p.RoleClaims)
            //        .HasForeignKey(d => d.ClaimId)
            //        .HasConstraintName("FK_dbo_RoleClaims_dbo_Claims");

            //    entity.HasOne(d => d.Role)
            //        .WithMany(p => p.RoleClaims)
            //        .HasForeignKey(d => d.RoleId)
            //        .HasConstraintName("FK_dbo_RoleClaims_dbo_Accounts");
            //});

            //modelBuilder.Entity<Role>(entity =>
            //{
            //    entity.HasKey(e => e.RoleId)
            //        .HasName("PK_dbo_Roles");

            //    entity.HasIndex(e => e.Name)
            //        .HasName("UQ_dbo_Roles_Name")
            //        .IsUnique();

            //    entity.Property(e => e.Name)
            //        .IsRequired()
            //        .HasMaxLength(256);
            //});
        }
    }
}