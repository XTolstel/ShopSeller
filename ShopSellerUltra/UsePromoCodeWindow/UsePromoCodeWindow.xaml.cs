using System.Windows;

namespace AutoSellerUltra
{
    public partial class UsePromoCodeWindow : Window
    {
        public string PromoCode { get; private set; }

        public UsePromoCodeWindow()
        {
            InitializeComponent();
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            string promoCode = PromoCodeTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(promoCode))
            {
                ErrorTextBlock.Text = "Write promocode.";
                ErrorTextBlock.Visibility = Visibility.Visible;
                return;
            }

            PromoCode = promoCode;

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}