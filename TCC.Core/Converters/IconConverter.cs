﻿using System;
using System.Globalization;
using System.Windows.Data;
namespace TCC.Converters
{
    public class IconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string iconName = "unknown";
            if (value != null)
            {
                if (value.ToString() != "")
                {
                    iconName = value.ToString();
                    iconName = iconName.Replace(".", "/");
                }
            }
            return Environment.CurrentDirectory + "/resources/images/" + iconName + ".png";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
