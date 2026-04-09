using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WpfApp1_lab4_5.Models;
using WpfApp1_lab4_5.Services;

namespace WpfApp1_lab4_5
{
    public partial class AdminMainWindow : Window
    {
        private readonly RoomService _roomService = new();
        private readonly User _user;
        private List<Room> _allRooms = new();

        private readonly Brush _activeBrush = new SolidColorBrush(Color.FromRgb(166, 124, 82));
        private readonly Brush _defaultBrush = Brushes.Transparent;

        public AdminMainWindow(User user)
        {
            InitializeComponent();
            _user = user;

            Title = $"Админ — {user.FullName}";
            TxtAdminName.Text = user.FullName;
            TxtCurrentDate.Text = DateTime.Now.ToString("dd.MM.yyyy");

            LoadRooms();
            UpdateDashboard();
            ShowDashboard();
        }

        private void LoadRooms()
        {
            _allRooms = _roomService.GetAll();
            RoomsGrid.ItemsSource = _allRooms;
        }

        private void UpdateDashboard()
        {
            int total = _allRooms.Count;
            int available = _allRooms.Count(r => r.IsAvailable);
            int busy = total - available;
            decimal averagePrice = total > 0 ? _allRooms.Average(r => r.PricePerNight) : 0;

            TxtTotalRooms.Text = total.ToString();
            TxtAvailableRooms.Text = available.ToString();
            TxtBusyRooms.Text = busy.ToString();
            TxtAveragePrice.Text = averagePrice.ToString("N0");
        }

        private void HideAllViews()
        {
            DashboardView.Visibility = Visibility.Collapsed;
            RoomsView.Visibility = Visibility.Collapsed;
            UsersView.Visibility = Visibility.Collapsed;
            BookingsView.Visibility = Visibility.Collapsed;
        }

        private void ResetMenuButtons()
        {
            BtnDashboard.Background = _defaultBrush;
            BtnRooms.Background = _defaultBrush;
            BtnUsers.Background = _defaultBrush;
            BtnBookings.Background = _defaultBrush;
        }

        private void SetActiveMenuButton(Button activeButton)
        {
            ResetMenuButtons();
            activeButton.Background = _activeBrush;
        }

        private void ShowDashboard()
        {
            HideAllViews();
            DashboardView.Visibility = Visibility.Visible;
            TxtPageTitle.Text = "Сводка";
            TxtPageSubtitle.Text = "Общая информация по гостиничному комплексу";
            SetActiveMenuButton(BtnDashboard);
        }

        private void ShowRooms()
        {
            HideAllViews();
            RoomsView.Visibility = Visibility.Visible;
            TxtPageTitle.Text = "Управление номерами";
            TxtPageSubtitle.Text = "Добавление, редактирование, удаление и поиск номеров";
            SetActiveMenuButton(BtnRooms);
        }

        private void ShowUsers()
        {
            HideAllViews();
            UsersView.Visibility = Visibility.Visible;
            TxtPageTitle.Text = "Пользователи";
            TxtPageSubtitle.Text = "Управление клиентами и администраторами";
            SetActiveMenuButton(BtnUsers);
        }

        private void ShowBookings()
        {
            HideAllViews();
            BookingsView.Visibility = Visibility.Visible;
            TxtPageTitle.Text = "Бронирования";
            TxtPageSubtitle.Text = "Контроль активных и завершённых броней";
            SetActiveMenuButton(BtnBookings);
        }

        private void RefreshAll()
        {
            LoadRooms();
            UpdateDashboard();
            ApplyRoomSearch();
        }

        private void ApplyRoomSearch()
        {
            if (SearchRoomBox == null)
                return;

            string query = SearchRoomBox.Text.Trim().ToLower();

            if (string.IsNullOrWhiteSpace(query))
            {
                RoomsGrid.ItemsSource = _allRooms;
                return;
            }

            var filtered = _allRooms.Where(r =>
                r.ShortName.ToLower().Contains(query) ||
                r.FullName.ToLower().Contains(query) ||
                r.Category.ToLower().Contains(query))
                .ToList();

            RoomsGrid.ItemsSource = filtered;
        }

        private Room? GetSelectedRoom()
        {
            return RoomsGrid.SelectedItem as Room;
        }

        private void BtnDashboard_Click(object sender, RoutedEventArgs e)
        {
            ShowDashboard();
        }

        private void BtnRooms_Click(object sender, RoutedEventArgs e)
        {
            ShowRooms();
        }

        private void BtnUsers_Click(object sender, RoutedEventArgs e)
        {
            ShowUsers();
        }

        private void BtnBookings_Click(object sender, RoutedEventArgs e)
        {
            ShowBookings();
        }

        private void SearchRoomBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyRoomSearch();
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            var window = new RoomEditWindow();
            window.Owner = this;
            window.ShowDialog();

            if (window.Saved)
                RefreshAll();
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            var selectedRoom = GetSelectedRoom();

            if (selectedRoom == null)
            {
                MessageBox.Show("Выберите номер для редактирования.",
                    "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var window = new RoomEditWindow(selectedRoom);
            window.Owner = this;
            window.ShowDialog();

            if (window.Saved)
                RefreshAll();
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            var selectedRoom = GetSelectedRoom();

            if (selectedRoom == null)
            {
                MessageBox.Show("Выберите номер для удаления.",
                    "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"Удалить номер «{selectedRoom.ShortName}»?",
                "Подтверждение",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                var all = _roomService.GetAll();
                all.RemoveAll(r => r.Id == selectedRoom.Id);
                _roomService.Save(all);
                RefreshAll();
            }
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshAll();
        }

        private void BtnOpenClientView_Click(object sender, RoutedEventArgs e)
        {
            var clientWindow = new ClientMainWindow(_user);
            clientWindow.Show();
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}