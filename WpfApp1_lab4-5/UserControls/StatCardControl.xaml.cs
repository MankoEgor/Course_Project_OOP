using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WpfApp1_lab4_5.UserControls
{
    public partial class StatCardControl : UserControl
    {
        public StatCardControl()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(
                nameof(Title),
                typeof(string),
                typeof(StatCardControl),
                new PropertyMetadata("Title"));

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(
                nameof(Value),
                typeof(string),
                typeof(StatCardControl),
                new PropertyMetadata("0"),
                ValidateValue);

        public static readonly DependencyProperty CardBackgroundProperty =
            DependencyProperty.Register(
                nameof(CardBackground),
                typeof(Brush),
                typeof(StatCardControl),
                new PropertyMetadata(Brushes.White));

        public static readonly DependencyProperty ValueBrushProperty =
            DependencyProperty.Register(
                nameof(ValueBrush),
                typeof(Brush),
                typeof(StatCardControl),
                new FrameworkPropertyMetadata(
                    Brushes.Black,
                    null,
                    CoerceValueBrush));

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public string Value
        {
            get => (string)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public Brush CardBackground
        {
            get => (Brush)GetValue(CardBackgroundProperty);
            set => SetValue(CardBackgroundProperty, value);
        }

        public Brush ValueBrush
        {
            get => (Brush)GetValue(ValueBrushProperty);
            set => SetValue(ValueBrushProperty, value);
        }

        private static bool ValidateValue(object value)
        {
            return value is string s && !string.IsNullOrWhiteSpace(s);
        }

        private static object CoerceValueBrush(DependencyObject d, object baseValue)
        {
            return baseValue ?? Brushes.Black;
        }
    }
}