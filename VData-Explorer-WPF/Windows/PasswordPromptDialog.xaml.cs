using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace VData_Explorer.Windows
{
    /// <summary>
    /// Interaction logic for PasswordPromptDialog.xaml
    /// </summary>
    public partial class PasswordPromptDialog : CustomDialog
    {
        public PasswordPromptDialog(MetroWindow window) : this(window, new MetroDialogSettings()) { }
        public PasswordPromptDialog(MetroWindow window, MetroDialogSettings settings) : base(window, settings)
        {
            InitializeComponent();

            string[] maybenullaswell = Settings.UsedPassword;
            if (maybenullaswell != null && maybenullaswell.Length > 0)
            {
                this.pwHistory.ItemsSource = maybenullaswell;
            }
        }

        public string Password => this.passwordbox.Password;
        public string Description { get => this.desc.Text; set => this.desc.Text = value; }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            this.RequestCloseAsync();
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            this.passwordbox.Password = string.Empty;
            this.RequestCloseAsync();
        }

        private void ButtonToggleHidePassword_Click(object sender, RoutedEventArgs e)
        {
            if (this.passwordbox.Visibility == Visibility.Visible)
                this.passwordbox.Visibility = Visibility.Collapsed;
            else
                this.passwordbox.Visibility = Visibility.Visible;
        }

        private void Passwordbox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (this.passwordbox.Password == this.plainpasswordbox.Text) return;
            this.plainpasswordbox.Text = this.passwordbox.Password;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems != null && e.AddedItems.Count > 0)
            {
                string pw2 = (string)e.AddedItems[0];
                this.plainpasswordbox.Text = pw2;
                this.passwordbox.Password = pw2;
            }
        }

        private void ButtonSuggestDropDown_Click(object sender, RoutedEventArgs e)
        {
            if (this.pwHistory.Items.Count == 0) return;
            if (this.pwHistory.IsDropDownOpen)
                this.pwHistory.IsDropDownOpen = false;
            else
                this.pwHistory.IsDropDownOpen = true;
        }
    }
}
