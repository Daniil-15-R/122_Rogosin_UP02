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
        private Ad _editingAd;

        // Событие для уведомления о добавлении/обновлении объявления
        public event EventHandler AdAdded;
        public event EventHandler AdUpdated;

        // Конструктор для создания нового объявления
        public AddAdsPage(int userId)
        {
            InitializeComponent();
            _currentUserId = userId;
            InitializePage();
        }

        // Конструктор для редактирования существующего объявления
        public AddAdsPage(int userId, Ad adToEdit)
        {
            InitializeComponent();
            _currentUserId = userId;
            _editingAd = adToEdit;
            InitializePage();
        }

        private void InitializePage()
        {
            try
            {
                context = new _122_Rogosin_UP02.Entities();
                LoadData();

                // Если редактируем существующее объявление - заполняем поля
                if (_editingAd != null)
                {
                    FillFormWithAdData();
                    btnSave.Content = "Обновить";
                    this.Title = "Редактирование объявления";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка инициализации: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FillFormWithAdData()
        {
            try
            {
                // Заполнение полей данными из объявления
                txtTitle.Text = _editingAd.Ad_Title1?.ad_title1 ?? "";
                txtDescription.Text = _editingAd.Ad_Description1?.ad_description1 ?? "";

                // Парсинг даты
                if (DateTime.TryParse(_editingAd.Ad_post_date1?.ad_post_date1, out DateTime postDate))
                {
                    dpPostDate.SelectedDate = postDate;
                }
                else
                {
                    dpPostDate.SelectedDate = DateTime.Today;
                }

                // Цена
                if (_editingAd.price.HasValue)
                {
                    txtPrice.Text = _editingAd.price.Value.ToString();
                }

                // Установка выбранных элементов в комбобоксы
                if (_editingAd.Category != null)
                    cmbCategory.SelectedValue = _editingAd.Category.ID;

                if (_editingAd.Ad_Type != null)
                    cmbAdType.SelectedValue = _editingAd.Ad_Type.ID;

                if (_editingAd.Ad_Status != null)
                    cmbAdStatus.SelectedValue = _editingAd.Ad_Status.ID;

                if (_editingAd.City != null)
                    cmbCity.SelectedValue = _editingAd.City.ID;

                if (_editingAd.Users_Login != null)
                    cmbUserLogin.SelectedValue = _editingAd.Users_Login.ID;

                if (_editingAd.Users_Password != null)
                    cmbUserPassword.SelectedValue = _editingAd.Users_Password.ID;
            }
            catch (Exception ex)
            {
                ShowMessage($"Ошибка при загрузке данных объявления: {ex.Message}", true);
            }
        }

        private void LoadData()
        {
            try
            {
                // Загрузка категорий
                cmbCategory.ItemsSource = context.Category.ToList();
                cmbCategory.DisplayMemberPath = "category1";
                cmbCategory.SelectedValuePath = "ID";

                // Загрузка типов объявлений
                cmbAdType.ItemsSource = context.Ad_Type.ToList();
                cmbAdType.DisplayMemberPath = "ad_type1";
                cmbAdType.SelectedValuePath = "ID";

                // Загрузка статусов объявлений
                cmbAdStatus.ItemsSource = context.Ad_Status.ToList();
                cmbAdStatus.DisplayMemberPath = "ad_status1";
                cmbAdStatus.SelectedValuePath = "ID";

                // Загрузка городов
                cmbCity.ItemsSource = context.City.ToList();
                cmbCity.DisplayMemberPath = "city1";
                cmbCity.SelectedValuePath = "ID";

                // Загрузка пользователей
                cmbUserLogin.ItemsSource = context.Users_Login.ToList();
                cmbUserLogin.DisplayMemberPath = "user_login1";
                cmbUserLogin.SelectedValuePath = "ID";

                cmbUserPassword.ItemsSource = context.Users_Password.ToList();
                cmbUserPassword.DisplayMemberPath = "user_password1";
                cmbUserPassword.SelectedValuePath = "ID";

                // Если создаем новое объявление - устанавливаем значения по умолчанию
                if (_editingAd == null)
                {
                    // Установка текущего пользователя по умолчанию
                    var currentUserLogin = context.Users_Login.FirstOrDefault(u => u.ID == _currentUserId);
                    var currentUserPassword = context.Users_Password.FirstOrDefault(u => u.ID == _currentUserId);

                    if (currentUserLogin != null)
                        cmbUserLogin.SelectedValue = currentUserLogin.ID;
                    if (currentUserPassword != null)
                        cmbUserPassword.SelectedValue = currentUserPassword.ID;

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
                if (_editingAd != null)
                {
                    UpdateAdvertisement();
                }
                else
                {
                    SaveAdvertisement();
                }
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Вы уверены, что хотите отменить? Все несохраненные данные будут потеряны.",
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

        private void UpdateAdvertisement()
        {
            try
            {
                // Обновление связанных записей
                if (_editingAd.Ad_Title1 != null)
                {
                    _editingAd.Ad_Title1.ad_title1 = txtTitle.Text.Trim();
                }
                else
                {
                    var newTitle = new Ad_Title { ad_title1 = txtTitle.Text.Trim() };
                    context.Ad_Title.Add(newTitle);
                    _editingAd.Ad_Title1 = newTitle;
                    _editingAd.ad_title = newTitle.ID;
                }

                if (_editingAd.Ad_Description1 != null)
                {
                    _editingAd.Ad_Description1.ad_description1 = txtDescription.Text.Trim();
                }
                else
                {
                    var newDescription = new Ad_Description { ad_description1 = txtDescription.Text.Trim() };
                    context.Ad_Description.Add(newDescription);
                    _editingAd.Ad_Description1 = newDescription;
                    _editingAd.ad_description = newDescription.ID;
                }

                // Обновление даты
                string postDateString = dpPostDate.SelectedDate?.ToString("yyyy-MM-dd") ?? DateTime.Today.ToString("yyyy-MM-dd");
                if (_editingAd.Ad_post_date1 != null)
                {
                    _editingAd.Ad_post_date1.ad_post_date1 = postDateString;
                }
                else
                {
                    var newPostDate = new Ad_post_date { ad_post_date1 = postDateString };
                    context.Ad_post_date.Add(newPostDate);
                    _editingAd.Ad_post_date1 = newPostDate;
                    _editingAd.ad_post_date = newPostDate.ID;
                }

                // Обновление выбранных значений
                _editingAd.Category = cmbCategory.SelectedItem as Category;
                _editingAd.Ad_Type = cmbAdType.SelectedItem as Ad_Type;
                _editingAd.Ad_Status = cmbAdStatus.SelectedItem as Ad_Status;
                _editingAd.City = cmbCity.SelectedItem as City;
                _editingAd.Users_Login = cmbUserLogin.SelectedItem as Users_Login;
                _editingAd.Users_Password = cmbUserPassword.SelectedItem as Users_Password;

                // Обновление ID
                _editingAd.category_id = (_editingAd.Category?.ID) ?? _editingAd.category_id;
                _editingAd.ad_type_id = (_editingAd.Ad_Type?.ID) ?? _editingAd.ad_type_id;
                _editingAd.ad_status_id = (_editingAd.Ad_Status?.ID) ?? _editingAd.ad_status_id;
                _editingAd.city_id = (_editingAd.City?.ID) ?? _editingAd.city_id;
                _editingAd.user_login_id = (_editingAd.Users_Login?.ID) ?? _editingAd.user_login_id;
                _editingAd.user_password_id = (_editingAd.Users_Password?.ID) ?? _editingAd.user_password_id;

                // Обновление цены
                _editingAd.price = string.IsNullOrWhiteSpace(txtPrice.Text) ? null : (double?)double.Parse(txtPrice.Text);

                context.SaveChanges();

                ShowMessage("Объявление успешно обновлено!", false);

                // Вызываем событие обновления объявления
                AdUpdated?.Invoke(this, EventArgs.Empty);

                // Возврат на предыдущую страницу
                if (NavigationService.CanGoBack)
                {
                    NavigationService.GoBack();
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Ошибка при обновлении: {ex.Message}", true);
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