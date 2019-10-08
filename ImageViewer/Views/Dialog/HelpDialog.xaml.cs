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
using ImageViewer.Models;
using Markdig;
using Markdig.Renderers;

namespace ImageViewer.Views.Dialog
{
    /// <summary>
    /// Interaction logic for HelpDialog.xaml
    /// </summary>
    public partial class HelpDialog : Window
    {
        public bool IsValid { get; private set; } = true;

        public HelpDialog(ModelsEx models, string filename)
        {
            InitializeComponent();

            string text = "";
            try
            {
                // load text from file
                text = File.ReadAllText(filename);
            }
            catch (Exception)
            {
                models.Window.ShowErrorDialog("Could not open " + filename);
                Close();
                IsValid = false;
                return;
            } 

            //var pipe = new MarkdownPipeline();
            // convert markup into htm
            var pipe = new Markdig.MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
            var html = Markdig.Markdown.ToHtml(text, pipe);

            // get correct pixel colors
            var bg = (SolidColorBrush)FindResource("BackgroundBrush");
            var fg = (SolidColorBrush)FindResource("FontBrush");
            var bgCol = new byte[]{bg.Color.R, bg.Color.G, bg.Color.B};
            var fgCol = new byte[]{fg.Color.R, fg.Color.G, fg.Color.B};
            var bgColString = BitConverter.ToString(bgCol).Replace("-", String.Empty);
            var fgColString = BitConverter.ToString(fgCol).Replace("-", String.Empty);

            html = $@"
<body style=""background-color:#{bgColString}; color:#{fgColString};"">
{html}
</body>
";

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
