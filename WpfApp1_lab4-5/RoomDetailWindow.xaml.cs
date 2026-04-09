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

        private void FillData()
        {
            Title = _room.ShortName;
            TxtCategory.Text = _room.Category;
            TxtFullName.Text = _room.FullName;
            TxtRating.Text = _room.Rating.ToString("F1");
            TxtFloor.Text = $"{_room.Floor} этаж";
            TxtArea.Text = $"{_room.Area} м²";
            TxtCapacity.Text = $"{_room.Capacity} чел.";
            TxtDescription.Text = _room.Description;
            TxtPrice.Text = _room.PricePerNight.ToString("N0");

            // Статус
            if (_room.IsAvailable)
            {
                TxtAvailable.Text = "● Свободен";
                TxtAvailable.Foreground = new SolidColorBrush(Color.FromRgb(74, 93, 78));
                BadgeAvailable.Background = new SolidColorBrush(Color.FromRgb(234, 243, 234));
            }
            else
            {
                TxtAvailable.Text = "● Занят";
                TxtAvailable.Foreground = new SolidColorBrush(Color.FromRgb(204, 68, 68));
                BadgeAvailable.Background = new SolidColorBrush(Color.FromRgb(252, 235, 235));
            }

            // Удобства
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
                MessageBox.Show("Для бронирования необходимо войти в аккаунт.",
                    "Требуется вход", MessageBoxButton.OK, MessageBoxImage.Information);
                new LoginWindow().ShowDialog();
                return;
            }

            if (!_room.IsAvailable)
            {
                MessageBox.Show("Этот номер уже занят.", "Недоступно",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Окно бронирования — сделаем позже
            MessageBox.Show($"Бронирование номера «{_room.ShortName}» — в разработке.",
                "Скоро", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            // Окно редактирования — сделаем позже
            MessageBox.Show("Редактирование — в разработке.",
                "Скоро", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                $"Удалить номер «{_room.ShortName}»?",
                "Подтверждение",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                var all = _roomService.GetAll();
                all.RemoveAll(r => r.Id == _room.Id);
                _roomService.Save(all);

                MessageBox.Show("Номер удалён.", "Готово",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                this.Close();
            }
        }
    }
}