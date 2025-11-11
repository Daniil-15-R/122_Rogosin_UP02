using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Security.Cryptography;
using System.Text;

namespace _122_Rogosin_UP02.Pages
{
    public partial class RegPage : Page
    {
        public RegPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Вычисляет хеш-сумму пароля с использованием алгоритма SHA1
        /// </summary>
        /// <param name="password">Пароль для хеширования</param>
        /// <returns>Хеш-сумма пароля в шестнадцатеричном формате</returns>
        public static string GetHash(string password)
        {
            using (var hash = SHA1.Create())
            {
                return string.Concat(hash.ComputeHash(Encoding.UTF8.GetBytes(password)).Select(x => x.ToString("X2")));
            }
        }

        /// <summary>
        /// Обрабатывает нажатие кнопки регистрации пользователя
        /// </summary>
        private void regButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtbxLog.Text) ||
                string.IsNullOrEmpty(passBxFrst.Password) ||
                string.IsNullOrEmpty(passBxScnd.Password))
            {
                ShowError("Заполните все поля!");
                return;
            }

            // Проверка существования пользователя
            using (var db = new Entities())
            {
                var existingUser = db.Users_Login.FirstOrDefault(u => u.user_login == txtbxLog.Text);
                if (existingUser != null)
                {
                    ShowError("Пользователь с таким логином уже существует!");
                    return;
                }
            }

            // Валидация пароля
            if (passBxFrst.Password.Length < 6)
            {
                ShowError("Пароль слишком короткий, должно быть минимум 6 символов!");
                return;
            }

            bool en = true;
            bool number = false;

            for (int i = 0; i < passBxFrst.Password.Length; i++)
            {
                if (passBxFrst.Password[i] >= '0' && passBxFrst.Password[i] <= '9')
                    number = true;
                else if (!((passBxFrst.Password[i] >= 'A' && passBxFrst.Password[i] <= 'Z') ||
                          (passBxFrst.Password[i] >= 'a' && passBxFrst.Password[i] <= 'z')))
                    en = false;
            }

            if (!en)
            {
                ShowError("Используйте только английскую раскладку!");
                return;
            }
            else if (!number)
            {
                ShowError("Добавьте хотя бы одну цифру!");
                return;
            }

            if (passBxFrst.Password != passBxScnd.Password)
            {
                ShowError("Пароли не совпадают!");
                return;
            }

            try
            {
                using (var db = new Entities())
                {
                    // Создание записи в Users_Login
                    var newUserLogin = new Users_Login
                    {
                        user_login = txtbxLog.Text,
                    };

                    db.Users_Login.Add(newUserLogin);
                    db.SaveChanges();

                    // Создание записи в Users_Password
                    string hashedPassword = GetHash(passBxFrst.Password);
                    var newUserPassword = new Users_Password
                    {
                        ID = newUserLogin.ID,
                        user_password = hashedPassword
                    };

                    db.Users_Password.Add(newUserPassword);
                    db.SaveChanges();

                    MessageBox.Show("Пользователь успешно зарегистрирован!", "Успех",
                                  MessageBoxButton.OK, MessageBoxImage.Information);

                    // Очистка полей
                    ClearFields();

                    // Возврат на страницу авторизации
                    ButtonBack_Click(sender, e);
                }
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка при регистрации: {ex.Message}");
            }
        }

        private void ButtonBack_Click(object sender, RoutedEventArgs e)
        {
            // Возврат на страницу авторизации
            NavigationService?.GoBack();
        }

        private void ShowError(string message)
        {
            ErrorText.Text = message;
            ErrorText.Visibility = Visibility.Visible;

            // Анимация ошибки
            var animation = new System.Windows.Media.Animation.DoubleAnimation
            {
                From = -5,
                To = 5,
                Duration = TimeSpan.FromMilliseconds(50),
                AutoReverse = true,
                RepeatBehavior = new System.Windows.Media.Animation.RepeatBehavior(3)
            };

            var translate = new TranslateTransform();
            Border border = FindParent<Border>(txtbxLog);
            if (border != null)
            {
                border.RenderTransform = translate;
                translate.BeginAnimation(TranslateTransform.XProperty, animation);
            }
        }

        private T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);
            if (parentObject == null) return null;
            T parent = parentObject as T;
            if (parent != null) return parent;
            return FindParent<T>(parentObject);
        }

        private void ClearFields()
        {
            txtbxLog.Clear();
            passBxFrst.Clear();
            passBxScnd.Clear();

            lblLogHitn.Visibility = Visibility.Visible;
            lblPassHitn.Visibility = Visibility.Visible;
            lblPassSecHitn.Visibility = Visibility.Visible;
        }

        // Обработчики для hint-текстов
        private void txtbxLog_TextChanged(object sender, TextChangedEventArgs e)
        {
            lblLogHitn.Visibility = txtbxLog.Text.Length == 0 ? Visibility.Visible : Visibility.Hidden;
        }
        private void passBxFrst_PasswordChanged(object sender, RoutedEventArgs e)
        {
            lblPassHitn.Visibility = passBxFrst.Password.Length == 0 ? Visibility.Visible : Visibility.Hidden;
        }

        private void passBxScnd_PasswordChanged(object sender, RoutedEventArgs e)
        {
            lblPassSecHitn.Visibility = passBxScnd.Password.Length == 0 ? Visibility.Visible : Visibility.Hidden;
        }

        private void lblLogHitn_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            txtbxLog.Focus();
        }

        private void lblPassHitn_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            passBxFrst.Focus();
        }

        private void lblPassSecHitn_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            passBxScnd.Focus();
        }

    }
}