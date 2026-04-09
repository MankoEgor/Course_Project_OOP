using System.IO;
using System.Text.Json;
using WpfApp1_lab4_5.Models;

namespace WpfApp1_lab4_5.Services
{
    public class RoomService
    {
        private readonly string _path = "data/rooms.json";
        private List<Room> _rooms = new();

        public List<Room> GetAll()
        {
            if (!File.Exists(_path)) return new List<Room>();
            var json = File.ReadAllText(_path);
            _rooms = JsonSerializer.Deserialize<List<Room>>(json) ?? new List<Room>();
            return _rooms;
        }

        public void Save(List<Room> rooms)
        {
            var json = JsonSerializer.Serialize(rooms, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_path, json);
        }

        public List<Room> Filter(string category, decimal maxPrice, bool onlyAvailable)
        {
            var all = GetAll();
            return all.Where(r =>
                (category == "Все" || r.Category == category) &&
                r.PricePerNight <= maxPrice &&
                (!onlyAvailable || r.IsAvailable)
            ).ToList();
        }

        public List<Room> Search(string query)
        {
            var all = GetAll();
            query = query.ToLower();
            return all.Where(r =>
                r.ShortName.ToLower().Contains(query) ||
                r.FullName.ToLower().Contains(query) ||
                r.Category.ToLower().Contains(query)
            ).ToList();
        }
    }
}