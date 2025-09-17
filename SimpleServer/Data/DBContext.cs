using System.Data.Entity;
using SimpleServer.Models;  

namespace SimpleServer.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext() : base("PostgresDb") { }

        public DbSet<User> Users { get; set; }
    }
}