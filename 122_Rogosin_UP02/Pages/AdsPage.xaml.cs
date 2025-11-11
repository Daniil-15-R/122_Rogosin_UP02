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
        private Entities db;

        public AdsPage()
        {
            InitializeComponent();
            db = new Entities();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadAds();
        }

        private void LoadAds()
        {
            try
            {
                // Загружаем объявления со всеми связанными данными
                allAds = db.Ad
                    .Include("Category")
                    .Include("City")
                    .Include("Ad_Title1")        // Правильное имя навигационного свойства
                    .Include("Ad_Description1")  // Правильное имя навигационного свойства
                    .Include("Ad_Status")        // Правильное имя навигационного свойства
                    .Include("Ad_Type")          // Правильное имя навигационного свойства
                    .Include("Ad_post_date1")    // Правильное имя навигационного свойства
                    .OrderByDescending(a => a.ad_post_date)
                    .ToList();

                // Создаем список для отображения с дополнительными свойствами
                var displayAds = allAds.Select(ad => new AdDisplayModel(ad)).ToList();

                AdsItemsControl.ItemsSource = displayAds;
                StatusText.Text = $"Загружено {displayAds.Count} объявлений";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки объявлений: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText.Text = "Ошибка загрузки данных";
            }
        }

        private void FilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (allAds == null) return;

            var filter = (FilterComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            var displayAds = allAds.Select(ad => new AdDisplayModel(ad)).ToList();

            switch (filter)
            {
                case "Активные":
                    AdsItemsControl.ItemsSource = displayAds.Where(a =>
                        a.Ad_StatusString.ToLower() == "активно" ||
                        a.Ad_StatusString.ToLower() == "active");
                    break;
                case "Завершенные":
                    AdsItemsControl.ItemsSource = displayAds.Where(a =>
                        a.Ad_StatusString.ToLower() == "завершено" ||
                        a.Ad_StatusString.ToLower() == "completed");
                    break;
                default:
                    AdsItemsControl.ItemsSource = displayAds;
                    break;
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Функция добавления объявления будет реализована позже", "Информация",
                          MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // Обработчик клика по объявлению (если нужно открыть детали)
        private void AdItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is AdDisplayModel ad)
            {
                MessageBox.Show($"Выбрано объявление: {ad.Ad_Title}\n\n{ad.Ad_Description}",
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
    // Вспомогательный класс для отображения объявлений
    // Вспомогательный класс для отображения объявлений
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