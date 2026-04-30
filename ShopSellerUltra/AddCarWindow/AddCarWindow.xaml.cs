using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Ribbon.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using AutoSellerUltra.AutoWindow;
using Microsoft.Win32;
using Write;
using static Write.WriteDB;

namespace AutoSellerUltra.AddCarWindow
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class AddCarWindow : Window
    {
        private byte[]? _imageBytes;

        public AddCarWindow()
        {
            InitializeComponent();
        }
        private void BrowseImageButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Images (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png",
                Title = "Select car image"
            };

            if (dialog.ShowDialog() != true)
                return;

            ImagePathTextBox.Text = dialog.FileName;

            // читаем байты (для BLOB)
            _imageBytes = File.ReadAllBytes(dialog.FileName);

            // показываем превью
            PreviewImage.Source = CreateBitmapImage(_imageBytes);
        }

        private static BitmapImage CreateBitmapImage(byte[] bytes)
        {
            using var ms = new MemoryStream(bytes);

            var img = new BitmapImage();
            img.BeginInit();
            img.CacheOption = BitmapCacheOption.OnLoad;
            img.StreamSource = ms;
            img.EndInit();
            img.Freeze();

            return img;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // базовая валидация
                if (string.IsNullOrWhiteSpace(NameTextBox.Text))
                {
                    MessageBox.Show("Введите Name.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (_imageBytes == null)
                {
                    MessageBox.Show("Выберите изображение.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }


                // 1. Создаём объект Auto из полей
                var auto = new Auto
                {
                    Name = NameTextBox.Text.Trim(),
                    Category = CategoryTextBox.Text.Trim(),
                    //Category = "Sedan",
                    Price = int.Parse(PriceTextBox.Text.Trim()),
                    Quantity = int.Parse(QuantityTextBox.Text.Trim()),
                    ImageBytes = _imageBytes
                };
                var categories = AutoSellerUltra.AutoWindow.SelectAutoWindow.Get_categories();
                    // ПРОВЕРКА КАТЕГОРИИ
                    if (!categories.Contains(auto.Category))
                    {
                    MessageBox.Show(
                        $"Category \"{auto.Category}\" doesn`t exist.\n" +
                        "Choose other Category.",
                        "Category Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );

                    return; // ❗ прекращаем добавление
                    }
            

                // 2. Вызываем запись в БД
                WriteDB.Write_toDB(auto);

                
                // 3. Закрываем окно
                this.DialogResult = true;
                this.Close();
            }
            catch (FormatException)
            {
                MessageBox.Show("Price и Quantity должны быть числами.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Invalid data: " + ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
