﻿using System.Globalization;
using System.Windows.Data;
using Binding = System.Windows.Data.Binding;

namespace Tools.Library.Converters;

public class EqualBooleanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value != null)
        {
            if (value.Equals(parameter))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}