using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Xps.Packaging;
using System.Globalization;
using MahApps.Metro.Controls.Dialogs;
using NLog;

namespace Chronos.UserControls
{
    /// <summary>
    /// Interaktionslogik für AboutControl.xaml
    /// </summary>
    public partial class AboutControl : UserControl
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public AboutControl()
        {
            InitializeComponent();

            try
            {
                string country = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
                if (country.Length > 0) 
                {
                    string docname = String.Format("Docs/readme-{0}.xps", country);
                    XpsDocument xpsDoc = new XpsDocument(docname, FileAccess.Read);
                    help_viewer.Document = xpsDoc.GetFixedDocumentSequence();
                } 
                else
                {
                    string docname = String.Format("Docs/readme-{0}.xps", "en");
                    XpsDocument xpsDoc = new XpsDocument(docname, FileAccess.Read);
                    help_viewer.Document = xpsDoc.GetFixedDocumentSequence();
                }
            }
            catch (Exception ex)
            {
                Logger.Error(Properties.Resources.ReadReadmeFail + ex.Message);
                ShowError(ex.Message);
            }
        }

        public async void ShowError(string message)
        {
            MainWindow mw = Application.Current.MainWindow as MainWindow;
            await mw.ShowMessageAsync("Error", message);
        }
    }
}
