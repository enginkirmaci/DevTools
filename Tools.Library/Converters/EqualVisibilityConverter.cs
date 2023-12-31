﻿using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Binding = System.Windows.Data.Binding;

namespace Tools.Library.Converters;

public class EqualVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value != null)
        {
            if (value.Equals(parameter))
            {
                return Visibility.Visible;
            }
            else
            {
                return Visibility.Collapsed;
            }
        }

        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}