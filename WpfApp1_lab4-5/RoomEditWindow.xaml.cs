using System.Windows;
using System.Windows.Controls;
using WpfApp1_lab4_5.Models;
using WpfApp1_lab4_5.Services;

namespace WpfApp1_lab4_5
{
    public partial class RoomEditWindow : Window
    {
        private readonly RoomService _roomService = new();
        private readonly Room? _existingRoom;
        public bool Saved { get; private set; } = false;

        public RoomEditWindow()
        {
            InitializeComponent();
            TxtWindowTitle.Text = GetText("AddRoomWindow", "Add Room");
            CmbCategory.SelectedIndex = 0;
            ChkAvailable.IsChecked = true;
        }

        public RoomEditWindow(Room room) : this()
        {
            _existingRoom = room;
            TxtWindowTitle.Text = GetText("EditRoomWindow", "Edit Room");
            FillFields(room);
        }

        private string GetText(string key, string fallback = "")
        {
            return TryFindResource(key)?.ToString() ?? fallback;
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

            for (int i = 0; i < CmbCategory.Items.Count; i++)
            {
                if (room.Category == "Одноместный" && i == 0)
                {
                    CmbCategory.SelectedIndex = 0;
                    break;
                }

                if (room.Category == "Двухместный" && i == 1)
                {
                    CmbCategory.SelectedIndex = 1;
                    break;
                }

                if (room.Category == "Люкс" && i == 2)
                {
                    CmbCategory.SelectedIndex = 2;
                    break;
                }
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtShortName.Text) ||
                string.IsNullOrWhiteSpace(TxtFullName.Text) ||
                string.IsNullOrWhiteSpace(TxtPrice.Text))
            {
                TxtError.Text = GetText("RequiredFieldsError", "Fill in required fields.");
                return;
            }

            if (!decimal.TryParse(TxtPrice.Text, out var price) || price <= 0)
            {
                TxtError.Text = GetText("InvalidPriceError", "Invalid price.");
                return;
            }

            if (!int.TryParse(TxtFloor.Text, out var floor))
            {
                TxtError.Text = GetText("InvalidFloorError", "Invalid floor.");
                return;
            }

            if (!double.TryParse(TxtArea.Text, out var area))
            {
                TxtError.Text = GetText("InvalidAreaError", "Invalid area.");
                return;
            }

            if (!int.TryParse(TxtCapacity.Text, out var capacity))
            {
                TxtError.Text = GetText("InvalidCapacityError", "Invalid capacity.");
                return;
            }

            if (!double.TryParse(TxtRating.Text, out var rating) || rating < 0 || rating > 5)
            {
                TxtError.Text = GetText("InvalidRatingError", "Rating must be between 0 and 5.");
                return;
            }

            var amenities = TxtAmenities.Text
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(a => a.Trim())
                .Where(a => !string.IsNullOrEmpty(a))
                .ToList();

            var category = CmbCategory.SelectedIndex switch
            {
                1 => "Двухместный",
                2 => "Люкс",
                _ => "Одноместный"
            };

            var all = _roomService.GetAll();

            if (_existingRoom == null)
            {
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
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}