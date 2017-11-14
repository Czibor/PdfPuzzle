using System.Windows.Data;

namespace PdfPuzzleView
{
    public class SettingBindingExtension : Binding
    {
        public SettingBindingExtension()
        {
            Initialize();
        }

        public SettingBindingExtension(string path) : base(path)
        {
            Initialize();
        }

        private void Initialize()
        {
            this.Source = PdfPuzzle.Properties.Settings.Default;
            this.Mode = BindingMode.TwoWay;
        }
    }
}