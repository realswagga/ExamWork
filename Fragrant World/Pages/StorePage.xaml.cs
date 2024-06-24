using Fragrant_World.Classes;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;

namespace Fragrant_World.Pages
{
    /// <summary>
    /// Interaction logic for StorePage.xaml
    /// </summary>
    public partial class StorePage : Page
    {
        internal List<Product> Products { get; set; }

        //Словарь для составления запроса
        Dictionary<int, string> queryBlocks = new()
        {
            { 0, "AND ProductDiscountAmount BETWEEN 0 AND 9.99" },
            { 1, "AND ProductDiscountAmount BETWEEN 10 AND 14.99" },
            { 2, "AND ProductDiscountAmount BETWEEN 15 AND 100" },
            { 3, string.Empty },
            { 4, $"{Environment.NewLine}ORDER BY ROUND(ProductCost - (ProductCost * ProductDiscountAmount*0.01),2) ASC" },
            { 5, $"{Environment.NewLine}ORDER BY ROUND(ProductCost - (ProductCost * ProductDiscountAmount*0.01),2) DESC" },
        };

        //Переменные для плавной прокрутки
        private double targetVerticalOffset;
        private bool isUserScrolling = false;
        private const double smoothingFactor = 0.09;

        public StorePage()
        {
            InitializeComponent();
            UserNameLabel.Content = $"{UserDataBus.Surname} {UserDataBus.Name} {UserDataBus.Patronymic}";
            Style = (Style)FindResource(typeof(Page));

            SaleComboBox.SelectedIndex = 3;
            PriceComboBox.SelectedIndex = 0;

            //Подписки для плавной прокрутки
            scrollViewer.PreviewMouseWheel += ScrollViewer_PreviewMouseWheel;
            scrollViewer.ScrollChanged += ScrollViewer_ScrollChanged;
            CompositionTarget.Rendering += CompositionTarget_Rendering;
            scrollViewer.PreviewMouseDown += ScrollViewer_PreviewMouseDown;
            scrollViewer.PreviewMouseUp += ScrollViewer_PreviewMouseUp;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateProducts();
        }

        private void ExitImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.NavigationService.Navigate(new AuthPage());
        }

        //Обновляет товары
        private void UpdateProducts()
        {
            Products = DataAccessLayer.GetProductsData(UserDataBus.SearchQuery);
            foreach (Product product in Products)
            {
                CreateProductsList(product);
            }
            ProductsQuantityLabel.Content = $"{Products.Count} из {DataAccessLayer.GetProductsCount()}";
        }

        //Общее событие для составления строки поиска
        private void QueryBuilder(object sender, RoutedEventArgs e)
        {
            MainStackPanel.Children.Clear();

            UserDataBus.SearchQuery = $"WHERE ProductName LIKE '%{SearchTextBox.Text}%'";
            for (int i = 0; i < SaleComboBox.Items.Count; i++)
            {
                if (SaleComboBox.SelectedIndex == i)
                    UserDataBus.SearchQuery += queryBlocks[i];
            }
            for (int i = 0; i < PriceComboBox.Items.Count; i++)
            {
                if (PriceComboBox.SelectedIndex == i)
                    UserDataBus.SearchQuery += queryBlocks[i+4];
            }

            UpdateProducts();
        }

        //Создание контекстного меню
        private static ContextMenu GenerateContextMenu(Product product)
        {
            MenuItem AddItemMenu = new MenuItem { Header = "Добавить в корзину", Tag = product };
            MenuItem EditItem = new MenuItem { Header = "Изменить товар", Tag = product };
            MenuItem DeleteItem = new MenuItem { Header = "Удалить товар", Tag = product };
            ContextMenu contextMenu = new ContextMenu();
            contextMenu.Items.Add(AddItemMenu);
            if (UserDataBus.RoleId == 1 || UserDataBus.RoleId == 3)
            {
                contextMenu.Items.Add(EditItem);
                contextMenu.Items.Add(DeleteItem);
            }
            AddItemMenu.Click += AddItemMenu_Click;
            return contextMenu;
        }
    
        //Метод для добавления товаров в корзину
        public static void AddItemMenu_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.Tag is Product product)
            {
                /*Random random = new();
                Order order = new Order()
                {
                    OrderID = DataAccessLayer.GetLastOrderID(),
                    UserID = UserDataBus.UserId,
                    OrderStatus = "Новый",
                    OrderDate = DateTime.Now,
                    OrderDeliveryDate = DateTime.Now.AddDays(random.Next(2, 10)),
                    OrderPickupPoint = DataAccessLayer.GetPickupPoint(),
                    OrderPickupCode = DataAccessLayer.GetPickupCode(),
                };
                DataAccessLayer.UpdateExamOrder(order);
                DataAccessLayer.UpdateExamOrderProduct(DataAccessLayer.GetLastOrderID(), product.Article, 1);*/
            }
        }

        //Создание карточек товаров
        private void CreateProductsList(Product product)
        {
            Border productBorder = new()
            {
                Height = 130,
                Background = new SolidColorBrush(Color.FromRgb(245, 209, 174)),
                Margin = new Thickness(10),
            };

            productBorder.ContextMenu = GenerateContextMenu(product);

            //Подписка события для подсветки товара при наведении мышки
            productBorder.MouseEnter += ProductBorder_MouseEnter;
            productBorder.MouseLeave += ProductBorder_MouseLeave;

            //Создание главного StackPanel
            StackPanel externalStackPanel = new()
            {
                Margin = new Thickness(10),
            };

            Grid productGrid = new Grid();

            StackPanel innerStackPanel = new();

            //Создание метки для названия товара
            Label nameProductLabel = new()
            {
                Content = product.Name,
                HorizontalAlignment = HorizontalAlignment.Left,
                FontSize = 22,
                Effect = new DropShadowEffect
                {
                    Color = Colors.Black,
                    BlurRadius = 10,
                    ShadowDepth = 2,
                    Opacity = 0.1
                }
            };

            //Создание метки для производителя товара
            Label manufacturerProductLabel = new()
            {
                Content = product.Manufacturer,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(0, -7.5, 0, 0),
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.FromRgb(125, 125, 125))
            };

            //Создание метки для описания товара
            Label descriptionLabel = new()
            {
                Content = product.Description,
                HorizontalAlignment = HorizontalAlignment.Left,
                FontSize = 13,
            };

            //Создание контейнера для цены товара
            StackPanel priceStackPanel = new()
            {
                Orientation = Orientation.Horizontal,
            };

            //Создание метки для цены товара
            Label priceLabel = new()
            {
                Content = "Цена:",
                FontSize = 20
            };

            //Вычисление цены после скидки
            TextBlock discounCostTextBlock = new()
            {
                Text = Math.Round(product.Cost - (product.Cost * product.DiscountAmount * 0.01), 2).ToString(),
                Height = 20,
                FontSize = 18
            };

            //Создание места для картинки товара
            Border imageBorder = new()
            {
                Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0)),
                Margin = new Thickness(0, 0, 10, 0),
                Height = 100,
                Width = 100,
                Effect = new DropShadowEffect
                {
                    Color = product.DiscountAmount < 15 ? Color.FromRgb(245, 209, 174) : Color.FromRgb(127, 255, 0),
                    BlurRadius = product.DiscountAmount < 15 ? 20 : 100,
                    ShadowDepth = 0,
                    Opacity = 0.7
                }
            };

            //Картинка товара
            Image image = new()
            {
                Source = new BitmapImage(new Uri("/Images/product.png", UriKind.Relative))
            };

            //Размещение всех элементов по контейнерам
            MainStackPanel.Children.Add(productBorder);
            productBorder.Child = externalStackPanel;

            externalStackPanel.Children.Add(productGrid);
            productGrid.ColumnDefinitions.Add(new ColumnDefinition());
            productGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            productGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

            productGrid.Children.Add(innerStackPanel);
            innerStackPanel.Children.Add(nameProductLabel);
            innerStackPanel.Children.Add(manufacturerProductLabel);
            innerStackPanel.Children.Add(descriptionLabel);
            innerStackPanel.Children.Add(priceStackPanel);

            priceStackPanel.Children.Add(priceLabel);
            priceStackPanel.Children.Add(discounCostTextBlock);

            //Размещение скидки и старой цены
            if (product.DiscountAmount > 0)
            {
                TextBlock priceTextBlock = new()
                {
                    Text = product.Cost.ToString(),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    FontSize = 16,
                    Margin = new Thickness(3, 0, 0, 0),
                    TextDecorations = TextDecorations.Strikethrough,
                    Foreground = new SolidColorBrush(Color.FromRgb(115, 115, 115))
                };

                Label discountLabel = new()
                {
                    Content = $"-{product.DiscountAmount}%",
                    FontSize = product.DiscountAmount < 15 ? 30 : 36,
                    VerticalAlignment = VerticalAlignment.Bottom,
                    Margin = new Thickness(0, 0, 10, 5),
                    Effect = new DropShadowEffect
                    {
                        Color = product.DiscountAmount < 15 ? Color.FromRgb(245, 209, 174) : Color.FromRgb(127, 255, 0),
                        BlurRadius = product.DiscountAmount < 15 ? 20 : 40,
                        ShadowDepth = 0,
                        Opacity = product.DiscountAmount < 15 ? 0.6 : 1
                    }
                };

                priceStackPanel.Children.Add(priceTextBlock);
                productGrid.Children.Add(discountLabel);
                Grid.SetColumn(discountLabel, 1);
            }

            //Размещение плашки с изображением товара
            imageBorder.Child = image;
            productGrid.Children.Add(imageBorder);

            //Указание столбцов
            Grid.SetColumn(innerStackPanel, 0);
            Grid.SetColumn(imageBorder, 2);
        }

        private void ProductBorder_MouseEnter(object sender, MouseEventArgs e)
        {
            Border border = sender as Border;
            if (border != null)
            {
                // Анимация изменения цвета фона
                ColorAnimation colorAnimation = new ColorAnimation
                {
                    To = Color.FromRgb(255, 204, 154),
                    Duration = new Duration(TimeSpan.FromSeconds(0.2))
                };
                border.Background.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);

                // Анимация добавления тени
                DropShadowEffect shadowEffect = new DropShadowEffect
                {
                    Color = Colors.Black,
                    BlurRadius = 30,
                    ShadowDepth = 2,
                    Opacity = 0
                };
                border.Effect = shadowEffect;

                DoubleAnimation shadowAnimation = new DoubleAnimation
                {
                    To = 0.2,
                    Duration = new Duration(TimeSpan.FromSeconds(0.2))
                };
                shadowEffect.BeginAnimation(DropShadowEffect.OpacityProperty, shadowAnimation);
            }
        }

        private void ProductBorder_MouseLeave(object sender, MouseEventArgs e)
        {
            Border border = sender as Border;
            if (border != null)
            {
                // Анимация изменения цвета фона
                ColorAnimation colorAnimation = new ColorAnimation
                {
                    To = Color.FromRgb(245, 209, 174),
                    Duration = new Duration(TimeSpan.FromSeconds(0.33))
                };
                border.Background.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);

                // Анимация удаления тени
                if (border.Effect is DropShadowEffect shadowEffect)
                {
                    DoubleAnimation shadowAnimation = new DoubleAnimation
                    {
                        To = 0,
                        Duration = new Duration(TimeSpan.FromSeconds(0.33))
                    };
                    shadowEffect.BeginAnimation(DropShadowEffect.OpacityProperty, shadowAnimation);
                }
            }
        }

        #region Плавная прокрутка
        //Множество событий для плавной прокрутки содержимого на экране
        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;
            targetVerticalOffset = scrollViewer.VerticalOffset - e.Delta;
            if (targetVerticalOffset < 0)
            {
                targetVerticalOffset = 0;
            }
            else if (targetVerticalOffset > scrollViewer.ScrollableHeight)
            {
                targetVerticalOffset = scrollViewer.ScrollableHeight;
            }
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (isUserScrolling)
            {
                targetVerticalOffset = scrollViewer.VerticalOffset;
            }
        }

        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            double currentVerticalOffset = scrollViewer.VerticalOffset;
            double delta = targetVerticalOffset - currentVerticalOffset;
            if (Math.Abs(delta) > 0.1)
            {
                double newVerticalOffset = currentVerticalOffset + delta * smoothingFactor;
                scrollViewer.ScrollToVerticalOffset(newVerticalOffset);
            }
        }

        private void ScrollViewer_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            isUserScrolling = true;
        }

        private void ScrollViewer_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            isUserScrolling = false;
        }
        #endregion

        private void CartImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.NavigationService.Navigate(new CheckoutPage());
        }
    }
}
