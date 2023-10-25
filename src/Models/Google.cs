namespace Telegram.Bot.Models
{
    public class Google
    {
        public int Id { get; set; }
        public string AccessToken { get; set; }

        public string RefreshToken { get; set; }

        public User Owner { get; set; }
    }
}
