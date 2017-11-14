using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using PdfPuzzle;

namespace PdfPuzzleViewModel
{
    public class PuzzleViewModel : ObservableObject, IDataErrorInfo
    {
        private int? _selectedPageNumber;
        private string _selectedFile;
        private string _outputFolder;
        private string _fileName;
        private List<int> _availablePageNumbers;
        public ObservableCollection<PdfPageData> _pdfPageList = new ObservableCollection<PdfPageData>();
        private RelayCommand _fileDialog;
        private RelayCommand _folderDialog;
        private RelayCommand _addPage;
        private RelayCommand _deletePage;
        private RelayCommand _boostPage;
        private RelayCommand _startAddition;
        
        public string SelectedFile
        {
            get
            {
                return _selectedFile;
            }
            set
            {
                _selectedFile = value;
                NotifyPropertyChanged("SelectedFile");
            }
        }

        public int? SelectedPageNumber
        {
            get
            {
                return _selectedPageNumber;
            }
            set
            {
                _selectedPageNumber = value;
                NotifyPropertyChanged("SelectedPageNumber");
            }
        }

        public List<int> AvailablePageNumbers
        {
            get
            {
                return _availablePageNumbers;
            }
            set
            {
                _availablePageNumbers = value;
                NotifyPropertyChanged("AvailablePageNumbers");
            }
        }

        public ObservableCollection<PdfPageData> PdfPageList
        {
            get
            {
                return _pdfPageList;
            }
            set
            {
                _pdfPageList = value;
                NotifyPropertyChanged("PdfPageList");
            }
        }

        public string OutputFolder
        {
            get
            {
                return _outputFolder;
            }
            set
            {
                _outputFolder = value;
                NotifyPropertyChanged("OutputFolder");
            }
        }

        public string FileName
        {
            get
            {
                return _fileName;
            }
            set
            {
                _fileName = value;
                NotifyPropertyChanged("FileName");
            }
        }

        public ICommand FileDialog
        {
            get
            {
                return _fileDialog ?? (_fileDialog = new RelayCommand(param => ShowDialog()));
            }
        }

        public ICommand FolderDialog
        {
            get
            {
                return _folderDialog ?? (_folderDialog = new RelayCommand(param => ShowFolderDialog()));
            }
        }

        public ICommand AddPage
        {
            get
            {
                return _addPage ?? (_addPage = new RelayCommand(param => AddPageToList()));
            }
        }

        public ICommand DeletePage
        {
            get
            {
                return _deletePage ?? (_deletePage = new RelayCommand(param => DeletePageFromList(param)));
            }
        }

        public ICommand BoostPage
        {
            get
            {
                return _boostPage ?? (_boostPage = new RelayCommand(param => BoostSelectedPage(param)));
            }
        }

        public ICommand StartAddition
        {
            get
            {
                return _startAddition ?? (_startAddition = new RelayCommand(param => AddPdf()));
            }
        }

        private void ShowDialog()
        {
            string chosenFile = Parameters.ShowFileDialog();

            if (chosenFile != null)
            {
                SelectedFile = chosenFile;
                AvailablePageNumbers = Enumerable.Range(1, Parameters.GetPdfLastPage(SelectedFile)).ToList();
                SelectedPageNumber = 1;

                if (FileName == null)
                {
                    FileName = Path.GetFileNameWithoutExtension(SelectedFile) + "_concat.pdf";
                }
            }
        }

        private void ShowFolderDialog()
        {
            string chosenFolder = Parameters.ShowFolderDialog();

            if (chosenFolder != null)
            {
                OutputFolder = chosenFolder;
            }
        }

        private void AddPageToList()
        {
            if (PdfPageList.Count == 4)
            {
                MessageBox.Show("You have already added 4 items.");
            }
            else if (string.IsNullOrEmpty(SelectedFile) || SelectedPageNumber == null)
            {
                PdfPageList.Add(new PdfPageData(null, 0));
            }
            else if (!Parameters.ValidatePdfFile(SelectedFile))
            {
                MessageBox.Show("The given PDF is not valid.");
            }
            else
            {
                PdfPageList.Add(new PdfPageData(SelectedFile, Convert.ToInt32(SelectedPageNumber)));
                SelectedPageNumber = null;
            }
        }

        private void DeletePageFromList(object selectedIndexParam)
        {
            int selectedIndex = (int)selectedIndexParam;

            if (selectedIndex >= 0)
            {
                PdfPageList.RemoveAt(Convert.ToInt32(selectedIndex));
            }
        }

        private void BoostSelectedPage(object selectedIndexParam)
        {
            int selectedIndex = (int)selectedIndexParam;

            if (selectedIndex > 0)
            {
                PdfPageList.Move(selectedIndex, selectedIndex - 1);
            }
        }

        private void AddPdf()
        {
            if (Parameters.ValidateOutputFolder(OutputFolder) != null)
            {
                if (string.IsNullOrEmpty(OutputFolder))
                {
                    MessageBox.Show("Please give the output folder.");
                }
                else
                {
                    MessageBox.Show("Please provide a folder that exists or can be created.");
                }
            }
            else if (!Parameters.ValidateFileName(FileName))
            {
                if (string.IsNullOrEmpty(FileName))
                {
                    MessageBox.Show("Please give the filename.");
                }
                else
                {
                    MessageBox.Show("Please provide a valid filename.");
                }
            }
            else if (PdfPageList.All(item => string.IsNullOrEmpty(item.FilePath)))
            {
                MessageBox.Show("Please choose at least 1 valid page.");
            }
            else
            {
                if (Path.GetExtension(FileName) != ".pdf")
                {
                    FileName += ".pdf";
                }

                Task.Run(() =>
                {
                    try
                    {
                        string outputFullName = Path.Combine(OutputFolder, FileName);
                        PdfPageCreator.AddPages(PdfPageList, OutputFolder, FileName);
                        Process.Start(outputFullName);
                        PdfPageList = new ObservableCollection<PdfPageData>();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                });
            }
        }

        public string Error { get { return string.Empty; } }

        public string this[string propertyName]
        {
            get
            {
                string errorMessage = null;

                if (propertyName == "SelectedFile")
                {
                    if (!string.IsNullOrEmpty(SelectedFile))
                    {
                        if (!Parameters.ValidatePdfFile(SelectedFile))
                        {
                            errorMessage = "The selected PDF is not valid.";
                        }
                    }
                }
                else if (propertyName == "OutputFolder")
                {
                    if (!string.IsNullOrEmpty(OutputFolder))
                    {
                        errorMessage = Parameters.ValidateOutputFolder(OutputFolder);

                        if (!string.IsNullOrEmpty(FileName))
                        {
                            NotifyPropertyChanged("FileName");
                        }
                    }
                }
                else if (propertyName == "FileName")
                {
                    if (!string.IsNullOrEmpty(FileName))
                    {
                        if (!Parameters.ValidateFileName(FileName))
                        {
                            errorMessage = "The filename contains invalid character(s).";
                        }
                        else if (!string.IsNullOrEmpty(OutputFolder))
                        {
                            if (Parameters.PdfFileExists(Path.Combine(OutputFolder, FileName)))
                            {
                                errorMessage = "The file already exists. It will be overwritten.";
                            }
                        }
                    }
                }

                return errorMessage;
            }
        }
    }
}