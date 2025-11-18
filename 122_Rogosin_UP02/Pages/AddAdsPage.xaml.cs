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
            context = new _122_Rogosin_UP02.Entities();
            // Перезагружаем сущность из текущего контекста
            _editingAd = context.Ad.Find(adToEdit.ID);
            InitializePage();
        }

        private void InitializePage()
        {
            try
            {
                if (context == null)
                {
                    context = new _122_Rogosin_UP02.Entities();
                }
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

                // Установка выбранных элементов в комбобоксы по ID
                if (_editingAd.category_id > 0)
                    cmbCategory.SelectedValue = _editingAd.category_id;

                if (_editingAd.ad_type_id > 0)
                    cmbAdType.SelectedValue = _editingAd.ad_type_id;

                if (_editingAd.ad_status_id > 0)
                    cmbAdStatus.SelectedValue = _editingAd.ad_status_id;

                if (_editingAd.city_id > 0)
                    cmbCity.SelectedValue = _editingAd.city_id;
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

                // Убрана загрузка пользователей, так как форма теперь независима от логина и пароля

                // Если создаем новое объявление - устанавливаем значения по умолчанию
                if (_editingAd == null)
                {
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

            // Убраны проверки логина и пароля пользователя

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

                    // Прямые поля (ID)
                    ad_title = adTitle.ID,
                    ad_description = adDescription.ID,
                    ad_post_date = adPostDate.ID,
                    city_id = selectedCity.ID,
                    category_id = selectedCategory.ID,
                    ad_type_id = selectedAdType.ID,
                    ad_status_id = selectedAdStatus.ID,

                    // Используем текущего пользователя для связи
                    user_login_id = _currentUserId,
                    user_password_id = _currentUserId,

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
                // Получаем выбранные объекты
                var selectedCategory = cmbCategory.SelectedItem as Category;
                var selectedAdType = cmbAdType.SelectedItem as Ad_Type;
                var selectedAdStatus = cmbAdStatus.SelectedItem as Ad_Status;
                var selectedCity = cmbCity.SelectedItem as City;

                // ПРОВЕРКА ВЫБРАННЫХ ЗНАЧЕНИЙ
                if (selectedCategory == null) throw new Exception("Категория не выбрана");
                if (selectedAdType == null) throw new Exception("Тип объявления не выбран");
                if (selectedAdStatus == null) throw new Exception("Статус объявления не выбран");
                if (selectedCity == null) throw new Exception("Город не выбран");

                // ОБНОВЛЕНИЕ СВЯЗАННЫХ ЗАПИСЕЙ С СОХРАНЕНИЕМ

                // Обновление заголовка
                if (_editingAd.Ad_Title1 != null)
                {
                    _editingAd.Ad_Title1.ad_title1 = txtTitle.Text.Trim();
                }
                else
                {
                    var newTitle = new Ad_Title { ad_title1 = txtTitle.Text.Trim() };
                    context.Ad_Title.Add(newTitle);
                    context.SaveChanges(); // Сохраняем для получения ID
                    _editingAd.ad_title = newTitle.ID;
                }

                // Обновление описания
                if (_editingAd.Ad_Description1 != null)
                {
                    _editingAd.Ad_Description1.ad_description1 = txtDescription.Text.Trim();
                }
                else
                {
                    var newDescription = new Ad_Description { ad_description1 = txtDescription.Text.Trim() };
                    context.Ad_Description.Add(newDescription);
                    context.SaveChanges(); // Сохраняем для получения ID
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
                    context.SaveChanges(); // Сохраняем для получения ID
                    _editingAd.ad_post_date = newPostDate.ID;
                }

                // ОБНОВЛЕНИЕ ОСНОВНЫХ ПОЛЕЙ
                _editingAd.category_id = selectedCategory.ID;
                _editingAd.ad_type_id = selectedAdType.ID;
                _editingAd.ad_status_id = selectedAdStatus.ID;
                _editingAd.city_id = selectedCity.ID;

                // Обновление цены
                _editingAd.price = string.IsNullOrWhiteSpace(txtPrice.Text) ? null : (double?)double.Parse(txtPrice.Text);

                context.SaveChanges();

                ShowMessage("Объявление успешно обновлено!", false);
                AdUpdated?.Invoke(this, EventArgs.Empty);

                if (NavigationService.CanGoBack)
                {
                    NavigationService.GoBack();
                }
            }
            catch (Exception ex)
            {
                string errorMessage = $"Ошибка при обновлении: {ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMessage += $"\nВнутренняя ошибка: {ex.InnerException.Message}";
                }
                ShowMessage(errorMessage, true);
                System.Diagnostics.Debug.WriteLine($"ОШИБКА ОБНОВЛЕНИЯ: {ex}");
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