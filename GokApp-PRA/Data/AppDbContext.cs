using GokApp_PRA.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GokApp_PRA.Data
{
    internal class AppDbContext : DbContext
    {
        public DbSet<Bet> Bets { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql(
                "server=localhost;" +                     // Server name
                "port=3306;" +                            // Server port
                "user=root;" +                     // Username
                "password=;" +                 // Password
                "database=Native_EindOpdracht;"       // Database name
                , Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.0.21-mysql") // Version
                );
        }
    }
}
