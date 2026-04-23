using System.Windows;
using System.Windows.Media;
using WpfApp1_lab4_5.Models;
using WpfApp1_lab4_5.Services;

namespace WpfApp1_lab4_5
{
    public partial class RoomDetailWindow : Window
    {
        private readonly Room _room;
        private readonly User? _currentUser;
        private readonly RoomService _roomService = new();

        public RoomDetailWindow(Room room, User? currentUser)
        {
            InitializeComponent();
            _room = room;
            _currentUser = currentUser;
            FillData();
            SetupButtons();
        }

        private string GetText(string key, string fallback = "")
        {
            return TryFindResource(key)?.ToString() ?? fallback;
        }

        private void FillData()
        {
            Title = _room.ShortName;
            TxtCategory.Text = _room.Category;
            TxtFullName.Text = _room.FullName;
            TxtRating.Text = _room.Rating.ToString("F1");
            TxtFloor.Text = string.Format(GetText("FloorFormat", "{0} этаж"), _room.Floor);
            TxtArea.Text = string.Format(GetText("AreaFormat", "{0} м²"), _room.Area);
            TxtCapacity.Text = string.Format(GetText("CapacityFormat", "{0} чел."), _room.Capacity);
            TxtDescription.Text = _room.Description;
            TxtPrice.Text = _room.PricePerNight.ToString("N0");

            if (_room.IsAvailable)
            {
                TxtAvailable.Text = GetText("AvailableStatus", "● Available");
                TxtAvailable.Foreground = new SolidColorBrush(Color.FromRgb(74, 93, 78));
                BadgeAvailable.Background = new SolidColorBrush(Color.FromRgb(234, 243, 234));
            }
            else
            {
                TxtAvailable.Text = GetText("BusyStatus", "● Occupied");
                TxtAvailable.Foreground = new SolidColorBrush(Color.FromRgb(204, 68, 68));
                BadgeAvailable.Background = new SolidColorBrush(Color.FromRgb(252, 235, 235));
            }

            AmenitiesPanel.Children.Clear();

            foreach (var amenity in _room.Amenities)
            {
                var badge = new System.Windows.Controls.Border
                {
                    Background = new SolidColorBrush(Color.FromRgb(245, 245, 240)),
                    CornerRadius = new CornerRadius(6),
                    Padding = new Thickness(10, 4, 10, 4),
                    Margin = new Thickness(0, 0, 8, 8),
                    Child = new System.Windows.Controls.TextBlock
                    {
                        Text = amenity,
                        FontSize = 13,
                        Foreground = new SolidColorBrush(Color.FromRgb(26, 26, 26))
                    }
                };
                AmenitiesPanel.Children.Add(badge);
            }
        }

        private void SetupButtons()
        {
            if (_currentUser?.Role == "admin")
            {
                BtnEdit.Visibility = Visibility.Visible;
                BtnDelete.Visibility = Visibility.Visible;
                BtnBook.Visibility = Visibility.Collapsed;
            }
        }

        private void BtnBook_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser == null)
            {
                MessageBox.Show(
                    GetText("BookingRequiresLogin", "You need to log in."),
                    GetText("LoginRequiredTitle", "Login Required"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                new LoginWindow().ShowDialog();
                return;
            }

            if (!_room.IsAvailable)
            {
                MessageBox.Show(
                    GetText("RoomUnavailable", "This room is unavailable."),
                    GetText("UnavailableTitle", "Unavailable"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            MessageBox.Show(
                string.Format(GetText("BookingInDevelopment", "Booking for room “{0}” is under development."), _room.ShortName),
                GetText("SoonTitle", "Coming Soon"),
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            var editWindow = new RoomEditWindow(_room);
            editWindow.Owner = this;
            editWindow.ShowDialog();

            if (editWindow.Saved)
            {
                MessageBox.Show(
                    GetText("RoomUpdated", "Room updated."),
                    GetText("DoneTitle", "Done"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                Close();
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                string.Format(GetText("DeleteRoomConfirm", "Delete room “{0}”?"), _room.ShortName),
                GetText("ConfirmTitle", "Confirmation"),
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                var all = _roomService.GetAll();
                all.RemoveAll(r => r.Id == _room.Id);
                _roomService.Save(all);

                MessageBox.Show(
                    GetText("RoomDeleted", "Room deleted."),
                    GetText("DoneTitle", "Done"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                Close();
            }
        }
    }
}