using System;
using System.Collections.Generic;
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
using AutoSellerUltra.Login;

namespace AutoSellerUltra.UserWindow
{
    /// <summary>
    /// Interaction logic for UserWindow.xaml
    /// </summary>
    public partial class UserWindow : Window
    {
        public UserWindow()
        {
            InitializeComponent();
            LoadUser();
        }
        private void LoadUser()
        {
            var user = UserSession.CurrentUser;

            if (user == null)
            {
                MessageBox.Show("No active user. Please log in first.",
                    "Profile", MessageBoxButton.OK, MessageBoxImage.Warning);
                Close();
                return;
            }

            UserLoginHeader.Text = user.Login;

            IdText.Text = user.Id.ToString();
            LoginText.Text = user.Login;
            EmailText.Text = user.Email;
            DobText.Text = user.DateOfBirth;

            // Если этих полей нет в твоём UserDto — удали строки ниже
            BalanceText.Text = user.balance.ToString();
            SpentText.Text = user.spendbalance.ToString();
        }

        
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
