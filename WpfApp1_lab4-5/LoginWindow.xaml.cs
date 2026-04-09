using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using WpfApp1_lab4_5.Models;

namespace WpfApp1_lab4_5
{
    public partial class LoginWindow : Window
    {
        private List<User> _users;

        public LoginWindow()
        {
            InitializeComponent();
            LoadUsers();
        }

        private void LoadUsers()
        {
            var path = "Data/users.json";
            if (!File.Exists(path))
            {
                _users = new List<User>();
                return;
            }
            var json = File.ReadAllText(path);
            _users = JsonSerializer.Deserialize<List<User>>(json) ?? new List<User>();
        }

        private void SwitchToRegister(object sender, MouseButtonEventArgs e)
        {
            new RegWindow().Show();
            this.Close();
        }

        private void BtnAction_Click(object sender, RoutedEventArgs e)
        {
            var email = TxtLogin.Text;
            var password = TxtPassword.Password;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                TxtError.Text = "Заполните все поля.";
                return;
            }

            var user = _users.FirstOrDefault(u =>
                u.Email == email && u.Password == password);

            if (user == null)
            {
                TxtError.Text = "Неверный email или пароль.";
                return;
            }

            // Открываем нужное окно по роли
            if (user.Role == "admin")
            {
                new AdminMainWindow(user).Show();
            }
            else
            {
                new ClientMainWindow(user).Show();
            }

            this.Close();
        }
    }
}