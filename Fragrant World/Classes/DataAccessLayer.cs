using Fragrant_World.Classes;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace Fragrant_World
{
    public class DataAccessLayer
    {
        public static string ServerName { get; set; } = "localhost";
        public static string DataBaseName { get; set; } = "Exam";
        public static string Login { get; set; } = "";
        public static string Password { get; set; } = "";
        public static string ConnectionString
        {
            get
            {
                SqlConnectionStringBuilder builder = new()
                {
                    DataSource = ServerName,
                    UserID = Login,
                    Password = Password,
                    InitialCatalog = DataBaseName,
                    IntegratedSecurity = true,
                    TrustServerCertificate = true,
                };
                return builder.ConnectionString;
            }
        }

        //Проверяет существует ли пользователь в БД
        public static bool IsUser(string login, string password)
        {
            using SqlConnection connection = new(ConnectionString);
            connection.Open();

            var query = $"SELECT * FROM ExamUser WHERE UserLogin = @login AND UserPassword = @password";

            SqlCommand command = new(query, connection);
            command.Parameters.AddWithValue("@login", login);
            command.Parameters.AddWithValue("@password", password);

            return Convert.ToBoolean(command.ExecuteScalar());
        }

        //Получение данных пользователя по логину и паролю
        public static User GetUserData(string login, string password)
        {
            SqlConnection connection = new(ConnectionString);
            connection.Open();

            string query = "SELECT * FROM ExamUser WHERE UserLogin = @login AND UserPassword = @password";

            SqlCommand command = new(query, connection);
            command.Parameters.AddWithValue("@login", login);
            command.Parameters.AddWithValue("@password", password);

            var reader = command.ExecuteReader();

            DataTable table = new();
            table.Load(reader);

            User user = new()
            {
                Name = table.Rows[0]["UserName", DataRowVersion.Current].ToString(),
                Patronymic = table.Rows[0]["UserPatronymic", DataRowVersion.Current].ToString(),
                Surname = table.Rows[0]["UserSurname", DataRowVersion.Current].ToString(),
                Login = table.Rows[0]["UserLogin", DataRowVersion.Current].ToString(),
                Password = table.Rows[0]["UserPassword", DataRowVersion.Current].ToString(),
                RoleId = Convert.ToInt32(table.Rows[0]["RoleID", DataRowVersion.Current]),
                UserId = Convert.ToInt32(table.Rows[0]["UserID", DataRowVersion.Current]),
            };
            return user;
        }

        //Перенос данных пользователя в статический класс
        public static void TransferUserData(User user)
        {
            UserDataBus.Name = user.Name;
            UserDataBus.Patronymic = user.Patronymic;
            UserDataBus.Surname = user.Surname;
            UserDataBus.Login = user.Login;
            UserDataBus.Password = user.Password;
            UserDataBus.RoleId = user.RoleId;
            UserDataBus.UserId = user.UserId;
        }

        //Получение товаров
        public static List<Product> GetProductsData(string searchQuery)
        {
            SqlConnection connection = new(ConnectionString);
            connection.Open();

            string query = $"SELECT * FROM ExamProduct {searchQuery}";
            SqlCommand command = new(query, connection);

            var reader = command.ExecuteReader();

            List<Product> products = new();
            while (reader.Read())
            {
                Product product = new()
                {
                    Article = reader["ProductArticleNumber"].ToString(),
                    Name = reader["ProductName"].ToString(),
                    Description = reader["ProductDescription"].ToString(),
                    Category = reader["ProductCategory"].ToString(),
                    Manufacturer = reader["ProductManufacturer"].ToString(),
                    Cost = Convert.ToDouble(reader["ProductCost"]),
                    DiscountAmount = Convert.ToInt32(reader["ProductDiscountAmount"]),
                    QuantityInStock = Convert.ToInt32(reader["ProductQuantityInStock"]),
                    Status = reader["ProductStatus"].ToString(),
                };
                products.Add(product);
            }
            return products;
        }

        //Получение количества товаров
        public static int GetProductsCount()
        {
            SqlConnection connection = new(ConnectionString);
            connection.Open();

            string query = $"SELECT * FROM ExamProduct";
            SqlCommand command = new(query, connection);

            var reader = command.ExecuteReader();

            int count = 0;
            while (reader.Read())
            {
                count++;
            }
            return count;
        }

        //Метод добавляет запись о заказе
        public static void InsertExamOrder(Order order)
        {
            using SqlConnection connection = new(ConnectionString);
            connection.Open();

            string query = $"INSERT INTO ExamOrder(UserID, OrderStatus, OrderDate, OrderDeliveryDate, OrderPickupPoint, OrderPickupCode) VALUES(@userID, @orderStatus, @orderDate, @orderDeliveryDate, @orderPickupPoint, @orderPickupCode);";
            SqlCommand sqlCommand = new(query, connection);

            sqlCommand.Parameters.AddWithValue("@userID", order.UserID != 0 ? UserDataBus.UserId : DBNull.Value);
            sqlCommand.Parameters.AddWithValue("@orderStatus", order.OrderStatus);
            sqlCommand.Parameters.AddWithValue("@orderDate", order.OrderDate);
            sqlCommand.Parameters.AddWithValue("@orderDeliveryDate", order.OrderDeliveryDate);
            sqlCommand.Parameters.AddWithValue("@orderPickupPoint", order.OrderPickupPoint);
            sqlCommand.Parameters.AddWithValue("@orderPickupCode", order.OrderPickupCode);
            sqlCommand.ExecuteNonQuery();
        }

        //Метод добавляет запись 
        public static void InsertExamOrderProduct(int OrderId, string article, int amount)
        {
            using SqlConnection connection = new(ConnectionString);
            connection.Open();

            string query = $"INSERT INTO ExamOrderProduct(OrderID, ProductArticleNumber, Amount) VALUES(@OrderID, @article, @amount);";
            SqlCommand sqlCommand = new(query, connection);
            sqlCommand.Parameters.AddWithValue("@OrderID", OrderId);
            sqlCommand.Parameters.AddWithValue("@article", article);
            sqlCommand.Parameters.AddWithValue("@amount", amount);

            sqlCommand.ExecuteNonQuery();
        }

        public static int GetLastOrderID()
        {
            using SqlConnection connection = new(ConnectionString);
            connection.Open();

            string query = $"SELECT Max(OrderID) FROM ExamOrder";
            SqlCommand sqlCommand = new(query, connection);

            return Convert.ToInt32(sqlCommand.ExecuteScalar());
        }

        public static int GetPickupPoint()
        {
            using SqlConnection connection = new(ConnectionString);
            connection.Open();

            string query = $"SELECT TOP 10 OrderPickupPoint FROM ExamPickupPoint ORDER BY NEWID()";
            SqlCommand sqlCommand = new(query, connection);

            return Convert.ToInt32(sqlCommand.ExecuteScalar());
        }

        public static int GetPickupCode()
        {
            bool isCodeUnique = false;
            while (!isCodeUnique)
            {
                using SqlConnection connection = new(ConnectionString);
                connection.Open();

                Random random = new();
                int code = random.Next(9999);

                string query = $"SELECT OrderPickupCode FROM ExamOrder WHERE OrderPickupCode = @code";
                SqlCommand sqlCommand = new(query, connection);
                sqlCommand.Parameters.AddWithValue("@code", code);

                if (Convert.ToInt32(sqlCommand.ExecuteScalar()) == 0)
                {
                    isCodeUnique = true;
                    return code;
                }
            }
            throw new Exception("Не удалось сгенерировать код для выдачи товара");
        }

        public static int GetProductAmount()
        {
            using SqlConnection connection = new(ConnectionString);
            connection.Open();

            string query = $"SELECT COUNT(*) ";
            SqlCommand sqlCommand = new(query, connection);

            return Convert.ToInt32(sqlCommand.ExecuteScalar());
        }
    }
}
