using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.IO;
using Microsoft.Win32;

namespace _122_Rogosin_UP02.Pages
{
    public partial class CompletedAdsPage : Page
    {
        private int _currentUserId;
        private List<Ad> _completedAds;

        public CompletedAdsPage(int userId)
        {
            InitializeComponent();
            _currentUserId = userId;
            LoadCompletedAds();
        }

        // Загрузка завершенных объявлений
        private void LoadCompletedAds()
        {
            try
            {
                using (var context = new Entities())
                {
                    // Получаем завершенные объявления текущего пользователя
                    _completedAds = context.Ad
                        .Where(a => a.user_login_id == _currentUserId)
                        .ToList()
                        .Where(a => GetStatusText(a.Ad_Status?.ad_status1) == "Завершено")
                        .ToList();

                    DisplayAds();
                    UpdateStatistics();
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Ошибка загрузки завершенных объявлений: {ex.Message}");
            }
        }

        // Отображение объявлений
        private void DisplayAds()
        {
            AdsPanel.Children.Clear();

            if (_completedAds == null || !_completedAds.Any())
            {
                ShowNoAdsMessage();
                return;
            }

            foreach (var ad in _completedAds.OrderByDescending(a => a.Ad_post_date1?.ad_post_date1))
            {
                var adCard = CreateAdCard(ad);
                AdsPanel.Children.Add(adCard);
            }

            StatusText.Text = $"Найдено завершенных объявлений: {_completedAds.Count}";
        }

        // Создание карточки объявления
        private Border CreateAdCard(Ad ad)
        {
            var card = new Border
            {
                Style = (Style)FindResource("AdCardStyle"),
                Margin = new Thickness(0, 0, 0, 8)
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // Основная информация об объявлении
            var mainStack = new StackPanel();

            // Заголовок и статус
            var headerStack = new StackPanel { Orientation = Orientation.Horizontal };

            var titleText = new TextBlock
            {
                Text = ad.Ad_Title1?.ad_title1 ?? "Без названия",
                FontWeight = FontWeights.Bold,
                FontSize = 14,
                Margin = new Thickness(0, 0, 16, 0)
            };

            var statusText = new TextBlock
            {
                Text = $"(Завершено)",
                Foreground = (Brush)FindResource("SecondaryTextBrush"),
                FontWeight = FontWeights.SemiBold
            };

            headerStack.Children.Add(titleText);
            headerStack.Children.Add(statusText);

            // Описание
            var descText = new TextBlock
            {
                Text = ad.Ad_Description1?.ad_description1 ?? "Описание отсутствует",
                Style = (Style)FindResource("SecondaryTextStyle"),
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 4, 0, 4)
            };

            // Детали
            var detailsStack = new StackPanel { Orientation = Orientation.Horizontal };

            var typeText = new TextBlock
            {
                Text = $"Тип: {ad.Ad_Type?.ad_type1 ?? "Не указан"}",
                Margin = new Thickness(0, 0, 16, 0)
            };

            var categoryText = new TextBlock
            {
                Text = $"Категория: {ad.Category?.category1 ?? "Не указана"}",
                Margin = new Thickness(0, 0, 16, 0)
            };

            var cityText = new TextBlock
            {
                Text = $"Город: {ad.City?.city1 ?? "Не указан"}",
                Style = (Style)FindResource("SecondaryTextStyle"),
                Margin = new Thickness(0, 0, 16, 0)
            };

            var priceText = new TextBlock
            {
                Text = ad.price.HasValue ? $"Цена: {ad.price.Value:C}" : "Цена не указана",
                FontWeight = FontWeights.SemiBold
            };

            detailsStack.Children.Add(typeText);
            detailsStack.Children.Add(categoryText);
            detailsStack.Children.Add(cityText);
            detailsStack.Children.Add(priceText);

            // Дата публикации
            var dateText = new TextBlock
            {
                Text = $"Завершено: {ad.Ad_post_date1?.ad_post_date1 ?? "Дата не указана"}",
                Style = (Style)FindResource("SecondaryTextStyle"),
                FontSize = 10
            };

            mainStack.Children.Add(headerStack);
            mainStack.Children.Add(descText);
            mainStack.Children.Add(detailsStack);
            mainStack.Children.Add(dateText);

            Grid.SetColumn(mainStack, 0);

            // Кнопка реактивации
            var buttonsStack = new StackPanel
            {
                Orientation = Orientation.Vertical,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(16, 0, 0, 0)
            };

            var activateButton = new Button
            {
                Content = "Активировать",
                Style = (Style)FindResource("SmallButtonStyle"),
                Tag = ad.ID,
                Margin = new Thickness(0, 2, 0, 2)
            };
            activateButton.Click += ActivateButton_Click;

            buttonsStack.Children.Add(activateButton);

            Grid.SetColumn(buttonsStack, 1);

            grid.Children.Add(mainStack);
            grid.Children.Add(buttonsStack);

            card.Child = grid;
            return card;
        }

        // Получение текста статуса
        private string GetStatusText(string status)
        {
            if (string.IsNullOrEmpty(status)) return "Неизвестно";

            var statusLower = status.ToLower();
            if (statusLower == "active" || statusLower == "активно")
                return "Активно";
            if (statusLower == "completed" || statusLower == "завершено")
                return "Завершено";

            return status;
        }

        // Сообщение при отсутствии объявлений
        private void ShowNoAdsMessage()
        {
            var messageText = new TextBlock
            {
                Text = "У вас нет завершенных объявлений.",
                Style = (Style)FindResource("HeaderStyle"),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(16, 32, 16, 32)
            };

            AdsPanel.Children.Add(messageText);
            StatusText.Text = "Завершенные объявления не найдены";
        }

        // Обновление статистики
        private void UpdateStatistics()
        {
            if (_completedAds == null || !_completedAds.Any())
            {
                StatsText.Text = "Нет данных для отображения";
                return;
            }

            var totalCount = _completedAds.Count;
            var totalValue = _completedAds.Sum(a => a.price ?? 0);
            var avgPrice = totalCount > 0 ? totalValue / totalCount : 0;

            StatsText.Text = $"Всего завершено: {totalCount} | Общая стоимость: {totalValue:C} | Средняя цена: {avgPrice:C}";
        }

        // Активация объявления
        private void ActivateButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var adId = (int)button.Tag;

            try
            {
                using (var context = new Entities())
                {
                    var adToUpdate = context.Ad.FirstOrDefault(a => a.ID == adId);
                    if (adToUpdate != null)
                    {
                        // Активация объявления
                        var activeStatus = context.Ad_Status.FirstOrDefault(s =>
                            s.ad_status1.ToLower() == "active" || s.ad_status1.ToLower() == "активно");

                        if (activeStatus != null)
                        {
                            adToUpdate.ad_status_id = activeStatus.ID;
                            context.SaveChanges();

                            ShowSuccessMessage("Объявление активировано");
                            LoadCompletedAds(); // Обновляем список
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Ошибка при активации объявления: {ex.Message}");
            }
        }

        // Экспорт в CSV
        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            if (_completedAds == null || !_completedAds.Any())
            {
                MessageBox.Show("Нет данных для экспорта", "Экспорт",
                              MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                var saveDialog = new SaveFileDialog
                {
                    Filter = "CSV files (*.csv)|*.csv",
                    FileName = $"completed_ads_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    using (var writer = new StreamWriter(saveDialog.FileName))
                    {
                        // Заголовок CSV
                        writer.WriteLine("Название;Описание;Тип;Категория;Город;Цена;Дата публикации");

                        // Данные
                        foreach (var ad in _completedAds)
                        {
                            var title = ad.Ad_Title1?.ad_title1 ?? "";
                            var description = ad.Ad_Description1?.ad_description1 ?? "";
                            var type = ad.Ad_Type?.ad_type1 ?? "";
                            var category = ad.Category?.category1 ?? "";
                            var city = ad.City?.city1 ?? "";
                            var price = ad.price?.ToString("F2") ?? "";
                            var date = ad.Ad_post_date1?.ad_post_date1 ?? "";

                            // Экранируем специальные символы
                            title = title.Replace(";", ",").Replace("\"", "\"\"");
                            description = description.Replace(";", ",").Replace("\"", "\"\"");

                            writer.WriteLine($"\"{title}\";\"{description}\";{type};{category};{city};{price};{date}");
                        }
                    }

                    ShowSuccessMessage($"Данные экспортированы в файл: {saveDialog.FileName}");
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Ошибка при экспорте: {ex.Message}");
            }
        }

        // Назад
        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }

        // Вспомогательные методы для сообщений
        private void ShowErrorMessage(string message)
        {
            MessageBox.Show(message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            StatusText.Text = "Ошибка";
        }

        private void ShowSuccessMessage(string message)
        {
            MessageBox.Show(message, "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}