using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using WpfApp1_lab4_5.Models;

namespace WpfApp1_lab4_5
{
    public partial class LoginWindow : Window
    {
        private List<User> _users = new();

        public LoginWindow()
        {
            InitializeComponent();
            LoadUsers();
        }

        private void LoadUsers()
        {
            var path = "data/users.json";
            if (!File.Exists(path))
            {
                _users = new List<User>();
                return;
            }

            var json = File.ReadAllText(path);
            _users = JsonSerializer.Deserialize<List<User>>(json) ?? new List<User>();
        }

        private string GetText(string key, string fallback = "")
        {
            return TryFindResource(key)?.ToString() ?? fallback;
        }

        private void SwitchToRegister(object sender, MouseButtonEventArgs e)
        {
            new RegWindow().Show();
            Close();
        }

        private void BtnAction_Click(object sender, RoutedEventArgs e)
        {
            var email = TxtLogin.Text.Trim();
            var password = TxtPassword.Password;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                TxtError.Text = GetText("FillAllFieldsError", "Fill in all fields.");
                return;
            }

            var user = _users.FirstOrDefault(u =>
                u.Email == email && u.Password == password);

            if (user == null)
            {
                TxtError.Text = GetText("InvalidCredentialsError", "Invalid email or password.");
                return;
            }

            if (user.Role == "admin")
            {
                new AdminMainWindow(user).Show();
            }
            else
            {
                new ClientMainWindow(user).Show();
            }

            Close();
        }
    }
}