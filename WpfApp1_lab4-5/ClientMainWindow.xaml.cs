using System.Windows;
using System.Windows.Controls;
using WpfApp1_lab4_5.Models;
using WpfApp1_lab4_5.Services;

namespace WpfApp1_lab4_5
{
    public partial class ClientMainWindow : Window
    {
        private readonly RoomService _roomService = new();
        private List<Room> _allRooms = new();
        public static User? CurrentUser { get; private set; } = null;

        public ClientMainWindow()
        {
            InitializeComponent();
            LoadRooms();
        }

        public ClientMainWindow(User user) : this()
        {
            SetUser(user);
        }

        private void LoadRooms()
        {
            _allRooms = _roomService.GetAll();
            RoomsPanel.ItemsSource = _allRooms;
        }

        // Применяем все фильтры и поиск вместе
        private void ApplyFilters()
        {
            // Защита от вызова до инициализации
            if (FilterPrice == null || FilterCategory == null || FilterAvailable == null || SearchBox == null)
                return;

            var category = (FilterCategory.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Все";
            var maxPrice = (decimal)FilterPrice.Value;
            var onlyAvailable = FilterAvailable.IsChecked == true;
            var query = SearchBox.Text.Trim().ToLower();

            var result = _allRooms.Where(r =>
                (category == "Все" || r.Category == category) &&
                r.PricePerNight <= maxPrice &&
                (!onlyAvailable || r.IsAvailable) &&
                (string.IsNullOrEmpty(query) ||
                 r.ShortName.ToLower().Contains(query) ||
                 r.FullName.ToLower().Contains(query) ||
                 r.Category.ToLower().Contains(query))
            ).ToList();

            RoomsPanel.ItemsSource = result;
        }

        private void Filter_Changed(object sender, RoutedEventArgs e) => ApplyFilters();
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e) => ApplyFilters();

        private void BtnResetFilters_Click(object sender, RoutedEventArgs e)
        {
            FilterCategory.SelectedIndex = 0;
            FilterPrice.Value = 20000;
            FilterAvailable.IsChecked = false;
            SearchBox.Text = "";
            RoomsPanel.ItemsSource = _allRooms;
        }

        // Вход
        public void SetUser(User user)
        {
            CurrentUser = user;
            TxtUserName.Text = user.FullName;
            TxtUserName.Visibility = Visibility.Visible;
            BtnLogout.Visibility = Visibility.Visible;
            BtnLogin.Visibility = Visibility.Collapsed;
            BtnRegister.Visibility = Visibility.Collapsed;

            // Кнопка добавить — только для админа
            if (user.Role == "admin")
                BtnAddRoom.Visibility = Visibility.Visible;
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            new LoginWindow().ShowDialog();
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            new RegWindow().ShowDialog();
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            CurrentUser = null;
            TxtUserName.Visibility = Visibility.Collapsed;
            BtnLogout.Visibility = Visibility.Collapsed;
            BtnLogin.Visibility = Visibility.Visible;
            BtnRegister.Visibility = Visibility.Visible;
            BtnAddRoom.Visibility = Visibility.Collapsed;
        }

        private void BtnDetails_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int id)
            {
                var room = _allRooms.FirstOrDefault(r => r.Id == id);
                if (room != null)
                    new RoomDetailWindow(room, CurrentUser).ShowDialog();
            }
        }

        private void BtnAddRoom_Click(object sender, RoutedEventArgs e)
        {
            // Откроем окно добавления — сделаем на следующем шаге
        }

        // Перезагрузить номера после добавления/редактирования
        public void RefreshRooms()
        {
            LoadRooms();
            ApplyFilters();
        }
    }
}