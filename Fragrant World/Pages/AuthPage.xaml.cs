using Fragrant_World.Classes;
using System.Windows;
using System.Windows.Controls;

namespace Fragrant_World.Pages
{
    /// <summary>
    /// Interaction logic for AuthPage.xaml
    /// </summary>
    public partial class AuthPage : Page
    {
        public AuthPage()
        {
            InitializeComponent();
            Style = (Style)FindResource(typeof(Page));
        }

        private void AuthButton_Click(object sender, RoutedEventArgs e)
        {
            if (!DataAccessLayer.IsUser(LoginTextBox.Text, PasswordBox.Password))
                MessageBox.Show("Пользователь не найден!", Title="Ошибка авторизации", MessageBoxButton.OK, MessageBoxImage.Error);
            else
            {
                DataAccessLayer.TransferUserData(DataAccessLayer.GetUserData(LoginTextBox.Text, PasswordBox.Password));
                this.NavigationService.Navigate(new StorePage());
            }
        }

        private void GuestButton_Click(object sender, RoutedEventArgs e)
        {
            DataAccessLayer.TransferUserData(new User());
            this.NavigationService.Navigate(new StorePage());
        }
    }
}
