using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Shapes;
using Markdig;

namespace TextureViewer.Views
{
    /// <summary>
    /// Interaction logic for HelpDialog.xaml
    /// </summary>
    public partial class HelpDialog : Window
    {
        public bool IsValid { get; private set; } = true;

        public HelpDialog(string filename)
        {
            InitializeComponent();

            string text = "";
            try
            {
                // load text from file
                text = File.ReadAllText(filename);
            }
            catch(Exception)
            {
                App.ShowErrorDialog(this, "Could not open " + filename);
                Close();
                IsValid = false;
                return;
            }

            // convert markup into htm
            var pipe = new Markdig.MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
            var html = Markdig.Markdown.ToHtml(text, pipe);

            // display markup in browser
            Browser.NavigateToString(html);
            Browser.Navigating += BrowserOnNavigating;
        }

        private void BrowserOnNavigating(object sender, NavigatingCancelEventArgs args)
        {
            // dont open web page in the embedded browser
            if (args.Uri.ToString().StartsWith("http") || args.Uri.ToString().StartsWith("www"))
            {
                args.Cancel = true;
                System.Diagnostics.Process.Start(args.Uri.ToString());
            }
        }

        private void OkOnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
