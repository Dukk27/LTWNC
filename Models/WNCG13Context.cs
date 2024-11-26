using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace WNC_G13.Models
{
    public partial class WNCG13Context : DbContext
    {
        public WNCG13Context()
        {
        }

        public WNCG13Context(DbContextOptions<WNCG13Context> options)
            : base(options)
        {
        }

        public virtual DbSet<ChatChannel> ChatChannels { get; set; } = null!;
        public virtual DbSet<Document> Documents { get; set; } = null!;
        public virtual DbSet<Message> Messages { get; set; } = null!;
        public virtual DbSet<Organization> Organizations { get; set; } = null!;
        public virtual DbSet<Project> Projects { get; set; } = null!;
        public virtual DbSet<ProjectTask> ProjectTasks { get; set; } = null!;
        public virtual DbSet<User> Users { get; set; } = null!;
        public virtual DbSet<UserProject> UserProjects { get; set; } = null!;
        public virtual DbSet<UserOrganization> UserOrganizations { get; set; } = null!;
        public virtual DbSet<ProgressReport> ProgressReports { get; set; } = null!;
        public virtual DbSet<Notification> Notifications { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseSqlServer("Server=DUKK;Database=WNCG13;Trusted_Connection=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ChatChannel>(entity =>
            {
                entity.Property(e => e.ChannelName).HasMaxLength(100);

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.HasMany(d => d.Users)
                    .WithMany(p => p.Channels)
                    .UsingEntity<Dictionary<string, object>>(
                        "ChannelMember",
                        l => l.HasOne<User>().WithMany().HasForeignKey("UserId").OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK__ChannelMe__UserI__5812160E"),
                        r => r.HasOne<ChatChannel>().WithMany().HasForeignKey("ChannelId").OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK__ChannelMe__Chann__571DF1D5"),
                        j =>
                        {
                            j.HasKey("ChannelId", "UserId").HasName("PK__ChannelM__E9BB64D0F95C2906");

                            j.ToTable("ChannelMembers");
                        });
            });

            modelBuilder.Entity<Document>(entity =>
            {
                entity.Property(e => e.Description).HasMaxLength(500);

                entity.Property(e => e.DocumentName).HasMaxLength(255);

                entity.Property(e => e.DocumentUrl).HasMaxLength(1000);

                entity.Property(e => e.UploadedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.Project)
                    .WithMany(p => p.Documents)
                    .HasForeignKey(d => d.ProjectId)
                    .HasConstraintName("FK__Documents__Proje__282DF8C2");

                entity.HasOne(d => d.Task)
                    .WithMany(p => p.Documents)
                    .HasForeignKey(d => d.TaskId)
                    .HasConstraintName("FK__Documents__TaskI__29221CFB");

                entity.HasOne(d => d.UploadedByNavigation)
                    .WithMany(p => p.Documents)
                    .HasForeignKey(d => d.UploadedBy)
                    .HasConstraintName("FK__Documents__Uploa__2A164134");
            });

            modelBuilder.Entity<Message>(entity =>
            {
                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.MessageContent).HasMaxLength(1000);

                entity.HasOne(d => d.Channel)
                    .WithMany(p => p.Messages)
                    .HasForeignKey(d => d.ChannelId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Messages__Channe__6C190EBB");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Messages)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Messages__UserId__6D0D32F4");
            });

            modelBuilder.Entity<Organization>(entity =>
            {
                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Name).HasMaxLength(100);

                entity.Property(e => e.SubscriptionPlan).HasMaxLength(50);

                entity.HasOne(d => d.CreatedByNavigation)
                    .WithMany(p => p.Organizations)
                    .HasForeignKey(d => d.CreatedBy)
                    .HasConstraintName("FK__Organizat__Creat__3D5E1FD2");
            });

            modelBuilder.Entity<Project>(entity =>
            {
                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Description).HasMaxLength(500);

                entity.Property(e => e.Name).HasMaxLength(100);

                entity.HasOne(d => d.CreatedByNavigation)
                    .WithMany(p => p.Projects)
                    .HasForeignKey(d => d.CreatedBy)
                    .HasConstraintName("FK__Projects__Create__4316F928");

                entity.HasOne(d => d.Organization)
                    .WithMany(p => p.Projects)
                    .HasForeignKey(d => d.OrganizationId)
                    .HasConstraintName("FK__Projects__Organi__4222D4EF");
            });

            modelBuilder.Entity<ProjectTask>(entity =>
            {
                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Description).HasMaxLength(500);

                entity.Property(e => e.DueDate).HasColumnType("datetime");

                entity.Property(e => e.Priority).HasDefaultValueSql("((0))");

                entity.Property(e => e.Status).HasDefaultValueSql("((0))");

                entity.Property(e => e.TaskName).HasMaxLength(100);

                entity.HasOne(d => d.Project)
                    .WithMany(p => p.ProjectTasks)
                    .HasForeignKey(d => d.ProjectId)
                    .HasConstraintName("FK__ProjectTa__Proje__1EA48E88");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(e => e.Email, "UQ__Users__A9D10534B14C5631")
                    .IsUnique();

                entity.Property(e => e.Address).HasMaxLength(255);

                entity.Property(e => e.Birthday).HasColumnType("datetime");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Email).HasMaxLength(100);

                entity.Property(e => e.FullName).HasMaxLength(100);

                entity.Property(e => e.Password).HasMaxLength(100);

                entity.Property(e => e.PhoneNumber).HasMaxLength(15);
            });

            modelBuilder.Entity<UserProject>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.ProjectId })
                    .HasName("PK__UserProj__00E967A3BC9502DE");

                entity.Property(e => e.IsPm).HasColumnName("IsPM");

                entity.HasOne(d => d.Project)
                    .WithMany(p => p.UserProjects)
                    .HasForeignKey(d => d.ProjectId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__UserProje__Proje__5441852A");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UserProjects)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__UserProje__UserI__534D60F1");
            });
            modelBuilder.Entity<UserOrganization>()
            .ToTable("UserOrganization");

            modelBuilder.Entity<UserOrganization>()
           .HasKey(uo => new { uo.UserId, uo.OrganizationId });

            // Thiết lập mối quan hệ giữa User và UserOrganization
            modelBuilder.Entity<UserOrganization>()
                .HasOne(uo => uo.User)
                .WithMany(u => u.UserOrganizations)
                .HasForeignKey(uo => uo.UserId);

            // Thiết lập mối quan hệ giữa Organization và UserOrganization
            modelBuilder.Entity<UserOrganization>()
                .HasOne(uo => uo.Organization)
                .WithMany(o => o.UserOrganizations)
                .HasForeignKey(uo => uo.OrganizationId);

            OnModelCreatingPartial(modelBuilder);


            // Đặt tên bảng
            modelBuilder.Entity<ProgressReport>().ToTable("ProgressReports");

            // Đặt khóa chính
            modelBuilder.Entity<ProgressReport>()
                .HasKey(pr => pr.Id);

            // Cấu hình khóa ngoại tới User
            modelBuilder.Entity<ProgressReport>()
                .HasOne(pr => pr.User)
                .WithMany(u => u.ProgressReports)
                .HasForeignKey(pr => pr.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Cấu hình khóa ngoại tới Task (nullable)
            modelBuilder.Entity<ProgressReport>()
                .HasOne(pr => pr.Task)
                .WithMany(t => t.ProgressReports)
                .HasForeignKey(pr => pr.TaskId)
                .OnDelete(DeleteBehavior.SetNull);

            // Cấu hình khóa ngoại tới Project
            modelBuilder.Entity<ProgressReport>()
                .HasOne(pr => pr.Project)
                .WithMany(p => p.ProgressReports)
                .HasForeignKey(pr => pr.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProgressReport>()
                .HasOne(pr => pr.Approver)
                .WithMany()
                .HasForeignKey(pr => pr.ApprovedBy)
                .OnDelete(DeleteBehavior.SetNull);

            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Notification>(entity =>
            {
                // Cấu hình khóa chính
                entity.HasKey(n => n.Id);

                // Cấu hình mối quan hệ với bảng Users (UserId)
                entity.HasOne(n => n.User)
                        .WithMany(u => u.Notifications)
                        .HasForeignKey(n => n.UserId)
                        .OnDelete(DeleteBehavior.Cascade);

                // Cấu hình mối quan hệ với bảng Projects (ProjectId)
                entity.HasOne(n => n.Project)
                        .WithMany(p => p.Notifications)
                        .HasForeignKey(n => n.ProjectId)
                        .OnDelete(DeleteBehavior.SetNull);

                // Cấu hình các thuộc tính
                entity.Property(n => n.Message)
                        .IsRequired()
                        .HasMaxLength(500); // Đặt độ dài tối đa cho Message

                entity.Property(n => n.CreatedAt)
                        .HasDefaultValueSql("GETDATE()");

                entity.Property(n => n.IsRead)
                        .HasDefaultValue(false);
            });
        }


        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
