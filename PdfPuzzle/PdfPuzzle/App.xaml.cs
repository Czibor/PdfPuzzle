namespace PdfPuzzleView
{
    public partial class App
    {
        private void Application_Exit(object sender, System.Windows.ExitEventArgs e)
        {
            PdfPuzzle.Properties.Settings.Default.Save();
        }
    }
}