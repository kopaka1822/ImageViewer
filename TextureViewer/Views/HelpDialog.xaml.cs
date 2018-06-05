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
using System.Windows.Shapes;
using Markdig;

namespace TextureViewer.Views
{
    /// <summary>
    /// Interaction logic for HelpDialog.xaml
    /// </summary>
    public partial class HelpDialog : Window
    {
        public HelpDialog(string filename)
        {
            InitializeComponent();

            // load text from file
            var text = File.ReadAllText(filename);

            // convert markup into htm
            var pipe = new Markdig.MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
            var html = Markdig.Markdown.ToHtml(text, pipe);

            // display markup in browser
            Browser.NavigateToString(html);
        }

        private void OkOnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
