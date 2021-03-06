﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using TCC.Data;
using TCC.Data.Databases;

namespace TCC.Converters
{
    class LocationToMarkerPositionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var loc = (Location)value;
            var point = MapDatabase.GetMarkerPosition(loc);
            Console.WriteLine("{0},{1}", point.X, point.Y);
            return new Thickness(point.Y,point.X, 0, 0);

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
