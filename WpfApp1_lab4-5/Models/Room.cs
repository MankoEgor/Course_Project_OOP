using System;
using System.Collections.Generic;
using System.Text;

namespace WpfApp1_lab4_5.Models
{
   public class Room
    {
        public int Id { get; set; }
        public string ShortName { get; set; } = string.Empty;      // "Люкс"
        public string FullName { get; set; } = string.Empty;       // "Номер Люкс с видом на город"
        public string Description { get; set; } = string.Empty;    // полное описание
        public string Category { get; set; } = string.Empty;       // "Одноместный" / "Двухместный" / "Люкс"
        public decimal PricePerNight { get; set; }                  // цена за ночь
        public int Floor { get; set; }                              // этаж
        public double Area { get; set; }                            // площадь м²
        public int Capacity { get; set; }                           // количество гостей
        public double Rating { get; set; }                          // рейтинг 0.0 - 5.0
        public bool IsAvailable { get; set; } = true;              // свободен или нет
        public List<string> Images { get; set; } = new();          // пути к изображениям
        public List<string> Amenities { get; set; } = new();
    }
}
