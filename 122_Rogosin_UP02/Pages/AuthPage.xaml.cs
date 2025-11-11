using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Input;

namespace _122_Rogosin_UP02.Pages
{
    public partial class AuthPage : Page
    {
        private int failedAttempts = 0;

        public AuthPage()
        {
            InitializeComponent();
            Loaded += (s, e) => TextBoxLogin.Focus();
        }

        private void ButtonLogin_Click(object sender, RoutedEventArgs e)
        {
            string login = TextBoxLogin.Text.Trim();
            string password = PasswordBox.Password;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                ShowError("Введите логин и пароль");
                return;
            }

            string hashedPassword = GetHash(password);

            using (var db = new Entities())
            {
                var user = db.Users_Login
                    .Join(db.Users_Password,
                          ul => ul.ID,
                          up => up.ID,
                          (ul, up) => new { UserLogin = ul, UserPassword = up })
                    .FirstOrDefault(u => u.UserLogin.user_login == login &&
                        (u.UserPassword.user_password == hashedPassword ||
                         u.UserPassword.user_password == password));

                if (user == null)
                {
                    ShowError("Пользователь с такими данными не найден!");
                    failedAttempts++;
                    if (failedAttempts >= 3)
                    {
                        ShowError("Превышено количество попыток входа. Обратитесь к администратору.");
                        ButtonLogin.IsEnabled = false;
                    }
                    return;
                }
                else
                {
                    // Если пароль был не хеширован - хешируем его и обновляем в БД
                    if (user.UserPassword.user_password == password)
                    {
                        user.UserPassword.user_password = hashedPassword;
                        db.SaveChanges();
                    }

                    failedAttempts = 0;
                    LoginSuccess(login);
                }
            }
        }

        private void ButtonRegister_Click(object sender, RoutedEventArgs e)
        {
            // Переход на страницу регистрации
            NavigationService?.Navigate(new RegPage());
        }

        /// <summary>
        /// Генерирует SHA1 хэш для пароля
        /// </summary>
        public static string GetHash(string password)
        {
            using (var hash = SHA1.Create())
            {
                return string.Concat(hash.ComputeHash(Encoding.UTF8.GetBytes(password)).Select(x => x.ToString("X2")));
            }
        }

        private void LoginSuccess(string username)
        {
            ErrorText.Visibility = Visibility.Collapsed;

            PasswordBox.Password = "";

            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                mainWindow.OnUserLoggedIn(username);
            }

            MessageBox.Show($"Добро пожаловать, {username}!", "Успешный вход",
                          MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ShowError(string message)
        {
            ErrorText.Text = message;
            ErrorText.Visibility = Visibility.Visible;

            var animation = new System.Windows.Media.Animation.DoubleAnimation
            {
                From = -5,
                To = 5,
                Duration = TimeSpan.FromMilliseconds(50),
                AutoReverse = true,
                RepeatBehavior = new System.Windows.Media.Animation.RepeatBehavior(3)
            };

            var translate = new System.Windows.Media.TranslateTransform();
            Border border = (Border)VisualTreeHelper.GetParent(VisualTreeHelper.GetParent(TextBoxLogin));
            border.RenderTransform = translate;
            translate.BeginAnimation(TranslateTransform.XProperty, animation);
        }

        private void TextBoxLogin_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                PasswordBox.Focus();
            }
        }

        private void PasswordBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ButtonLogin_Click(sender, e);
            }
        }

        private void GuestText_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            LoginSuccess("Гость");
        }
    }
}