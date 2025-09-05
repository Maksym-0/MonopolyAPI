namespace Monopoly.Models.AccountModels
{
    public class Account
    {
        public string Id {  get; set; }
        public string Name { get; set; }
        public string Password { get; set; }

        public Account(string id, string name, string password)
        {
            Id = id;
            Name = name;
            Password = password;
        }
        public Account() { }
    }
}
