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

namespace SingDictionaryWPF
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        private OnWindowClosing mainWindow;
        public AboutWindow(OnWindowClosing mainWindow)
        {
            InitializeComponent();
            this.mainWindow = mainWindow;
        }
        public interface OnWindowClosing
        {
            void onWindowClosing();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (mainWindow != null)
            {
                mainWindow.onWindowClosing();
            }
            
        }

       
        

        
    }
}
