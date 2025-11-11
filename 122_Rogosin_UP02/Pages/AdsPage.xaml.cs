using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace _122_Rogosin_UP02.Pages
{
    public partial class AdsPage : Page
    {
        private List<Ad> allAds;
        private List<Ad_Type> adTypes;
        private Entities db;

        public AdsPage()
        {
            InitializeComponent();
            db = new Entities();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadAdTypes();
            LoadAds();
        }

        private void LoadAdTypes()
        {
            try
            {
                // Загружаем все типы объявлений
                adTypes = db.Ad_Type.ToList();

                // Заполняем комбобокс фильтра по типам
                TypeFilterComboBox.Items.Clear();
                TypeFilterComboBox.Items.Add(new ComboBoxItem { Content = "Все типы", IsSelected = true });

                foreach (var adType in adTypes)
                {
                    TypeFilterComboBox.Items.Add(new ComboBoxItem { Content = adType.ad_type1 });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки типов объявлений: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadAds()
        {
            try
            {
                // Загружаем объявления со всеми связанными данными
                allAds = db.Ad
                    .Include("Category")
                    .Include("City")
                    .Include("Ad_Title1")
                    .Include("Ad_Description1")
                    .Include("Ad_Status")
                    .Include("Ad_Type")
                    .Include("Ad_post_date1")
                    .OrderByDescending(a => a.ad_post_date)
                    .ToList();

                ApplyFilters();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки объявлений: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText.Text = "Ошибка загрузки данных";
            }
        }

        private void ApplyFilters()
        {
            if (allAds == null) return;

            var displayAds = allAds.Select(ad => new AdDisplayModel(ad)).ToList();

            // Фильтрация по статусу
            var statusFilter = (FilterComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            if (!string.IsNullOrEmpty(statusFilter))
            {
                switch (statusFilter)
                {
                    case "Активные":
                        displayAds = displayAds.Where(a =>
                            a.Ad_StatusString.ToLower() == "активно" ||
                            a.Ad_StatusString.ToLower() == "active").ToList();
                        break;
                    case "Завершенные":
                        displayAds = displayAds.Where(a =>
                            a.Ad_StatusString.ToLower() == "завершено" ||
                            a.Ad_StatusString.ToLower() == "completed").ToList();
                        break;
                }
            }

            // Фильтрация по типу услуг
            var typeFilter = (TypeFilterComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            if (!string.IsNullOrEmpty(typeFilter) && typeFilter != "Все типы")
            {
                displayAds = displayAds.Where(a => a.Ad_Type == typeFilter).ToList();
            }

            AdsItemsControl.ItemsSource = displayAds;
            StatusText.Text = $"Показано {displayAds.Count} объявлений";
        }

        private void FilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void TypeFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int currentUserId = GetCurrentUserId();
                AddAdsPage addAdsPage = new AddAdsPage(currentUserId);

                addAdsPage.AdAdded += (s, args) =>
                {
                    LoadAds(); // Обновляем список после добавления
                };

                // Проверяем доступность навигации
                if (NavigationService != null)
                {
                    NavigationService.Navigate(addAdsPage);
                }
                else
                {
                    MessageBox.Show("Сервис навигации недоступен", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при переходе на страницу добавления: {ex.Message}",
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        // Метод для получения ID текущего пользователя (заглушка - замените на реальную реализацию)
        private int GetCurrentUserId()
        {
            // TODO: Замените на реальный способ получения ID текущего пользователя
            // Это может быть из настроек приложения, из базы данных, или другой источник
            return 1; // Временная заглушка
        }

        // Обработчик клика по объявлению (если нужно открыть детали)
        private void AdItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is AdDisplayModel ad)
            {
                MessageBox.Show($"Выбрано объявление: {ad.Ad_Title}\n\nТип: {ad.Ad_Type}\nЦена: {ad.Price}₽\n\n{ad.Ad_Description}",
                              "Детали объявления", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            // Добавляем обработчик клика для каждого элемента
            if (AdsItemsControl != null)
            {
                AdsItemsControl.AddHandler(MouseLeftButtonDownEvent,
                    new MouseButtonEventHandler(AdItem_MouseLeftButtonDown), true);
            }
        }
    }

    // Вспомогательный класс для отображения объявлений
    public class AdDisplayModel
    {
        private readonly Ad _ad;

        public AdDisplayModel(Ad ad)
        {
            _ad = ad;
        }

        // Основные свойства - получаем данные из связанных таблиц через навигационные свойства
        public string Ad_Title => _ad.Ad_Title1?.ad_title1 ?? "Без названия";
        public string Ad_Description => _ad.Ad_Description1?.ad_description1 ?? "Описание отсутствует";
        public DateTime Ad_post_date => GetPostDateSafe();
        public string Ad_Type => _ad.Ad_Type?.ad_type1 ?? "Не указан";

        // Связанные свойства
        public Category Category => _ad.Category;
        public City City => _ad.City;

        // Вычисляемые свойства
        public double Price => _ad.price ?? 0;

        // Свойство для строкового представления статуса
        public string Ad_StatusString => _ad.Ad_Status?.ad_status1 ?? "Неизвестно";

        public SolidColorBrush StatusColor
        {
            get
            {
                var status = Ad_StatusString.ToLower();
                switch (status)
                {
                    case "активно":
                    case "active":
                        return new SolidColorBrush(Colors.Green);
                    case "завершено":
                    case "completed":
                        return new SolidColorBrush(Colors.Gray);
                    case "ожидание":
                    case "pending":
                        return new SolidColorBrush(Colors.Orange);
                    default:
                        return new SolidColorBrush(Colors.Blue);
                }
            }
        }

        // Метод для безопасного получения даты (так как в Ad_post_date дата хранится как string)
        private DateTime GetPostDateSafe()
        {
            try
            {
                var dateString = _ad.Ad_post_date1?.ad_post_date1;
                if (DateTime.TryParse(dateString, out DateTime result))
                {
                    return result;
                }
                return DateTime.Now;
            }
            catch
            {
                return DateTime.Now;
            }
        }
    }
}