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
using System.Windows.Shapes;

namespace WPFGame1
{

    /// <summary>
    /// Interaction logic for Pausee.xaml
    /// </summary>
    public partial class Pause : Window
    {
        Gammer parentWindow;

        public Pause()
        {
            InitializeComponent();
        }

        public Pause(Window wnd)
        {
            InitializeComponent();
            parentWindow = (Gammer)wnd;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                parentWindow.startApp();
                this.Close();
            }
        }
    }
}
