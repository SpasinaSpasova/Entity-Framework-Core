using System.Diagnostics.CodeAnalysis;

using Microsoft.EntityFrameworkCore;
using P03_FootballBetting.Data.Models;

namespace P03_FootballBetting.Data
{
    public class FootballBettingContext : DbContext
    {
        //In order to test the db
        public FootballBettingContext()
        {

        }

        //For judge/For outer connection
        public FootballBettingContext([NotNull] DbContextOptions options)
            : base(options)
        {

        }

        public virtual DbSet<Bet> Bets { get; set; }
        public virtual DbSet<Color> Colors { get; set; }
        public virtual DbSet<Country> Countries { get; set; }
        public virtual DbSet<Game> Games { get; set; }
        public virtual DbSet<Player> Players { get; set; }
        public virtual DbSet<PlayerStatistic> PlayerStatistics { get; set; }
        public virtual DbSet<Position> Positions { get; set; }
        public virtual DbSet<Team> Teams { get; set; }
        public virtual DbSet<Town> Towns { get; set; }
        public virtual DbSet<User> Users { get; set; }


        //To configure connection to your server
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(@"Server=.;Database=FootballSystem;Integrated Security=True;");
            }
        }

        //To configure database relations (DDL)
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<PlayerStatistic>(k =>
            {
                k.HasKey(ps => new { ps.GameId, ps.PlayerId });
            });

            modelBuilder.Entity<Team>(t =>
            {
                t.HasOne(a => a.PrimaryKitColor)
                .WithMany(b => b.PrimaryKitTeams)
                .HasForeignKey(t => t.PrimaryKitColorId)
                .OnDelete(DeleteBehavior.Restrict);

                t.HasOne(a => a.SecondaryKitColor)
                .WithMany(b => b.SecondaryKitTeams)
                .HasForeignKey(t => t.SecondaryKitColorId)
                .OnDelete(DeleteBehavior.Restrict);

            });

            modelBuilder.Entity<Game>(g =>
            {
                g.HasOne(a => a.HomeTeam)
                .WithMany(b => b.HomeGames)
                .HasForeignKey(g => g.HomeTeamId)
                .OnDelete(DeleteBehavior.Restrict);

                g.HasOne(a => a.AwayTeam)
                 .WithMany(b => b.AwayGames)
                 .HasForeignKey(g => g.AwayTeamId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

        }
    }
}
