using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace play2wifi
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            try
            {
                taskbarIcon.Icon = System.Drawing.Icon.FromHandle(new System.Drawing.Bitmap(Application.GetResourceStream(new Uri("Crystal_Clear_app_lsongs.png",UriKind.Relative)).Stream).GetHicon());
            }
            catch (Exception e) {
                System.Diagnostics.Debug.WriteLine(e);
            }
            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
