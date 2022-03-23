using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using TousenBot.Models;

namespace TousenBot.Data
{
    public class ApplicationDbContext : DbContext
    {
        private readonly string _connectionString;
        public ApplicationDbContext(string connectionString)
        {
            _connectionString = connectionString;
        }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }
        public DbSet<User> User { get; set; }

        public DbSet<Jutsu> Jutsu { get; set; }
        public DbSet<Feat> Feat { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connectionStringBuilder = new SqlConnectionStringBuilder { DataSource = "server=localhost\\SQLExpress;database=Naruto5e;integrated security=true;" };
            var connectionString = connectionStringBuilder.ToString();
            var connection = new SqlConnection(connectionString);
            //optionsBuilder.UseSqlServer(connection);
            optionsBuilder.UseSqlServer("server=localhost\\SQLExpress;database=Naruto5e;integrated security=true;");
        }

    }
}
