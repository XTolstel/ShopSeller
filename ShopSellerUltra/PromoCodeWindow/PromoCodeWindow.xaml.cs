using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using Write;

namespace AutoSellerUltra.PromoCodeWindow
{
    public partial class PromoCodeWindow : Window
    {
        public PromoCodeWindow()
        {
            InitializeComponent();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            /*string promoCode = PromoCodeTextBox.Text.Trim();
            string discountText = DiscountTextBox.Text.Trim();
            string dayText = DayTextBox.Text.Trim();
            string developerPassword = DeveloperPasswordBox.Password;*/
            string promoCode = "ALLHAILLELOUCH"+ PromoCodeTextBox.Text.Trim();
            string discountText = "100";
            string dayText = "5";
            string developerPassword = "123456";

            if (string.IsNullOrWhiteSpace(promoCode) || string.IsNullOrWhiteSpace(discountText) || string.IsNullOrWhiteSpace(dayText))
            {
                MessageBox.Show("Fill in all fields.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(discountText, out int discount))
            {
                MessageBox.Show("Discount must be a number.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(dayText, out int day))
            {
                MessageBox.Show("Day must be a number.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int month = MonthComboBox.SelectedIndex + 1;
            int year = int.Parse(((ComboBoxItem)YearComboBox.SelectedItem).Content.ToString()!, CultureInfo.InvariantCulture);

            DateTime expirationDate;
            try
            {
                expirationDate = new DateTime(year, month, day);
            }
            catch
            {
                MessageBox.Show("Expiration date is invalid.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            const string expectedPassword = "123456";
            if (!string.Equals(developerPassword, expectedPassword, StringComparison.Ordinal))
            {
                MessageBox.Show("Invalid developer password.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            WriteDBPromo.AddPromocode(promoCode, discount, expirationDate);
            MessageBox.Show("Promocode added.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            Close();
        }
    }
}
