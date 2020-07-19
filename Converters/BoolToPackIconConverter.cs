using MaterialDesignThemes.Wpf;
using System;
using System.Globalization;
using System.Windows.Data;

namespace ElectricShimmer.Converters
{
    class BoolToPackIconConverter :IValueConverter
    {
        public PackIconKind FalseValue { get; set; }

        public PackIconKind TrueValue { get; set; }

        public BoolToPackIconConverter()
        {
            FalseValue = PackIconKind.Close;
            TrueValue = PackIconKind.Check;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool && ((bool)value) == false)
            {
                return FalseValue;
            }
            else
            {
                return TrueValue;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
