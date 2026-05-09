using System.Windows;
using System.Windows.Input;
using System.IO;
using System.Text.Json;
using WpfApp1_lab4_5.Commands;
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

        private readonly Stack<List<Room>> _undoStack = new();
        private readonly Stack<List<Room>> _redoStack = new();

        private readonly Brush _defaultBrush = Brushes.Transparent;

        public ICommand AddRoomCommand { get; }
        public ICommand RefreshRoomsCommand { get; }
        public ICommand DeleteRoomCommand { get; }
        public ICommand UndoCommand { get; }
        public ICommand RedoCommand { get; }

        public AdminMainWindow(User user)
        {
            InitializeComponent();

            AddRoomCommand = new RelayCommand(_ => ExecuteAddRoom());
            RefreshRoomsCommand = new RelayCommand(_ => RefreshAll());
            DeleteRoomCommand = new RelayCommand(_ => ExecuteDeleteRoom(), _ => GetSelectedRoom() != null);
            UndoCommand = new RelayCommand(_ => Undo(), _ => _undoStack.Count > 0);
            RedoCommand = new RelayCommand(_ => Redo(), _ => _redoStack.Count > 0);

            DataContext = this;

            _user = user;

            Title = TryFindResource("AdminPanel")?.ToString() ?? "Admin Panel";
            TxtAdminName.Text = user.FullName;
            TxtCurrentDate.Text = DateTime.Now.ToString("dd.MM.yyyy");

            LoadRooms();
            UpdateDashboard();
            ShowDashboard();
        }

        private string GetText(string key, string fallback = "")
        {
            return TryFindResource(key)?.ToString() ?? fallback;
        }


        private List<Room> CloneRooms(List<Room> rooms)
        {
            return rooms.Select(r => new Room
            {
                Id = r.Id,
                ShortName = r.ShortName,
                FullName = r.FullName,
                Description = r.Description,
                Category = r.Category,
                PricePerNight = r.PricePerNight,
                Floor = r.Floor,
                Area = r.Area,
                Capacity = r.Capacity,
                Rating = r.Rating,
                IsAvailable = r.IsAvailable,
                Amenities = r.Amenities != null ? new List<string>(r.Amenities) : new List<string>(),
                Images = r.Images != null ? new List<string>(r.Images) : new List<string>()
            }).ToList();
        }

        private void SaveUndoState()
        {
            _undoStack.Push(CloneRooms(_allRooms));
            _redoStack.Clear();
            CommandManager.InvalidateRequerySuggested();
        }

        private void Undo()
        {
            if (_undoStack.Count == 0)
                return;

            _redoStack.Push(CloneRooms(_allRooms));
            _allRooms = _undoStack.Pop();
            _roomService.Save(_allRooms);
            RefreshAll();
            CommandManager.InvalidateRequerySuggested();
        }

        private void Redo()
        {
            if (_redoStack.Count == 0)
                return;

            _undoStack.Push(CloneRooms(_allRooms));
            _allRooms = _redoStack.Pop();
            _roomService.Save(_allRooms);
            RefreshAll();
            CommandManager.InvalidateRequerySuggested();
        }

        private void ExecuteAddRoom()
        {
            SaveUndoState();

            var window = new RoomEditWindow();
            window.Owner = this;
            window.ShowDialog();

            if (window.Saved)
            {
                RefreshAll();
            }
            else if (_undoStack.Count > 0)
            {
                _undoStack.Pop();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private void ExecuteDeleteRoom()
        {
            var selectedRoom = GetSelectedRoom();

            if (selectedRoom == null)
            {
                MessageBox.Show(
                    GetText("SelectRoomToDelete", "Select a room to delete."),
                    GetText("WarningTitle", "Warning"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                string.Format(GetText("DeleteRoomConfirm", "Delete room “{0}”?"), selectedRoom.ShortName),
                GetText("ConfirmTitle", "Confirmation"),
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                SaveUndoState();

                var all = _roomService.GetAll();
                all.RemoveAll(r => r.Id == selectedRoom.Id);
                _roomService.Save(all);
                RefreshAll();
                CommandManager.InvalidateRequerySuggested();
            }
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
            ProfileView.Visibility = Visibility.Collapsed;
        }

        private void LoadProfileData()
        {
            TxtProfileFullName.Text = _user.FullName;
            TxtProfileEmail.Text = _user.Email;
            TxtProfileRole.Text = _user.Role == "admin"
                ? GetText("RoleAdmin", "Administrator")
                : GetText("RoleClient", "Client");
        }

        private void BtnProfile_Click(object sender, RoutedEventArgs e)
        {
            ShowProfile();
        }

        private void BtnSaveProfile_Click(object sender, RoutedEventArgs e)
        {
            var newName = TxtProfileFullName.Text.Trim();

            if (string.IsNullOrWhiteSpace(newName))
            {
                MessageBox.Show(
                    GetText("FillAllFieldsError", "Fill in all fields."),
                    GetText("WarningTitle", "Warning"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            var path = "Data/users.json";
            if (!File.Exists(path))
                return;

            var json = File.ReadAllText(path);
            var users = JsonSerializer.Deserialize<List<User>>(json) ?? new List<User>();

            var current = users.FirstOrDefault(u => u.Id == _user.Id);
            if (current != null)
            {
                current.FullName = newName;
                _user.FullName = newName;
                TxtAdminName.Text = newName;

                var updatedJson = JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(path, updatedJson);

                MessageBox.Show(
                    GetText("ProfileSaved", "Profile data saved."),
                    GetText("DoneTitle", "Done"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        private void ShowProfile()
        {
            HideAllViews();
            ProfileView.Visibility = Visibility.Visible;
            TxtPageTitle.Text = GetText("Profile", "Profile");
            TxtPageSubtitle.Text = GetText("PageSubtitleProfile", "");
            SetActiveMenuButton(BtnProfile);
            LoadProfileData();
        }

        private void ResetMenuButtons()
        {
            BtnDashboard.Background = _defaultBrush;
            BtnRooms.Background = _defaultBrush;
            BtnUsers.Background = _defaultBrush;
            BtnBookings.Background = _defaultBrush;
            BtnProfile.Background = _defaultBrush;
        }

        private void SetActiveMenuButton(Button activeButton)
        {
            ResetMenuButtons();
            activeButton.Background = (Brush)(TryFindResource("BrushAccent") ?? Brushes.SaddleBrown);
        }

        private void ShowDashboard()
        {
            HideAllViews();
            DashboardView.Visibility = Visibility.Visible;
            TxtPageTitle.Text = (string)FindResource("Dashboard");
            TxtPageSubtitle.Text = (string)FindResource("PageSubtitleDashboard");
            SetActiveMenuButton(BtnDashboard);
        }

        private void ShowRooms()
        {
            HideAllViews();
            RoomsView.Visibility = Visibility.Visible;
            TxtPageTitle.Text = (string)FindResource("RoomsManagement");
            TxtPageSubtitle.Text = (string)FindResource("PageSubtitleRooms");
            SetActiveMenuButton(BtnRooms);
        }

        private void ShowUsers()
        {
            HideAllViews();
            UsersView.Visibility = Visibility.Visible;
            TxtPageTitle.Text = (string)FindResource("Users");
            TxtPageSubtitle.Text = (string)FindResource("PageSubtitleUsers");
            SetActiveMenuButton(BtnUsers);
        }

        private void ShowBookings()
        {
            HideAllViews();
            BookingsView.Visibility = Visibility.Visible;
            TxtPageTitle.Text = (string)FindResource("Bookings");
            TxtPageSubtitle.Text = (string)FindResource("PageSubtitleBookings");
            SetActiveMenuButton(BtnBookings);
        }

        private void RefreshAll()
        {
            LoadRooms();
            UpdateDashboard();
            ApplyRoomSearch();
            CommandManager.InvalidateRequerySuggested();
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

        private void UpdateLocalizedTexts()
        {
            Title = TryFindResource("AdminPanel")?.ToString() ?? "Admin Panel";

            if (DashboardView.Visibility == Visibility.Visible)
                ShowDashboard();
            else if (RoomsView.Visibility == Visibility.Visible)
                ShowRooms();
            else if (UsersView.Visibility == Visibility.Visible)
                ShowUsers();
            else if (BookingsView.Visibility == Visibility.Visible)
                ShowBookings();
            else if (ProfileView.Visibility == Visibility.Visible)
                ShowProfile();
        }

        private void BtnRu_Click(object sender, RoutedEventArgs e)
        {
            LanguageService.ChangeLanguage("ru");
            UpdateLocalizedTexts();
        }

        private void BtnEn_Click(object sender, RoutedEventArgs e)
        {
            LanguageService.ChangeLanguage("en");
            UpdateLocalizedTexts();
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
            ExecuteAddRoom();
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            var selectedRoom = GetSelectedRoom();

            if (selectedRoom == null)
            {
                MessageBox.Show(
                    GetText("SelectRoomToEdit", "Select a room to edit."),
                    GetText("WarningTitle", "Warning"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            SaveUndoState();

            var window = new RoomEditWindow(selectedRoom);
            window.Owner = this;
            window.ShowDialog();

            if (window.Saved)
            {
                RefreshAll();
            }
            else if (_undoStack.Count > 0)
            {
                _undoStack.Pop();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            ExecuteDeleteRoom();
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshAll();
        }

        private void RoomsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CommandManager.InvalidateRequerySuggested();
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


        private void BtnLightTheme_Click(object sender, RoutedEventArgs e)
        {
            ThemeService.ApplyTheme("Light");
            UpdateLocalizedTexts();
            if (ProfileView.Visibility == Visibility.Visible)
                LoadProfileData();
        }

        private void BtnDarkTheme_Click(object sender, RoutedEventArgs e)
        {
            ThemeService.ApplyTheme("Dark");
            UpdateLocalizedTexts();
            if (ProfileView.Visibility == Visibility.Visible)
                LoadProfileData();
        }
    }
}