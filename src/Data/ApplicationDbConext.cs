using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Models;

namespace Telegram.Bot.Examples.WebHook.Data
{
    public class ApplicationDbConext: DbContext
    {
        private readonly IConfiguration _configuration;
        public ApplicationDbConext(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public DbSet<User> Users { get; set; }

        public DbSet<Panel> Panels { get; set; }

        public DbSet<Doc> Docs { get; set; }

        public DbSet<Models.Google> Googles { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseNpgsql(_configuration.GetConnectionString("Default"));
        }

    }

   
}
