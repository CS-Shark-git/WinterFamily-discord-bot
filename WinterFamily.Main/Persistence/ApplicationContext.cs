using Microsoft.EntityFrameworkCore;
using WinterFamily.Main.Persistence.Models;
using WinterFamily.Main.Utils.Discord;

namespace WinterFamily.Main.Persistence;

internal class ApplicationContext : DbContext
{
    public DbSet<Cooldown> Cooldowns => Set<Cooldown>();
    public DbSet<Vacancy> Vacancies => Set<Vacancy>();
    public DbSet<AutoRole> AutoRoles => Set<AutoRole>();
    public DbSet<SubmittedUser> SubmittedUsers => Set<SubmittedUser>();
    public DbSet<MiddleMan> MiddleMans => Set<MiddleMan>();
    public DbSet<Review> Reviews => Set<Review>();

    public DbSet<ActiveTrade> ActiveTrades => Set<ActiveTrade>();

    public ApplicationContext()     
    {
        bool isDatabaseExists = Database.CanConnect();
        if(isDatabaseExists != true)
        {
            Database.EnsureCreated();
            AutoRoles.AddRange(
                new AutoRole { CustomId = "events_role", RoleId = Settings.EventsRole },
                new AutoRole { CustomId = "giveaways_role", RoleId = Settings.GiveawaysRole },
                new AutoRole { CustomId = "news_role", RoleId = Settings.NewsRole },
                new AutoRole { CustomId = "shops_role", RoleId = Settings.ShopsRole });
            Vacancies.AddRange(
                new Vacancy() { Value = "moderator_value", IsOpened = true },
                new Vacancy() { Value = "eventer_value", IsOpened = true });
            SaveChanges();
        }
    }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) 
        => optionsBuilder.UseSqlite("Data Source=application.db");


    protected override void OnModelCreating(ModelBuilder modelBuilder) 
    {
        modelBuilder.Entity<MiddleMan>()
            .HasMany(b => b.Reviews)
            .WithOne(x => x.MiddleMan)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
