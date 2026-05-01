using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AutoSellerUltra.AddCarWindow;
using AutoSellerUltra.Login;
using AutoSellerUltra.Registration;
using Write;
using static Write.WriteDB;
using static Write.WriteDBUser;
using static AutoSellerUltra.Login.UserSession;
namespace AutoSellerUltra.AutoWindow
{
   

    public partial class SelectAutoWindow : Window
    {
        // Храним все машины здесь
        private static List<Auto> _allCars;

        private static List<string> categories = new();

        // это и есть "специальный список" для интерфейса  Wpf
        public ObservableCollection<Auto> Cart_cars { get; } = new ObservableCollection<Auto>();

        
        public static Dictionary<int, int> CartCounts { get; } = new();
        public static List<string> Get_categories() {
            return categories;
        }

        public static List<Auto> Get_cars()
        {
            return _allCars;
        }
        public SelectAutoWindow()
        {
            InitializeComponent();
            UserSession.SessionChanged += UpdateAuthUi;
            UpdateAuthUi();
            CartListBox.ItemsSource = Cart_cars;


            categories.Add("All");
            categories.Add("Sedan");
            categories.Add("Truck");
            categories.Add("Hatchback");
            categories.Add("Electric");
            categories.Add("Sport");

            CategoriesListBox.ItemsSource = categories;


            _allCars = WriteDB.LoadCarsFromDb();

            CarsItemsControl.ItemsSource = _allCars;

            //CategoriesListBox.SelectedItem = "All";
            // По умолчанию выберем первую категорию
            CategoriesListBox.SelectedIndex = 0;
        }

        private void UpdateAuthUi()
        {
            var user = UserSession.CurrentUser;
            bool isLoggedIn = user != null;

            AuthPanel.Visibility = isLoggedIn ? Visibility.Collapsed : Visibility.Visible;
            UserProfileButton.Visibility = isLoggedIn ? Visibility.Visible : Visibility.Collapsed;
            BalancePanel.Visibility = isLoggedIn ? Visibility.Visible : Visibility.Collapsed;
            LogoutButton.Visibility = isLoggedIn ? Visibility.Visible : Visibility.Collapsed;

            if (isLoggedIn)
            {
                UserLoginText.Text = user!.Login;
                BalanceText.Text = user.balance.ToString(); // или из user, если позже добавишь
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            UserSession.Logout();     // очищаем CurrentUser и уведомляем UI         
        }




        private void UsePromoCodeButton_Click(object sender, RoutedEventArgs e)
        {
            var promoCodeWindow = new AutoSellerUltra.PromoCodeWindow.PromoCodeWindow
            {
                Owner = this
            };

            promoCodeWindow.ShowDialog();
        }

        private void AddBalanceButton_Click(object sender, RoutedEventArgs e)
        {
            var win = new AutoSellerUltra.Balance.BalanceWindow
            {
                Owner = this
            };

            bool? result = win.ShowDialog();

            if (result == true)
            {
                UserSession.CurrentUser.balance += win.Amount;
                BalanceText.Text = UserSession.CurrentUser.balance.ToString();
                UpdateBalance(UserSession.CurrentUser);
            }
        }


        private void UserProfileButton_Click(object sender, RoutedEventArgs e)
        {
            var win = new AutoSellerUltra.UserWindow.UserWindow
            {
                Owner = this
            };
            win.ShowDialog();

        }



        // Вызывается при смене выбранной категории слева
        private void CategoriesListBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CategoriesListBox.SelectedItem is string selectedCategory)
            {
                // Фильтруем список машин по категории
                var filtered = _allCars
                    .Where(c => c.Category == selectedCategory)
                    .ToList();

                CarsItemsControl.ItemsSource = filtered;
            }
            if (CategoriesListBox.SelectedItem is "All")
            {
                // Если ничего не выбрано – показываем все машины
                CarsItemsControl.ItemsSource = _allCars;
            }
            
        }
        private void AddToListButton_Click(object sender, RoutedEventArgs e)
        {
            // создаём наше мини-окно
            var addCarWindow = new AutoSellerUltra.AddCarWindow.AddCarWindow
            {
                Owner = this   // чтобы окно было поверх текущего
            };
            
            // показываем его как диалог (пока просто открывается и закрывается)
            if (addCarWindow.ShowDialog() == true)
            {

                // 👇 ВАЖНО: выбираем All уже здесь
                CategoriesListBox.SelectedItem = "All";
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            _allCars = WriteDB.LoadCarsFromDb();
         
            CarsItemsControl.ItemsSource = _allCars;
        }

        // Вызывай это при клике по карточке машины
        private void AddToCart(Auto auto)
        {
            if (CartCounts.ContainsKey(auto.Id))
            {
                if (auto.Quantity <= CartCounts[auto.Id])
                {
                    return;
                }
                CartCounts[auto.Id] += 1;
            }
           
            else
            {
                CartCounts[auto.Id] = 1;
                Cart_cars.Add(auto);
            }
            CartListBox.Items.Refresh(); // 👈 обновляем UI
            UpdateTotal();
            CartListBox.SelectedItem = auto;
        }

        private int UpdateTotal()
        {
            // Если Price у тебя int:
            var total = Cart_cars.Sum(a => a.Price*CartCounts[a.Id]);

            TotalTextBlock.Text = $"Total: ${total}";
            return total;
        }

        private void RemoveFromCart_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.DataContext is Auto auto)
            {
                
                CartCounts[auto.Id] -= 1;
                if (CartCounts[auto.Id] <= 0)
                {
                    Cart_cars.Remove(auto);
                    CartCounts.Remove(auto.Id);

                }
                CartListBox.Items.Refresh(); // 👈 обновляем UI
                UpdateTotal();
            }
        }

        private void ClearCart_Click(object sender, RoutedEventArgs e)
        {
            Cart_cars.Clear();
            CartCounts.Clear();
            CartListBox.Items.Refresh(); // 👈 обновляем UI
            UpdateTotal();
        }

        private void Buy_Click(object sender, RoutedEventArgs e)
        {
            int quan = 0;
            foreach (var c in Cart_cars)
            {
                quan += CartCounts[c.Id];
            }

            if (Cart_cars.Count == 0)
            {
                MessageBox.Show($"You must have at least one item");
                return;
            }
            else if (getIsUser() != true)
            {
                MessageBox.Show($"You must login your account");
                return;
            }
            else if ((UserSession.CurrentUser.balance - UpdateTotal())<0)
            {
                MessageBox.Show($"You`re so fucking poor\nFind more money");
                return;
            }
            
            
            Buy_Auto(Cart_cars,CartCounts);
            MessageBox.Show($"Your buy is successfully complete\nItems: {quan}, Total: ${UpdateTotal()}");
            UserSession.CurrentUser.spendbalance += UpdateTotal();
            UserSession.CurrentUser.balance -= UpdateTotal();
            UpdateBalance(UserSession.CurrentUser);
            BalanceText.Text = UserSession.CurrentUser.balance.ToString();
            Cart_cars.Clear();
            CartCounts.Clear();
            CartListBox.Items.Refresh(); // 👈 обновляем UI
            UpdateTotal();
            //MessageBox.Show("Your buy is successfully complete");
        }

        // Общий обработчик клика по карточке авто (кнопке)

        private void AutoButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.DataContext is Auto auto)
            {
                AddToCart(auto);
            }
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            var win = new LoginUser
            {
                Owner = this
            };
            win.ShowDialog();
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            // TODO: открыть окно регистрации
            // var wnd = new RegisterWindow();
            // wnd.Owner = this;
            // wnd.ShowDialog();
            // создаём наше мини-окно
            var RegistrationWindow = new AutoSellerUltra.Registration.RegisterUser
            {
                Owner = this   // чтобы окно было поверх текущего
            };
            RegistrationWindow.ShowDialog();

        }


    }


}
