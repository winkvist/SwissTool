// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AccentColorConverter.cs" company="Fredrik Winkvist">
//   Copyright (c) Fredrik Winkvist. All rights reserved.
// </copyright>
// <summary>
//   Defines the AccentColorConverter type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SwissTool.Application.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;
    using System.Windows.Media;

    /// <summary>
    /// The accent color converter.
    /// </summary>
    public class AccentColorConverter : IValueConverter
    {
        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }

            var brushConverter = new BrushConverter();

            var accent = value.ToString();

            switch (accent)
            {
                case "Amber":
                    return brushConverter.ConvertFrom("#CCF0A30A");

                case "Blue":
                    return brushConverter.ConvertFrom("#CC119EDA");

                case "Brown":
                    return brushConverter.ConvertFrom("#CC825A2C");

                case "Cobalt":
                    return brushConverter.ConvertFrom("#FF003BB0");

                case "Crimson":
                    return brushConverter.ConvertFrom("#CCA20025");

                case "Cyan":
                    return brushConverter.ConvertFrom("#CC1BA1E2");

                case "Emerald":
                    return brushConverter.ConvertFrom("#CC008A00");

                case "Green":
                    return brushConverter.ConvertFrom("#CC60A917");

                case "Indigo":
                    return brushConverter.ConvertFrom("#CC6A00FF");

                case "Lime":
                    return brushConverter.ConvertFrom("#CCA4C400");

                case "Magenta":
                    return brushConverter.ConvertFrom("#CCD80073");

                case "Mauve":
                    return brushConverter.ConvertFrom("#CC76608A");

                case "Olive":
                    return brushConverter.ConvertFrom("#CC6D8764");

                case "Orange":
                    return brushConverter.ConvertFrom("#CCFA6800");

                case "Pink":
                    return brushConverter.ConvertFrom("#CCF472D0");

                case "Purple":
                    return brushConverter.ConvertFrom("#CC6459DF");

                case "Red":
                    return brushConverter.ConvertFrom("#CCE51400");

                case "Sienna":
                    return brushConverter.ConvertFrom("#CCA0522D");

                case "Steel":
                    return brushConverter.ConvertFrom("#CC647687");

                case "Taupe":
                    return brushConverter.ConvertFrom("#CC87794E");

                case "Teal":
                    return brushConverter.ConvertFrom("#CC00ABA9");

                case "Violet":
                    return brushConverter.ConvertFrom("#CCAA00FF");

                case "Yellow":
                    return brushConverter.ConvertFrom("#CCFEDE06");
            }

            return null;
        }

        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">The value that is produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        /// <exception cref="System.NotImplementedException">Not implemented.</exception>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
