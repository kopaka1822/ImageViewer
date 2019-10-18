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
using Path = System.IO.Path;

namespace ImageViewer.Views.Dialog
{
    /// <summary>
    /// Interaction logic for HelpDialog.xaml
    /// </summary>
    public partial class HelpDialog : Window
    {
        public bool IsValid { get; private set; } = true;
        private string curDirectory;
        private readonly ModelsEx models;
        private Stack<string> history;

        public HelpDialog(ModelsEx models, string filename)
        {
            InitializeComponent();
            this.models = models;
            history = new Stack<string>();

            LoadPage(filename);

            // set window title
            Title = "Help - Version " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

            Browser.Navigating += BrowserOnNavigating;
        }

        private void LoadPage(string filename, bool isBacklink = false)
        {
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

            curDirectory = Path.GetFullPath(Path.GetDirectoryName(filename));
            curDirectory += '/';

            var fileDirectory = curDirectory.Replace('\\', '/');
            fileDirectory = "file:///" + fileDirectory + "/";

            // correct links
            text = text.Replace("![](", "![](" + fileDirectory);

            // add back link
            if (history.Count > 0)
            {
                text += "\n[Back](back)";
            }
            
            history.Push(filename);

            //var pipe = new MarkdownPipeline();
            // convert markup into htm
            var pipe = new Markdig.MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
            var html = Markdig.Markdown.ToHtml(text, pipe);

            // get correct pixel colors
            var bg = (SolidColorBrush)FindResource("BackgroundBrush");
            var fg = (SolidColorBrush)FindResource("FontBrush");
            var bgCol = new byte[] { bg.Color.R, bg.Color.G, bg.Color.B };
            var fgCol = new byte[] { fg.Color.R, fg.Color.G, fg.Color.B };
            var bgColString = BitConverter.ToString(bgCol).Replace("-", String.Empty);
            var fgColString = BitConverter.ToString(fgCol).Replace("-", String.Empty);

            html = $@"
<body style=""background-color:#{bgColString}; color:#{fgColString};"">
{html}
</body>
";
            // display markup in browser
            Browser.NavigateToString(html);
        }

        private void BrowserOnNavigating(object sender, NavigatingCancelEventArgs args)
        {
            if (args.Uri == null) return;

            // dont open web page in the embedded browser
            if (args.Uri.ToString().StartsWith("http") || args.Uri.ToString().StartsWith("www"))
            {
                args.Cancel = true;
                System.Diagnostics.Process.Start(args.Uri.ToString());
            }
            else if (args.Uri.LocalPath == "back" && history.Count > 1)
            {
                args.Cancel = true;
                history.Pop();
                LoadPage(history.Pop(), true);
            }
            else
            {
                args.Cancel = true;
                // open other markdown page
                LoadPage(curDirectory + args.Uri.LocalPath);
            }
        }

        private void OkOnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
