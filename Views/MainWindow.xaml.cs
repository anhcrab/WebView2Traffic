using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using WebView2Traffic.ViewModels;

namespace WebView2Traffic.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            //var bitmap = new Bitmap("\\Assets\\icon.png");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Clicked");
            ((MainWindowViewModel) DataContext).ReloadDataCommand.Execute(sender);
        }
    }
}
