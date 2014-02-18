using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Markup;
using System.Windows.Data;
using System.Globalization;
using System.Windows;

namespace SingDictionaryWPF
{
    public class RegionConverter : MarkupExtension, IValueConverter
    {
        private static RegionConverter _converter = null;
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (_converter == null)
            {
                _converter = new RegionConverter();
            }
            return _converter;
        }
        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter,
            CultureInfo culture)
        {   
            switch(value as string){
                case "PO":
                    return "Prešovský";
                case "KR":
                    return "Kremnický";
                default:
                    return "Bratislavský";
            }
            //return value; // set breakpoint here to debug your binding
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            CultureInfo culture)
        {
            return value;
        }
        #endregion
    }

    //isChecked for radioButtons
    public class InstanceToBooleanConverter : MarkupExtension, IMultiValueConverter
    {
        private static InstanceToBooleanConverter _converter = null;
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values[0] as string == values[1] as string) return true;
            return false;

            //return value.Equals(parameter);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
            //return value.Equals(true) ? parameter : Binding.DoNothing;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (_converter == null)
            {
                _converter = new InstanceToBooleanConverter();
            }
            return _converter;
        }
    }
     
    //for converting height of the wordList
    public class HeightConverter :MarkupExtension, IValueConverter
    {
        private static HeightConverter _converter = null;
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (_converter == null)
            {
                _converter = new HeightConverter();
            }
            return _converter;
        }

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            // Default to 0. You may want to handle divide by zero 
            // and other issues differently than this.
            double result = 0;

            // Not the best code ever, but you get the idea.
            if (value != null && parameter != null)
            {
                try
                {
                    double height = (double)value;
                    double difference = double.Parse(parameter.ToString());
                    result = height - difference;
                    
                }
                catch (Exception e)
                {
                    // TODO: Handle casting exceptions.
                }
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
