namespace API1.Model
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; } // For demo purposes only! Use hashed passwords in production.
        public List<string> Roles { get; set; }
    }

}
