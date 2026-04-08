using System.Collections.Generic;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.Configuration;

namespace SIMA.Helper
{
    public class Util
    {

        public Dictionary<int, string> CommonSizeGrid { 
          get{
                Dictionary<int, string> columns_width = new Dictionary<int, string>
                {
                    { 0, "4%" },
                    { 1, "40%" },
                    { 2, "33%" },
                    { 3, "12%" },
                    { 4, "7%" }
                };
                return columns_width;
            } 
                
        } 
        /// <summary>
        /// Initialize Cofiguration Manager
        /// </summary>
        /// <returns></returns>
        public IConfiguration CustomConfiguration()
        {
        JsonFile<object> _configfile = new JsonFile<object>("appsettings.json", "/Infrastructure/Config");
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile(_configfile.FilePath, optional: false, reloadOnChange: true)
            .AddEnvironmentVariables();

            return builder.Build();
        }
        /// <summary>
        /// Grid Width Adaptation of the size screen 
        /// </summary>
        /// <param name="grid">Object Grid to resize gievn the gridWindowWidth </param>
        /// <param name="gridWindowWidth"> This is the actual Window size width value</param>
        /// <param name="columns_width"> (Key = Colunm Numbre, Value = Size Width = Only Numeric string percent {Size}% or Size} </param>
        public void ResponsiveGridWidth(GridView grid, double gridWindowWidth, Dictionary<int, string> columns_width)
        {
            if (columns_width.Count > 0)
                foreach (var col in columns_width)
                {
                    double width_col;
                    double out_size;
                    string val_Noprecent = col.Value.Replace("%", "");
                    if (double.TryParse(val_Noprecent, out out_size))
                    {
                        width_col = (col.Value[col.Value.Length - 1] == '%')
                                ? gridWindowWidth * double.Parse(val_Noprecent) / 100
                                : out_size;

                        grid.Columns[col.Key].Width = width_col;
                    }
                }
        }
        /// <summary>
        /// Grid Width Adaptation of the size screen 
        /// </summary>
        /// <param name="grid">>Object Grid to resize gievn the gridWindowWidth</param>
        /// <param name="gridWindowWidth">This is the actual Window size width  value</param>
        /// <param name="errorate"> the rate of dimension error</param>
        public void ResponsiveGridWidth(GridView grid, double gridWindowWidth, double errorate = 0)
        {
            double size_rel_column = (gridWindowWidth / (double)grid.Columns.Count) - errorate;
            double responsive = Math.Round(size_rel_column / gridWindowWidth, 4);
            foreach (var col in grid.Columns)
            {
                col.Width = gridWindowWidth * responsive;
            }
        }
        /// <summary>
        /// ListView Height Adaptation of the size screen 
        /// </summary>
        /// <param name="listview">>Object listview to resize given the gridWindowHeight</param>
        /// <param name="gridWindowHeight">This is the actual Window Size Height </param>
        /// <param name="height">Size Height = Only Numeric string percent {Size}% or Size}</param>
        public void ResponsiveListViewHeight(ListView listview, double gridWindowHeight, string height)
        {
            if (listview != null)
            {
                double lv_height;
                double out_size;
                string val_Noprecent = height.Replace("%", "");
                if (double.TryParse(val_Noprecent, out out_size))
                {
                    lv_height = (height[height.Length - 1] == '%')
                            ? gridWindowHeight * double.Parse(val_Noprecent) / 100
                            : out_size;
                    listview.Height = lv_height;
                }
            }

        }

        internal void ResponsiveGridWidth(object gridView, double v, Dictionary<int, string> columns_width)
        {
            throw new NotImplementedException();
        }
    }

}
