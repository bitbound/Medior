using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Medior.Controls
{
    public class GridEx : Grid
    {
        public static readonly DependencyProperty ColumnDefinitionsExProperty =
            DependencyProperty.Register("ColumnDefinitionsEx", typeof(string), typeof(GridEx),
                new PropertyMetadata((sender, args) =>
                {
                    if (sender is GridEx grid &&
                        args.NewValue is string stringVal)
                    {
                        grid.ColumnDefinitionsEx = stringVal;
                    }
                }));

        public static readonly DependencyProperty RowDefinitionsExProperty =
            DependencyProperty.Register(
                "RowDefinitionsEx", 
                typeof(string), 
                typeof(GridEx),
                new PropertyMetadata((sender, args) =>
                {
                    if (sender is GridEx grid && 
                        args.NewValue is string stringVal)
                    {
                        grid.RowDefinitionsEx = stringVal;
                    }
                }));

        private readonly GridLengthConverter _gridLengthConverter = new();

        public string ColumnDefinitionsEx
        {
            get { return (string)GetValue(ColumnDefinitionsExProperty); }
            set
            {
                SetValue(ColumnDefinitionsExProperty, value);
                ColumnDefinitions.Clear();

                if (string.IsNullOrWhiteSpace(value))
                {
                    return;
                }

                var definitions = value
                    .Split(",")
                    .Select(x =>
                    {
                        var def = new ColumnDefinition();
                        try
                        {
                            if (_gridLengthConverter.ConvertFrom(x) is GridLength length)
                            {
                                def.Width = length;
                            }
                        }
                        catch { }
                        return def;
                    });

                foreach (var definition in definitions)
                {
                    ColumnDefinitions.Add(definition);
                }
            }
        }

        public string RowDefinitionsEx
        {
            get { return (string)GetValue(RowDefinitionsExProperty); }
            set 
            { 
                SetValue(RowDefinitionsExProperty, value);
                RowDefinitions.Clear();

                if (string.IsNullOrWhiteSpace(value))
                {
                    return;
                }

                var definitions = value
                    .Split(",")
                    .Select(x =>
                    {
                        var def = new RowDefinition();
                        try
                        {
                            if (_gridLengthConverter.ConvertFrom(x) is GridLength length)
                            {
                                def.Height = length;
                            }
                        }
                        catch { }
                        return def;
                    });
                
                foreach (var definition in definitions)
                {
                    RowDefinitions.Add(definition);
                }
            }
        }
    }
}
