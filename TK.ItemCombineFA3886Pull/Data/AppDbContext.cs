using TK.ItemCombineFA3886Pull.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace TK.ItemCombineFA3886Pull.Data
{
    public class AppDbContext : DbContext
    {
        private string connectionString;
        public AppDbContext()
        {
            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory());
            builder.AddJsonFile("appsettings.json", optional: false);
            var configuration = builder.Build();
            connectionString = configuration.GetConnectionString("csbContext").ToString();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseOracle(connectionString);
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FA3886_00101_ITEM>().HasKey(u => new { u.FA3886_PROG_NR, u.FA3886_AUSW_NR, u.FA3886_PRIO, u.FA3886_ART_KOMM_ID, u.FA3886_SORT_NR, u.FA3886_ART_NR, u.FA3886_SORT_NR_2, u.FA3886_BS_NR, u.FA3886_BS_SUB_NR, u.FA3886_TYP, u.FA3886_POSTEN_ID });
            //modelBuilder.Entity<FA3886_00101_ITEM>().HasNoKey();
            modelBuilder.Entity<FA3886_00101>().HasKey(u => new { u.FA3886_PROG_NR, u.FA3886_AUSW_NR, u.FA3886_PRIO, u.FA3886_ART_KOMM_ID, u.FA3886_SORT_NR, u.FA3886_ART_NR, u.FA3886_SORT_NR_2, u.FA3886_BS_NR, u.FA3886_BS_SUB_NR, u.FA3886_TYP, u.FA3886_POSTEN_ID });
            //modelBuilder.Entity<FA3886_00101>().HasNoKey();

        }
        public DbSet<FA3886_00101_ITEM> FA3886_00101_ITEM { get; set; }
        public DbSet<FA3886_00101> FA3886_00101 { get; set; }

        ~AppDbContext()
        {
        }
    }
}
