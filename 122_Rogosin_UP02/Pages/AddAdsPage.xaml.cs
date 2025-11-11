using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace _122_Rogosin_UP02.Pages
{
    public partial class AddAdsPage : Page
    {
        private _122_Rogosin_UP02.Entities context;
        private int _currentUserId;

        // Событие для уведомления о добавлении объявления
        public event EventHandler AdAdded;

        public AddAdsPage(int userId)
        {
            InitializeComponent();
            _currentUserId = userId;
            try
            {
                context = new _122_Rogosin_UP02.Entities();
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка инициализации: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadData()
        {
            try
            {
                // Загрузка категорий
                cmbCategory.ItemsSource = context.Category.ToList();

                // Загрузка типов объявлений
                cmbAdType.ItemsSource = context.Ad_Type.ToList();

                // Загрузка статусов объявлений
                cmbAdStatus.ItemsSource = context.Ad_Status.ToList();

                // Загрузка городов
                cmbCity.ItemsSource = context.City.ToList();

                // Загрузка пользователей - устанавливаем текущего пользователя
                var currentUserLogin = context.Users_Login.FirstOrDefault(u => u.ID == _currentUserId);
                var currentUserPassword = context.Users_Password.FirstOrDefault(u => u.ID == _currentUserId);

                cmbUserLogin.ItemsSource = context.Users_Login.ToList();
                cmbUserPassword.ItemsSource = context.Users_Password.ToList();

                // Установка текущего пользователя по умолчанию
                if (currentUserLogin != null)
                    cmbUserLogin.SelectedItem = currentUserLogin;
                if (currentUserPassword != null)
                    cmbUserPassword.SelectedItem = currentUserPassword;

                // Установка текущей даты по умолчанию
                dpPostDate.SelectedDate = DateTime.Today;

                // Установка значений по умолчанию для выпадающих списков
                if (cmbAdStatus.Items.Count > 0)
                    cmbAdStatus.SelectedIndex = 0;
                if (cmbCategory.Items.Count > 0)
                    cmbCategory.SelectedIndex = 0;
                if (cmbAdType.Items.Count > 0)
                    cmbAdType.SelectedIndex = 0;
                if (cmbCity.Items.Count > 0)
                    cmbCity.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                ShowMessage($"Ошибка загрузки данных: {ex.Message}", true);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateForm())
            {
                SaveAdvertisement();
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Вы уверены, что хотите отменить создание объявления? Все несохраненные данные будут потеряны.",
                "Подтверждение отмены", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                // Возврат на предыдущую страницу
                if (NavigationService.CanGoBack)
                {
                    NavigationService.GoBack();
                }
            }
        }

        private bool ValidateForm()
        {
            // Проверка обязательных полей
            if (string.IsNullOrWhiteSpace(txtTitle.Text))
            {
                ShowMessage("Пожалуйста, введите название объявления", true);
                txtTitle.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtDescription.Text))
            {
                ShowMessage("Пожалуйста, введите описание объявления", true);
                txtDescription.Focus();
                return false;
            }

            if (cmbCategory.SelectedItem == null)
            {
                ShowMessage("Пожалуйста, выберите категорию", true);
                cmbCategory.Focus();
                return false;
            }

            if (cmbAdType.SelectedItem == null)
            {
                ShowMessage("Пожалуйста, выберите тип объявления", true);
                cmbAdType.Focus();
                return false;
            }

            if (cmbAdStatus.SelectedItem == null)
            {
                ShowMessage("Пожалуйста, выберите статус объявления", true);
                cmbAdStatus.Focus();
                return false;
            }

            if (cmbCity.SelectedItem == null)
            {
                ShowMessage("Пожалуйста, выберите город", true);
                cmbCity.Focus();
                return false;
            }

            if (cmbUserLogin.SelectedItem == null)
            {
                ShowMessage("Пожалуйста, выберите логин пользователя", true);
                cmbUserLogin.Focus();
                return false;
            }

            if (cmbUserPassword.SelectedItem == null)
            {
                ShowMessage("Пожалуйста, выберите пароль пользователя", true);
                cmbUserPassword.Focus();
                return false;
            }

            if (!string.IsNullOrWhiteSpace(txtPrice.Text) && !double.TryParse(txtPrice.Text, out _))
            {
                ShowMessage("Пожалуйста, введите корректную цену", true);
                txtPrice.Focus();
                return false;
            }

            return true;
        }

        private void SaveAdvertisement()
        {
            try
            {
                // Создание записей в связанных таблицах
                var adTitle = new Ad_Title { ad_title1 = txtTitle.Text.Trim() };
                var adDescription = new Ad_Description { ad_description1 = txtDescription.Text.Trim() };

                // Форматирование даты для сохранения
                string postDateString = dpPostDate.SelectedDate?.ToString("yyyy-MM-dd") ?? DateTime.Today.ToString("yyyy-MM-dd");
                var adPostDate = new Ad_post_date { ad_post_date1 = postDateString };

                // Сохранение связанных записей
                context.Ad_Title.Add(adTitle);
                context.Ad_Description.Add(adDescription);
                context.Ad_post_date.Add(adPostDate);
                context.SaveChanges();

                // Получение выбранных объектов
                var selectedCategory = cmbCategory.SelectedItem as Category;
                var selectedAdType = cmbAdType.SelectedItem as Ad_Type;
                var selectedAdStatus = cmbAdStatus.SelectedItem as Ad_Status;
                var selectedCity = cmbCity.SelectedItem as City;
                var selectedUserLogin = cmbUserLogin.SelectedItem as Users_Login;
                var selectedUserPassword = cmbUserPassword.SelectedItem as Users_Password;

                // Создание основного объявления
                var advertisement = new Ad
                {
                    // Связанные сущности
                    Ad_Title1 = adTitle,
                    Ad_Description1 = adDescription,
                    Ad_post_date1 = adPostDate,
                    Ad_Type = selectedAdType,
                    Ad_Status = selectedAdStatus,
                    Category = selectedCategory,
                    City = selectedCity,
                    Users_Login = selectedUserLogin,
                    Users_Password = selectedUserPassword,

                    // Прямые поля (ID)
                    user_login_id = selectedUserLogin.ID,
                    user_password_id = selectedUserPassword.ID,
                    ad_title = adTitle.ID,
                    ad_description = adDescription.ID,
                    ad_post_date = adPostDate.ID,
                    city_id = selectedCity.ID,
                    category_id = selectedCategory.ID,
                    ad_type_id = selectedAdType.ID,
                    ad_status_id = selectedAdStatus.ID,

                    // Цена (может быть null)
                    price = string.IsNullOrWhiteSpace(txtPrice.Text) ? null : (double?)double.Parse(txtPrice.Text)
                };

                context.Ad.Add(advertisement);
                context.SaveChanges();

                ShowMessage("Объявление успешно сохранено!", false);

                // Вызываем событие добавления объявления
                AdAdded?.Invoke(this, EventArgs.Empty);

                // Возврат на предыдущую страницу
                if (NavigationService.CanGoBack)
                {
                    NavigationService.GoBack();
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Ошибка при сохранении: {ex.Message}", true);
            }
        }

        private void ShowMessage(string message, bool isError)
        {
            txtMessage.Text = message;

            // Поиск стилей в ресурсах
            var errorStyle = FindResource("ErrorTextStyle") as Style;
            var successStyle = FindResource("SuccessTextStyle") as Style;

            txtMessage.Style = isError ? errorStyle : successStyle;
            txtMessage.Visibility = Visibility.Visible;

            // Автоматическое скрытие сообщения через 5 секунд
            var timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(5);
            timer.Tick += (s, e) =>
            {
                txtMessage.Visibility = Visibility.Collapsed;
                timer.Stop();
            };
            timer.Start();
        }
    }
}