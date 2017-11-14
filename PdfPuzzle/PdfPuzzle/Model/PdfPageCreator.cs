using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace PdfPuzzle
{
    public static class PdfPageCreator
    {
        public static void AddPages(ObservableCollection<PdfPageData> pageList, string outputFolder, string fileName)
        {
            using (Document document = new Document())
            using (PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(Path.Combine(outputFolder, fileName), FileMode.Create)))
            {
                ObservableCollection<PdfPageData> pdfPageList = new ObservableCollection<PdfPageData>(pageList);
                int[] multipliers = GetPageSizeMultipliers(pdfPageList);
                Rectangle rectangle = GetMaxWidthAndHeight(pdfPageList, multipliers[0], multipliers[1]);
                document.Open();
                document.SetPageSize(rectangle);
                document.NewPage();

                for (int i = 0; i < pdfPageList.Count; ++i)
                {
                    if (!string.IsNullOrEmpty(pdfPageList[i].FilePath))
                    {
                        PdfReader reader = new PdfReader(pdfPageList[i].FilePath);
                        PdfImportedPage importedPage = writer.GetImportedPage(reader, pdfPageList[i].PageNumber);
                        float left = GetLeftDistance(rectangle.Width / multipliers[0], importedPage.Width, i + 1);
                        float top = GetTopDistance(rectangle.Height / multipliers[1], importedPage.Height, i + 1, multipliers[1] == 1);
                        writer.DirectContent.AddTemplate(importedPage, left, top);
                    }
                }
                
                document.Close();
            }
        }

        /// <summary>
        /// Returns width and height multipliers. Also moves pages if needed.
        /// </summary>
        private static int[] GetPageSizeMultipliers(ObservableCollection<PdfPageData> pdfPageList)
        {
            List<int> pdfPositions = new List<int>();

            for (int i = 0; i < pdfPageList.Count; ++i)
            {
                if (!string.IsNullOrEmpty(pdfPageList[i].FilePath))
                {
                    pdfPositions.Add(i + 1);
                }
            }

            if (pdfPositions.Count != 2)
            {
                return new int[2] { 2, 2 };
            }
            else
            {
                switch (pdfPositions[0] + pdfPositions[1])
                {
                    case 3:
                        return new int[2] { 2, 1 };
                    case 4:
                        return new int[2] { 1, 2 };
                    case 6:
                        pdfPageList.Move(1, 0);
                        pdfPageList.Move(3, 2);
                        return new int[2] { 1, 2 };
                    case 7:
                        pdfPageList.Move(2, 0);
                        pdfPageList.Move(3, 1);
                        return new int[2] { 2, 1 };
                    default:
                        return new int[2] { 2, 2 };
                }
            }
        }

        private static Rectangle GetMaxWidthAndHeight(ObservableCollection<PdfPageData> pdfPageList, float widthMultiplier = 1, float heightMultiplier = 1)
        {
            float pageWidth = 0;
            float pageHeight = 0;
            List<int> pagePositions = new List<int>();

            for (int i = 0; i < pdfPageList.Count; ++i)
            {
                if (!string.IsNullOrEmpty(pdfPageList[i].FilePath))
                {
                    using (PdfReader reader = new PdfReader(pdfPageList[i].FilePath))
                    {
                        Rectangle rectangle = reader.GetPageSize(pdfPageList[i].PageNumber);
                        pagePositions.Add(i + 1);

                        if (rectangle.Width > pageWidth)
                        {
                            pageWidth = rectangle.Width;
                        }

                        if (rectangle.Height > pageHeight)
                        {
                            pageHeight = rectangle.Height;
                        }
                    }
                }
            }

            return new Rectangle(pageWidth * widthMultiplier, pageHeight * heightMultiplier);
        }

        private static float GetLeftDistance(float pageWidth, float subPageWidth, int position)
        {
            if (position == 1 || position == 3)
            {
                return pageWidth - subPageWidth;
            }
            else
            {
                return pageWidth;
            }
        }

        private static float GetTopDistance(float pageHeight, float subPageHeight, int position, bool halfSize = false)
        {
            if (halfSize)
            {
                return 0;
            }
            else if (position == 1 || position == 2)
            {
                return pageHeight;
            }
            else
            {
                return pageHeight - subPageHeight;
            }
        }
    }
}