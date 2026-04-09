using System.Windows;
using System.Windows.Controls;
using WpfApp1_lab4_5.Models;
using WpfApp1_lab4_5.Services;

namespace WpfApp1_lab4_5
{
    public partial class RoomEditWindow : Window
    {
        private readonly RoomService _roomService = new();
        private readonly Room? _existingRoom; // null = добавление, не null = редактирование
        public bool Saved { get; private set; } = false;

        // Добавление нового номера
        public RoomEditWindow()
        {
            InitializeComponent();
            TxtWindowTitle.Text = "Добавить номер";
            CmbCategory.SelectedIndex = 0;
            ChkAvailable.IsChecked = true;
        }

        // Редактирование существующего
        public RoomEditWindow(Room room) : this()
        {
            _existingRoom = room;
            TxtWindowTitle.Text = "Редактировать номер";
            FillFields(room);
        }

        private void FillFields(Room room)
        {
            TxtShortName.Text = room.ShortName;
            TxtFullName.Text = room.FullName;
            TxtDescription.Text = room.Description;
            TxtPrice.Text = room.PricePerNight.ToString();
            TxtFloor.Text = room.Floor.ToString();
            TxtArea.Text = room.Area.ToString();
            TxtCapacity.Text = room.Capacity.ToString();
            TxtRating.Text = room.Rating.ToString();
            TxtAmenities.Text = string.Join(", ", room.Amenities);
            ChkAvailable.IsChecked = room.IsAvailable;

            // Выбираем категорию
            foreach (ComboBoxItem item in CmbCategory.Items)
            {
                if (item.Content.ToString() == room.Category)
                {
                    CmbCategory.SelectedItem = item;
                    break;
                }
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // Валидация
            if (string.IsNullOrWhiteSpace(TxtShortName.Text) ||
                string.IsNullOrWhiteSpace(TxtFullName.Text) ||
                string.IsNullOrWhiteSpace(TxtPrice.Text))
            {
                TxtError.Text = "Заполните обязательные поля: название и цена.";
                return;
            }

            if (!decimal.TryParse(TxtPrice.Text, out var price) || price <= 0)
            {
                TxtError.Text = "Введите корректную цену.";
                return;
            }

            if (!int.TryParse(TxtFloor.Text, out var floor))
            {
                TxtError.Text = "Введите корректный этаж.";
                return;
            }

            if (!double.TryParse(TxtArea.Text, out var area))
            {
                TxtError.Text = "Введите корректную площадь.";
                return;
            }

            if (!int.TryParse(TxtCapacity.Text, out var capacity))
            {
                TxtError.Text = "Введите корректное количество гостей.";
                return;
            }

            if (!double.TryParse(TxtRating.Text, out var rating) || rating < 0 || rating > 5)
            {
                TxtError.Text = "Рейтинг должен быть от 0 до 5.";
                return;
            }

            var amenities = TxtAmenities.Text
                .Split(',', System.StringSplitOptions.RemoveEmptyEntries)
                .Select(a => a.Trim())
                .Where(a => !string.IsNullOrEmpty(a))
                .ToList();

            var category = (CmbCategory.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Одноместный";

            var all = _roomService.GetAll();

            if (_existingRoom == null)
            {
                // Добавляем новый
                var newRoom = new Room
                {
                    Id = all.Count > 0 ? all.Max(r => r.Id) + 1 : 1,
                    ShortName = TxtShortName.Text.Trim(),
                    FullName = TxtFullName.Text.Trim(),
                    Description = TxtDescription.Text.Trim(),
                    Category = category,
                    PricePerNight = price,
                    Floor = floor,
                    Area = area,
                    Capacity = capacity,
                    Rating = rating,
                    IsAvailable = ChkAvailable.IsChecked == true,
                    Amenities = amenities,
                    Images = new List<string>()
                };
                all.Add(newRoom);
            }
            else
            {
                // Редактируем существующий
                var existing = all.FirstOrDefault(r => r.Id == _existingRoom.Id);
                if (existing != null)
                {
                    existing.ShortName = TxtShortName.Text.Trim();
                    existing.FullName = TxtFullName.Text.Trim();
                    existing.Description = TxtDescription.Text.Trim();
                    existing.Category = category;
                    existing.PricePerNight = price;
                    existing.Floor = floor;
                    existing.Area = area;
                    existing.Capacity = capacity;
                    existing.Rating = rating;
                    existing.IsAvailable = ChkAvailable.IsChecked == true;
                    existing.Amenities = amenities;
                }
            }

            _roomService.Save(all);
            Saved = true;
            this.Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}