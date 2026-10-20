using GroupModel;
using Microsoft.EntityFrameworkCore;
using System;
using UserAccountModel;
using UserProfileModel;

public class AppDbContext : DbContext
{
    public DbSet<UserAccount> UserAccounts { get; set; } = null!;

    public DbSet<UserProfile> UserProfiles { get; set; } = null!;

    public DbSet<UserGroup> UserGroups { get; set; } = null!;

    public UserRole Role { get; set; }

    public DbSet<Test> Tests => Set<Test>();

    public DbSet<TestQuestion> TestQuestions => Set<TestQuestion>();

    public DbSet<Question> Questions => Set<Question>();

    public DbSet<AnswerOption> AnswerOptions => Set<AnswerOption>();

    public DbSet<CorrectAnswer> CorrectAnswers => Set<CorrectAnswer>();

    public DbSet<MatchingPair> MatchingPairs => Set<MatchingPair>();

    public DbSet<TestSession> TestSessions => Set<TestSession>();

    public DbSet<UserAnswer> UserAnswers => Set<UserAnswer>();

    public DbSet<UserAnswerOption> UserAnswerOptions => Set<UserAnswerOption>();

    public DbSet<UserMatchingPairAnswer> UserMatchingPairAnswers
    => Set<UserMatchingPairAnswer>();

    public DbSet<ThreeDModel> ThreeDModels { get; set; }
    public DbSet<UserModelView> UserModelViews { get; set; }

    private readonly string _databasePath;

    public AppDbContext(string databasePath)
    {
        _databasePath = databasePath;
    }

    protected override void OnConfiguring(
        DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite(
            $"Data Source={_databasePath}");

    }

    private static readonly Guid AdminGroupId =
    new("11111111-1111-1111-1111-111111111111");

    private static readonly Guid AdminAccountId =
        new("22222222-2222-2222-2222-222222222222");

    private static readonly Guid AdminProfileId =
        new("33333333-3333-3333-3333-333333333333");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<UserProfile>()
            .HasOne(p => p.UserAccount)
            .WithOne(a => a.Profile)
            .HasForeignKey<UserProfile>(p => p.UserAccountId)
            .OnDelete(DeleteBehavior.Cascade);


        modelBuilder.Entity<UserProfile>()
            .HasOne(p => p.UserGroup)
            .WithMany(g => g.Users)
            .HasForeignKey(p => p.UserGroupId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<UserModelView>()
            .HasOne(view => view.UserProfile)
            .WithMany()
            .HasForeignKey(view => view.UserProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserModelView>()
            .HasOne(view => view.ThreeDModel)
            .WithMany(model => model.Views)
            .HasForeignKey(view => view.ThreeDModelId)
            .OnDelete(DeleteBehavior.Cascade);


        modelBuilder.Entity<UserGroup>().HasData(
            new UserGroup
            {
                Id = AdminGroupId,
                Name = "Admin",
                Number = 0
            }
        );


        modelBuilder.Entity<UserAccount>().HasData(
            new UserAccount
            {
                Id = AdminAccountId,
                Login = "admin",

                // SHA-256("admin")
                PasswordHash =
                    "8C6976E5B5410415BDE908BD4DEE15DFB167A9C873FC4BB8A81F6F2AB448A918"
            }
        );


        modelBuilder.Entity<UserProfile>().HasData(
            new UserProfile
            {
                Id = AdminProfileId,

                UserAccountId = AdminAccountId,

                FirstName = "Admin",
                LastName = "Administrator",
                Patronymic = null,

                UserGroupId = AdminGroupId
            }
        );

        modelBuilder.Entity<UserAnswer>()
            .HasIndex(x => new
            {
                x.TestSessionId,
                x.QuestionId
            })
            .IsUnique();


        modelBuilder.Entity<TestQuestion>()
            .HasKey(tq => new { tq.TestId, tq.QuestionId });

        modelBuilder.Entity<TestQuestion>()
            .HasOne(tq => tq.Test)
            .WithMany(t => t.TestQuestions)
            .HasForeignKey(tq => tq.TestId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TestQuestion>()
            .HasOne(tq => tq.Question)
            .WithMany(q => q.TestQuestions)
            .HasForeignKey(tq => tq.QuestionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TestSession>()
            .HasOne(session => session.UserProfile)
            .WithMany()
            .HasForeignKey(session => session.UserProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Question>()
            .HasMany(q => q.Options)
            .WithOne(o => o.Question)
            .HasForeignKey(o => o.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Question>()
            .HasMany(q => q.CorrectAnswers)
            .WithOne(a => a.Question)
            .HasForeignKey(a => a.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Question>()
            .HasMany(q => q.MatchingPairs)
            .WithOne(p => p.Question)
            .HasForeignKey(p => p.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CorrectAnswer>()
            .HasOne(a => a.AnswerOption)
            .WithMany()
            .HasForeignKey(a => a.AnswerOptionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
