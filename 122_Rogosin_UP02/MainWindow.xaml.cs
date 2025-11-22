using System;
using System.Linq;
using System.Windows;
using System.Windows.Navigation;

namespace _122_Rogosin_UP02
{
    public partial class MainWindow : Window
    {
        private int _currentUserId = -1;
        public MainWindow()
        {
            InitializeComponent();
            InitializeApplication();
        }

        private void InitializeApplication()
        {
            MainFrame.Navigate(new Pages.AdsPage());

            InitializeTimer();

            UpdateUserStatus();
        }

        private void InitializeTimer()
        {
            var timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += (sender, e) =>
            {
                DateTimeText.Text = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
            };
            timer.Start();
        }

        private void UpdateUserStatus()
        {
            UserStatusText.Text = "Не авторизован";
        }


        private void BtnHome_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Pages.AdsPage());
        }

        private void BtnMyAds_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUserId == -1)
            {
                MessageBox.Show("Для просмотра объявлений необходимо войти в систему",
                              "Ошибка авторизации",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                MainFrame.Navigate(new Pages.AuthPage());
            }
            else
            {
                MainFrame.Navigate(new Pages.UserAdsPage(_currentUserId));
            }
        }

        private void BtnCompleted_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUserId == -1)
            {
                MessageBox.Show("Для просмотра завершенных объявлений необходимо войти в систему",
                              "Ошибка авторизации",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                MainFrame.Navigate(new Pages.AuthPage());
            }
            else
            {
                MainFrame.Navigate(new Pages.CompletedAdsPage(_currentUserId));
            }
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Pages.AuthPage());
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (MainFrame.CanGoBack)
            {
                MainFrame.GoBack();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var result = MessageBox.Show("Вы действительно хотите выйти?", "Подтверждение выхода",
                                        MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.No)
            {
                e.Cancel = true;
            }
        }

        public void OnUserLoggedIn(string username)
        {
            UserStatusText.Text = $"Пользователь: {username}";
            BtnLogin.Visibility = Visibility.Collapsed;
            BtnMyAds.Visibility = Visibility.Visible;
            BtnCompleted.Visibility = Visibility.Visible;

            using (var db = new Entities())
            {
                var user = db.Users_Login.FirstOrDefault(u => u.user_login == username);
                if (user != null)
                {
                    _currentUserId = user.ID;
                }
            }
            MainFrame.Navigate(new Pages.AdsPage());
        }

        public void OnUserLoggedOut()
        {
            _currentUserId = -1;
            UserStatusText.Text = "Не авторизован";
            BtnLogin.Visibility = Visibility.Visible;
            BtnMyAds.Visibility = Visibility.Collapsed;
            BtnCompleted.Visibility = Visibility.Collapsed;

            MainFrame.Navigate(new Pages.AuthPage());
        }
    }
}