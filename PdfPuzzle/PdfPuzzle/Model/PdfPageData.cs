namespace PdfPuzzle
{
    public class PdfPageData
    {
        public PdfPageData(string filePath, int pageNumber)
        {
            FilePath = filePath;
            PageNumber = pageNumber;
        }

        public string FilePath { get; set; }
        public int PageNumber { get; set; }
    }
}