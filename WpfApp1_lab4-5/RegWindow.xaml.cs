using System.IO;
using System.Text.Json;
using System.Windows;
using WpfApp1_lab4_5.Models;

namespace WpfApp1_lab4_5
{
    public partial class RegWindow : Window
    {
        private List<User> _users = new();

        public RegWindow()
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

        private void BtnSwitch_Click(object sender, RoutedEventArgs e)
        {
            new LoginWindow().Show();
            Close();
        }

        private void BtnAction_Click(object sender, RoutedEventArgs e)
        {
            var fullName = TxtFullName.Text.Trim();
            var email = TxtLogin.Text.Trim();
            var password = TxtPassword.Password;

            if (string.IsNullOrEmpty(fullName) ||
                string.IsNullOrEmpty(email) ||
                string.IsNullOrEmpty(password))
            {
                TxtError.Text = GetText("FillAllFieldsError", "Fill in all fields.");
                return;
            }

            if (password.Length < 6)
            {
                TxtError.Text = GetText("PasswordMinLengthError", "Password must be at least 6 characters long.");
                return;
            }

            if (_users.Any(u => u.Email == email))
            {
                TxtError.Text = GetText("UserAlreadyExistsError", "A user with this email already exists.");
                return;
            }

            var newUser = new User
            {
                Id = _users.Count > 0 ? _users.Max(u => u.Id) + 1 : 1,
                Email = email,
                Password = password,
                Role = "client",
                FullName = fullName
            };

            _users.Add(newUser);

            var json = JsonSerializer.Serialize(_users, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText("Data/users.json", json);

            MessageBox.Show(
                GetText("RegistrationSuccessMessage", "Registration completed successfully! Please sign in."),
                GetText("DoneTitle", "Done"),
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            new LoginWindow().Show();
            Close();
        }
    }
}