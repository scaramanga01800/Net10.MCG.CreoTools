using Fluent;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MCG.CREO_Tools.CutLengthApp.View
{
    /// <summary>
    /// Logique d'interaction pour CutLengthBulkQuantity.xaml
    /// </summary>
    public partial class CutLengthBulkQuantity : RibbonWindow, INotifyPropertyChanged
    {
        private double _Quantity;
        public double Quantity
        {
            get { return _Quantity; }
            set
            {
                if (this._Quantity != value)
                {
                    this._Quantity = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Quantity"));
                }

            }
        }

        public MessageBoxResult ReturnValue { get; set; }

        public CutLengthBulkQuantity()
        {
            DataContext = this;
            InitializeComponent();
        }

        public void SetQuantity(double quantity)
        {
            Quantity = quantity;
        }

        public double GetQuantity()
        {
            return Quantity;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9.]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void BtOk_Click(object sender, RoutedEventArgs e)
        {
            ReturnValue = MessageBoxResult.OK;
            this.Close();
        }

        private void BtCancel_Click(object sender, RoutedEventArgs e)
        {
            ReturnValue = MessageBoxResult.Cancel;
            this.Close();
        }
    }
}
