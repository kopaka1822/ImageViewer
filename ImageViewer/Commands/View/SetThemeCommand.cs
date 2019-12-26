using ImageViewer.Commands.Helper;
using ImageViewer.Models;
using ImageViewer.Views.Theme;

namespace ImageViewer.Commands.View
{
    public class SetThemeCommand : SimpleCommand<int>
    {
        private readonly ModelsEx models;
        private ThemeDictionary.Themes current;

        public SetThemeCommand(ModelsEx models)
        {
            this.models = models;
            current = models.Settings.Theme;
        }

        public override void Execute(int parameter)
        {
            models.Settings.Theme = (ThemeDictionary.Themes) parameter;

            if (models.Window.ShowYesNoDialog(
                "You have to restart the application for this change to take effect. Do you want to restart the application?",
                "Restart Application?"))
            {
                models.Settings.Save();
                models.Window.Window.Close();

                var info = new System.Diagnostics.ProcessStartInfo(models.Window.AssemblyPath);
                System.Diagnostics.Process.Start(info);
            }
        }

        public bool DefaultEnabled => current != ThemeDictionary.Themes.Default;

        public bool WhiteEnabled => current != ThemeDictionary.Themes.White;

        public bool DarkEnabled => current != ThemeDictionary.Themes.Dark;
    }
}
