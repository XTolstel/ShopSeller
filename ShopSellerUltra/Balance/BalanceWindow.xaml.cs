using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AutoSellerUltra.Balance
{
    /// <summary>
    /// Interaction logic for Balance_Window.xaml
    /// </summary>
    public partial class BalanceWindow : Window
    {
        public int Amount { get; private set; }

        public BalanceWindow()
        {
            InitializeComponent();


        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            var text = (AmountTextBox.Text ?? "").Trim();

            if (!int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value))
            {
                MessageBox.Show("Please enter a valid integer amount.", "Top up", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (value <= 0)
            {
                MessageBox.Show("Amount must be greater than 0.", "Top up", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Amount = value;
            DialogResult = true; // важно: вернёт true в ShowDialog()
            Close();
        }

    }
}
