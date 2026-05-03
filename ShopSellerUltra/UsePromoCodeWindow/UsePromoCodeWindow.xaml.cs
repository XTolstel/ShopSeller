using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using Write;
using AutoSellerUltra.Login;

namespace AutoSellerUltra.UsePromoCodeWindow
{
    public partial class UsePromoCodeWindow : Window
    {
        public string PromoCode { get; private set; }

        public UsePromoCodeWindow()
        {
            InitializeComponent();
        }

        private async void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            string promoCode = PromoCodeTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(promoCode))
            {
                MessageBox.Show("Please enter a promocode.");
                return;
            }

            try
            {
                var result = await Write.WriteDBPromo.CheckPromocodeAsync(promoCode);

                if (result.IsValid)
                {
                    MessageBox.Show(
                        $"Promocode status: {result.State}\n" +
                        $"Discount: {result.Discount}%"
                    );

                    UserSession.SetPromoDiscount(result.Discount);
                    DialogResult = true;
                    Close();
                }
                else
                {
                    MessageBox.Show(
                        $"{result.Message}\n" +
                        $"Status: {result.State}\n" +
                        $"Discount: {result.Discount}%"
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while checking the promocode: " + ex.Message);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}