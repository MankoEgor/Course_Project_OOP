using System.Windows;
using WpfApp1_lab4_5.Models;

namespace WpfApp1_lab4_5
{
    /// <summary>
    /// Логика взаимодействия для AdminMainWindow.xaml
    /// </summary>
    public partial class AdminMainWindow : Window
    {
        public AdminMainWindow(User user)
        {
            InitializeComponent();
            Title = $"Админ — {user.FullName}";
        }
    }
}
