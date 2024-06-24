namespace Fragrant_World.Classes
{
    //Класс для передачи данных пользователя между страницами
    internal static class UserDataBus
    {
        public static int UserId { get; set; }
        internal static int RoleId { get; set; }
        internal static string Surname { get; set; }
        internal static string Name { get; set; }
        internal static string Patronymic { get; set; }
        internal static string Login { get; set; }
        internal static string Password { get; set; }
        internal static string SearchQuery { get; set; }
    }
}
