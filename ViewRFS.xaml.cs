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
using System.Windows.Shapes;
using System.Runtime.InteropServices;

namespace RoofFrameSchedule
{
    /// <summary>
    /// Логика взаимодействия для ViewRFS.xaml
    /// </summary>
    public partial class ViewRFS : Window, IDisposable
    {
        [DllImport("user32.dll")]
        internal extern static int SetWindowLong(IntPtr hwnd, int index, int value);

        [DllImport("user32.dll")]
        internal extern static int GetWindowLong(IntPtr hwnd, int index);

        internal static void HideMinimizeAndMaximizeButtons(Window window)
        {
            const int GWL_STYLE = -16;

            IntPtr hwnd = new System.Windows.Interop.WindowInteropHelper(window).Handle;
            long value = GetWindowLong(hwnd, GWL_STYLE);

            SetWindowLong(hwnd, GWL_STYLE, (int)(value & -131073 & -65537));
        }

        public ViewRFS()
        {
            InitializeComponent();
        }

        public void Dispose()
        {
            this.Close();
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            HideMinimizeAndMaximizeButtons(this);
        }

        //private void checkBoxSelectDraftingView_Click(object sender, RoutedEventArgs e)
        //{
        //    if (checkBoxSelectDraftingView.IsChecked == true)
        //    {
        //        comboBoxDraftingViews.IsEnabled = true;
        //        textBoxViewDraftingName.IsEnabled = false;
        //    }
        //    else
        //    {
        //        comboBoxDraftingViews.IsEnabled = false;
        //        textBoxViewDraftingName.IsEnabled = true;
        //    }
        //}
    }
}
