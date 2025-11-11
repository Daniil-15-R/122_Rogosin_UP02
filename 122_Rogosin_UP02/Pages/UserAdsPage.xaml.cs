using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using _122_Rogosin_UP02;

namespace _122_Rogosin_UP02.Pages
{
    public partial class UserAdsPage : Page
    {
        private int _currentUserId;
        private List<Ad> _userAds;
        private bool _showCompletedOnly = false;

        public UserAdsPage(int userId)
        {
            InitializeComponent();
            _currentUserId = userId;
            LoadUserAds();
        }

        // Загрузка объявлений пользователя
        private void LoadUserAds()
        {
            try
            {
                using (var context = new Entities())
                {
                    // Получаем объявления текущего пользователя
                    _userAds = context.Ad
                        .Where(a => a.user_login_id == _currentUserId)
                        .ToList();

                    DisplayAds();
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Ошибка загрузки объявлений: {ex.Message}");
            }
        }

        // Отображение объявлений
        private void DisplayAds()
        {
            AdsPanel.Children.Clear();

            var adsToShow = _showCompletedOnly
                ? _userAds.Where(a => GetStatusText(a.Ad_Status?.ad_status1) == "Завершено").ToList()
                : _userAds;

            if (adsToShow == null || !adsToShow.Any())
            {
                ShowNoAdsMessage();
                return;
            }

            foreach (var ad in adsToShow.OrderByDescending(a => a.Ad_post_date1?.ad_post_date1))
            {
                var adCard = CreateAdCard(ad);
                AdsPanel.Children.Add(adCard);
            }

            UpdateStatusText();
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
                Text = $"({GetStatusText(ad.Ad_Status?.ad_status1)})",
                Foreground = GetStatusColor(ad.Ad_Status?.ad_status1),
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
                Text = $"Опубликовано: {ad.Ad_post_date1?.ad_post_date1 ?? "Дата не указана"}",
                Style = (Style)FindResource("SecondaryTextStyle"),
                FontSize = 10
            };

            mainStack.Children.Add(headerStack);
            mainStack.Children.Add(descText);
            mainStack.Children.Add(detailsStack);
            mainStack.Children.Add(dateText);

            Grid.SetColumn(mainStack, 0);

            // Кнопки управления
            var buttonsStack = new StackPanel
            {
                Orientation = Orientation.Vertical,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(16, 0, 0, 0)
            };

            var editButton = new Button
            {
                Content = "Редактировать",
                Style = (Style)FindResource("SmallButtonStyle"),
                Tag = ad.ID,
                Margin = new Thickness(0, 2, 0, 2)
            };
            editButton.Click += EditButton_Click;

            var statusButton = new Button
            {
                Content = GetStatusButtonText(ad.Ad_Status?.ad_status1),
                Style = (Style)FindResource("SmallButtonStyle"),
                Tag = ad.ID,
                Margin = new Thickness(0, 2, 0, 2)
            };
            statusButton.Click += StatusButton_Click;

            var deleteButton = new Button
            {
                Content = "Удалить",
                Style = (Style)FindResource("SmallButtonStyle"),
                Tag = ad.ID,
                Margin = new Thickness(0, 2, 0, 2)
            };
            deleteButton.Click += DeleteButton_Click;

            buttonsStack.Children.Add(editButton);
            buttonsStack.Children.Add(statusButton);
            buttonsStack.Children.Add(deleteButton);

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

        // Получение цвета статуса
        private Brush GetStatusColor(string status)
        {
            if (string.IsNullOrEmpty(status))
                return (Brush)FindResource("PrimaryTextBrush");

            var statusLower = status.ToLower();
            if (statusLower == "active" || statusLower == "активно")
                return (Brush)FindResource("SuccessBrush");
            if (statusLower == "completed" || statusLower == "завершено")
                return (Brush)FindResource("SecondaryTextBrush");

            return (Brush)FindResource("PrimaryTextBrush");
        }

        // Текст для кнопки изменения статуса
        private string GetStatusButtonText(string status)
        {
            if (string.IsNullOrEmpty(status)) return "Изменить статус";

            var statusLower = status.ToLower();
            if (statusLower == "active" || statusLower == "активно")
                return "Завершить";
            if (statusLower == "completed" || statusLower == "завершено")
                return "Активировать";

            return "Изменить статус";
        }

        // Сообщение при отсутствии объявлений
        private void ShowNoAdsMessage()
        {
            var messageText = new TextBlock
            {
                Text = _showCompletedOnly
                    ? "У вас нет завершенных объявлений."
                    : "У вас пока нет объявлений.",
                Style = (Style)FindResource("HeaderStyle"),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(16, 32, 16, 32)
            };

            AdsPanel.Children.Add(messageText);
            StatusText.Text = "Объявления не найдены";
        }

        // Обновление статусной строки
        private void UpdateStatusText()
        {
            if (_userAds == null)
            {
                StatusText.Text = "Нет объявлений";
                return;
            }

            var activeCount = _userAds.Count(a =>
                GetStatusText(a.Ad_Status?.ad_status1) == "Активно");
            var completedCount = _userAds.Count(a =>
                GetStatusText(a.Ad_Status?.ad_status1) == "Завершено");

            var currentMode = _showCompletedOnly ? "Завершенные" : "Все";
            StatusText.Text = $"{currentMode} | Всего: {_userAds.Count} | Активных: {activeCount} | Завершенных: {completedCount}";
        }

        // Редактирование объявления
        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var adId = (int)button.Tag;

            try
            {
                using (var context = new Entities())
                {
                    var adToEdit = context.Ad
                        .Include("Ad_Title1")
                        .Include("Ad_Description1")
                        .Include("Ad_post_date1")
                        .Include("Ad_Type")
                        .Include("Ad_Status")
                        .Include("Category")
                        .Include("City")
                        .Include("Users_Login")
                        .Include("Users_Password")
                        .FirstOrDefault(a => a.ID == adId);

                    if (adToEdit != null)
                    {
                        // Переход на страницу редактирования с передачей объявления
                        var editPage = new AddAdsPage(_currentUserId, adToEdit);
                        editPage.AdUpdated += (s, args) => LoadUserAds();
                        NavigationService.Navigate(editPage);
                    }
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Ошибка при открытии редактирования: {ex.Message}");
            }
        }

        // Изменение статуса объявления
        private void StatusButton_Click(object sender, RoutedEventArgs e)
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
                        var currentStatus = adToUpdate.Ad_Status?.ad_status1;
                        string newStatus;

                        if (GetStatusText(currentStatus) == "Активно")
                        {
                            // Завершение объявления - запрашиваем прибыль
                            var profitInput = new ProfitInputDialog();
                            if (profitInput.ShowDialog() == true)
                            {
                                var profit = profitInput.Profit;
                                newStatus = "Завершено";

                                // Обновляем статус
                                var completedStatus = context.Ad_Status.FirstOrDefault(s =>
                                    s.ad_status1.ToLower() == "completed" || s.ad_status1.ToLower() == "завершено");
                                if (completedStatus != null)
                                {
                                    adToUpdate.ad_status_id = completedStatus.ID;
                                }

                                ShowSuccessMessage($"Объявление завершено. Прибыль: {profit:C}");
                            }
                            else
                            {
                                return; // Пользователь отменил ввод
                            }
                        }
                        else
                        {
                            // Активация объявления
                            newStatus = "Активно";
                            var activeStatus = context.Ad_Status.FirstOrDefault(s =>
                                s.ad_status1.ToLower() == "active" || s.ad_status1.ToLower() == "активно");
                            if (activeStatus != null)
                            {
                                adToUpdate.ad_status_id = activeStatus.ID;
                            }

                            ShowSuccessMessage("Объявление активировано");
                        }

                        context.SaveChanges();
                        LoadUserAds(); // Обновляем список
                    }
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Ошибка при изменении статуса: {ex.Message}");
            }
        }

        // Удаление объявления
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var adId = (int)button.Tag;

            var result = MessageBox.Show(
                "Вы уверены, что хотите удалить это объявление?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (var context = new Entities())
                    {
                        var adToDelete = context.Ad.FirstOrDefault(a => a.ID == adId);
                        if (adToDelete != null)
                        {
                            context.Ad.Remove(adToDelete);
                            context.SaveChanges();

                            ShowSuccessMessage("Объявление успешно удалено");
                            LoadUserAds(); // Обновляем список
                        }
                    }
                }
                catch (Exception ex)
                {
                    ShowErrorMessage($"Ошибка при удалении объявления: {ex.Message}");
                }
            }
        }

        // Переключение на завершенные объявления
        private void BtnCompletedAds_Click(object sender, RoutedEventArgs e)
        {
            _showCompletedOnly = !_showCompletedOnly;
            BtnCompletedAds.Content = _showCompletedOnly ? "Все объявления" : "Завершенные";
            DisplayAds();
        }

        // Добавление нового объявления - ПЕРЕХОД НА AddAdsPage
        private void BtnAddAd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var addAdsPage = new AddAdsPage(_currentUserId);
                addAdsPage.AdAdded += (s, args) =>
                {
                    // Обновляем список объявлений после добавления нового
                    LoadUserAds();
                };
                NavigationService.Navigate(addAdsPage);
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Ошибка при переходе на страницу добавления: {ex.Message}");
            }
        }

        // Назад к главной странице
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
            StatusText.Text = "Ошибка загрузки данных";
        }

        private void ShowSuccessMessage(string message)
        {
            MessageBox.Show(message, "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    // Простой диалог для ввода прибыли
    public class ProfitInputDialog
    {
        public decimal Profit { get; private set; }

        public bool ShowDialog()
        {
            var inputDialog = new Window
            {
                Title = "Ввод прибыли",
                Height = 150,
                Width = 300,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ResizeMode = ResizeMode.NoResize
            };

            var stackPanel = new StackPanel { Margin = new Thickness(16) };

            var textBlock = new TextBlock
            {
                Text = "Введите полученную прибыль:",
                Margin = new Thickness(0, 0, 0, 8)
            };

            var textBox = new TextBox
            {
                Height = 30,
                Margin = new Thickness(0, 0, 0, 16)
            };

            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right
            };

            var okButton = new Button
            {
                Content = "OK",
                Width = 80,
                Margin = new Thickness(0, 0, 8, 0)
            };

            var cancelButton = new Button
            {
                Content = "Отмена",
                Width = 80
            };

            okButton.Click += (s, e) =>
            {
                if (decimal.TryParse(textBox.Text, out decimal profit))
                {
                    Profit = profit;
                    inputDialog.DialogResult = true;
                    inputDialog.Close();
                }
                else
                {
                    MessageBox.Show("Введите корректную сумму", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            };

            cancelButton.Click += (s, e) =>
            {
                inputDialog.DialogResult = false;
                inputDialog.Close();
            };

            buttonPanel.Children.Add(okButton);
            buttonPanel.Children.Add(cancelButton);

            stackPanel.Children.Add(textBlock);
            stackPanel.Children.Add(textBox);
            stackPanel.Children.Add(buttonPanel);

            inputDialog.Content = stackPanel;

            return inputDialog.ShowDialog() == true;
        }
    }
}