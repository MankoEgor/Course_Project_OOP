using System.IO;
using System.Text.Json;
using System.Windows;
using WpfApp1_lab4_5.Models;

namespace WpfApp1_lab4_5
{
    public partial class RegWindow : Window
    {
        private List<User> _users;

        public RegWindow()
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

        private void BtnSwitch_Click(object sender, RoutedEventArgs e)
        {
            new LoginWindow().Show();
            this.Close();
        }

        private void BtnAction_Click(object sender, RoutedEventArgs e)
        {
            var fullName = TxtFullName.Text.Trim();
            var email = TxtLogin.Text;
            var password = TxtPassword.Password;

            // Валидация
            if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                TxtError.Text = "Заполните все поля.";
                return;
            }

            if (password.Length < 6)
            {
                TxtError.Text = "Пароль должен быть не менее 6 символов.";
                return;
            }

            if (_users.Any(u => u.Email == email))
            {
                TxtError.Text = "Пользователь с таким email уже существует.";
                return;
            }

            // Создаём нового пользователя — роль всегда "client"
            var newUser = new User
            {
                Id = _users.Count + 1,
                Email = email,
                Password = password,
                Role = "client",
                FullName = fullName
            };

            _users.Add(newUser);

            var json = JsonSerializer.Serialize(_users, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText("Data/users.json", json);

            // Переходим на вход
            MessageBox.Show("Регистрация прошла успешно! Войдите в аккаунт.", "Готово",
                MessageBoxButton.OK, MessageBoxImage.Information);

            new LoginWindow().Show();
            this.Close();
        }
    }
}