﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ElectricShimmer.Converters
{
    class BoolToVisibilityConverter : IValueConverter
    {
        public Visibility FalseValue { get; set; }

        public Visibility TrueValue { get; set; }

        public BoolToVisibilityConverter()
        {
            FalseValue = Visibility.Collapsed;
            TrueValue = Visibility.Visible;
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
