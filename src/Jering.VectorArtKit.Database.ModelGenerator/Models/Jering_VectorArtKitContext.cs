using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Jering.VectorArtKit.Database.ModelGenerator.Models
{
    public partial class Jering_VectorArtKitContext : DbContext
    {
        public virtual DbSet<AccountClaims> AccountClaims { get; set; }
        public virtual DbSet<AccountRoles> AccountRoles { get; set; }
        public virtual DbSet<Accounts> Accounts { get; set; }
        public virtual DbSet<Claims> Claims { get; set; }
        public virtual DbSet<RoleClaims> RoleClaims { get; set; }
        public virtual DbSet<Roles> Roles { get; set; }
        public virtual DbSet<Tags> Tags { get; set; }
        public virtual DbSet<VakUnitTags> VakUnitTags { get; set; }
        public virtual DbSet<VakUnits> VakUnits { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            #warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
            optionsBuilder.UseSqlServer(@"Data Source=JEREMY-PC;Initial Catalog=Jering.VectorArtKit;Integrated Security=True;Connect Timeout=60;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AccountClaims>(entity =>
            {
                entity.HasKey(e => new { e.ClaimId, e.AccountId })
                    .HasName("PK_dbo_AccountClaims");

                entity.HasIndex(e => e.AccountId)
                    .HasName("IX_dbo_AccountClaims_AccountID");

                entity.HasIndex(e => e.ClaimId)
                    .HasName("IX_dbo_AccountClaims_ClaimID");

                entity.HasOne(d => d.Account)
                    .WithMany(p => p.AccountClaims)
                    .HasForeignKey(d => d.AccountId)
                    .HasConstraintName("FK_dbo_AccountClaims_dbo_Accounts");

                entity.HasOne(d => d.Claim)
                    .WithMany(p => p.AccountClaims)
                    .HasForeignKey(d => d.ClaimId)
                    .HasConstraintName("FK_dbo_AccountClaims_dbo_Claims");
            });

            modelBuilder.Entity<AccountRoles>(entity =>
            {
                entity.HasKey(e => new { e.AccountId, e.RoleId })
                    .HasName("PK_dbo_AccountRoles");

                entity.HasIndex(e => e.AccountId)
                    .HasName("IX_dbo_AccountRoles_AccountID");

                entity.HasIndex(e => e.RoleId)
                    .HasName("IX_dbo_AccountRoles_RoleID");

                entity.HasOne(d => d.Account)
                    .WithMany(p => p.AccountRoles)
                    .HasForeignKey(d => d.AccountId)
                    .HasConstraintName("FK_dbo_AccountRoles_dbo_Accounts");

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.AccountRoles)
                    .HasForeignKey(d => d.RoleId)
                    .HasConstraintName("FK_dbo_AccountRoles_dbo_Roles");
            });

            modelBuilder.Entity<Accounts>(entity =>
            {
                entity.HasKey(e => e.AccountId)
                    .HasName("PK_dbo_Accounts");

                entity.HasIndex(e => e.DisplayName)
                    .HasName("UQ_dbo_Accounts_DisplayName")
                    .IsUnique();

                entity.HasIndex(e => e.Email)
                    .HasName("UQ_dbo_Accounts_Email")
                    .IsUnique();

                entity.Property(e => e.AltEmail).HasMaxLength(256);

                entity.Property(e => e.AltEmailVerified).HasDefaultValueSql("0");

                entity.Property(e => e.DisplayName)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(e => e.EmailVerified).HasDefaultValueSql("0");

                entity.Property(e => e.PasswordHash)
                    .IsRequired()
                    .HasMaxLength(84);

                entity.Property(e => e.PasswordLastChanged).HasColumnType("datetimeoffset(0)");

                entity.Property(e => e.RowVersion)
                    .IsRequired()
                    .HasColumnType("timestamp")
                    .ValueGeneratedOnAddOrUpdate();

                entity.Property(e => e.TwoFactorEnabled).HasDefaultValueSql("0");
            });

            modelBuilder.Entity<Claims>(entity =>
            {
                entity.HasKey(e => e.ClaimId)
                    .HasName("PK_dbo_Claims");

                entity.HasIndex(e => new { e.Type, e.Value })
                    .HasName("UQ_dbo_Claims_Type_Value")
                    .IsUnique();

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(e => e.Value)
                    .IsRequired()
                    .HasMaxLength(256);
            });

            modelBuilder.Entity<RoleClaims>(entity =>
            {
                entity.HasKey(e => new { e.ClaimId, e.RoleId })
                    .HasName("PK_dbo_RoleClaims");

                entity.HasIndex(e => e.ClaimId)
                    .HasName("IX_dbo_RoleClaims_ClaimId");

                entity.HasIndex(e => e.RoleId)
                    .HasName("IX_dbo_RoleClaims_RoleId");

                entity.HasOne(d => d.Claim)
                    .WithMany(p => p.RoleClaims)
                    .HasForeignKey(d => d.ClaimId)
                    .HasConstraintName("FK_dbo_RoleClaims_dbo_Claims");

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.RoleClaims)
                    .HasForeignKey(d => d.RoleId)
                    .HasConstraintName("FK_dbo_RoleClaims_dbo_Accounts");
            });

            modelBuilder.Entity<Roles>(entity =>
            {
                entity.HasKey(e => e.RoleId)
                    .HasName("PK_dbo_Roles");

                entity.HasIndex(e => e.Name)
                    .HasName("UQ_dbo_Roles_Name")
                    .IsUnique();

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(256);
            });

            modelBuilder.Entity<Tags>(entity =>
            {
                entity.HasKey(e => e.TagId)
                    .HasName("PK_Tags");

                entity.Property(e => e.TagId).ValueGeneratedNever();

                entity.Property(e => e.Value)
                    .IsRequired()
                    .HasMaxLength(256);
            });

            modelBuilder.Entity<VakUnitTags>(entity =>
            {
                entity.HasKey(e => new { e.VakUnitId, e.TagId })
                    .HasName("PK_VakUnitTags");

                entity.HasIndex(e => e.TagId)
                    .HasName("IX_dbo_VakUnitTags_TagId");

                entity.HasIndex(e => e.VakUnitId)
                    .HasName("IX_dbo_VakUnitTags_VakUnitId");

                entity.HasOne(d => d.Tag)
                    .WithMany(p => p.VakUnitTags)
                    .HasForeignKey(d => d.TagId)
                    .HasConstraintName("FK_dbo_VakUnitTags_dbo_Tags");

                entity.HasOne(d => d.VakUnit)
                    .WithMany(p => p.VakUnitTags)
                    .HasForeignKey(d => d.VakUnitId)
                    .HasConstraintName("FK_dbo_VakUnitTags_dbo_VakUnits");
            });

            modelBuilder.Entity<VakUnits>(entity =>
            {
                entity.HasKey(e => e.VakUnitId)
                    .HasName("PK_VakUnits");

                entity.HasIndex(e => e.AccountId)
                    .HasName("IX_dbo_VakUnits_AccountId");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.HasOne(d => d.Account)
                    .WithMany(p => p.VakUnits)
                    .HasForeignKey(d => d.AccountId)
                    .HasConstraintName("FK_dbo_VakUnits_dbo_Accounts");
            });
        }
    }
}