namespace Fragrant_World.Classes
{
    public class User
    {
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public string Surname { get; set; }
        public string Name { get; set; }
        public string Patronymic { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }

        public User(int roleId, string surname, string name, string patronymic, string login, string password)
        {
            RoleId = roleId;
            Surname = surname;
            Name = name;
            Patronymic = patronymic;
            Login = login;
            Password = password;
        }

        public User() : this(2, string.Empty, string.Empty, "Гость", string.Empty, string.Empty) { }
    }
}
