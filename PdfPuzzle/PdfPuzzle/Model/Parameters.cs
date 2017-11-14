using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Windows.Forms;
using iTextSharp.text.pdf;

namespace PdfPuzzle
{
    public static class Parameters
    {
        public static bool ValidatePdfFile(string file)
        {
            try
            {
                using (PdfReader reader = new PdfReader(file))
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public static int GetPdfLastPage(string inputFile)
        {
            try
            {
                PdfReader reader = new PdfReader(inputFile);
                return reader.NumberOfPages;
            }
            catch
            {
                return 0;
            }
        }

        public static List<string> GetPdfFiles(string folder)
        {
            return Directory.GetFiles(folder, "*.pdf", SearchOption.TopDirectoryOnly).ToList();
        }

        public static List<string> RemoveInvalidPdfFromList(List<string> pdfList)
        {
            for (int i = pdfList.Count - 1; i >= 0; --i)
            {
                if (!Parameters.ValidatePdfFile(pdfList[i]))
                {
                    pdfList.RemoveAt(i);
                }
            }

            return pdfList;
        }

        public static string ReadAccess(string folder)
        {
            try
            {
                if (Path.GetFullPath(folder) != folder)
                {
                    return "The folder name is not valid.";
                }
            }
            catch
            {
                return "The folder name is not valid.";
            }

            if (Directory.Exists(folder))
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(folder);

                try
                {
                    DirectorySecurity acl = directoryInfo.GetAccessControl();
                    return null;
                }
                catch (UnauthorizedAccessException uae)
                {
                    if (uae.Message.ToUpper().Contains("READ-ONLY"))
                    {
                        return null;
                    }
                    else
                    {
                        return "You don't have read access to the given folder.";
                    }
                }
                catch
                {
                    return "The folder can't be reached by the program.";
                }
            }
            else
            {
                return "The folder doesn't exist or can't be reached.";
            }
        }

        public static string ValidateOutputFolder(string folder)
        {
            try
            {
                if (Path.GetFullPath(folder) != folder)
                {
                    return "The folder name is not valid.";
                }
            }
            catch
            {
                return "The folder name is not valid.";
            }

            if (Directory.Exists(folder))
            {
                if (HasWriteAccess(folder))
                {
                    return null;
                }
                else
                {
                    return "You don't have write access to the given folder.";
                }
            }
            else
            {
                string parentFolder = Directory.GetParent(folder).FullName;

                for (int i = 0; i < parentFolder.Split('\\').Length - 1; ++i)
                {
                    if (Directory.Exists(parentFolder))
                    {
                        if (HasWriteAccess(parentFolder))
                        {
                            return null;
                        }
                        else
                        {
                            return "You don't have write access to this folder: " + parentFolder;
                        }
                    }
                    else
                    {
                        try
                        {
                            parentFolder = Directory.GetParent(parentFolder).FullName;
                        }
                        catch
                        {
                            return "The given folder can't be reached or created.";
                        }
                    }
                }

                return "The given folder can't be reached or created.";
            }
        }

        private static bool HasWriteAccess(string folder)
        {
            if (UserHasWriteAccess(folder))
            {
                return true;
            }
            else if (UserInGroupWithWriteAccess(folder))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static bool UserHasWriteAccess(string folder)
        {
            string[] temp = Convert.ToString(WindowsIdentity.GetCurrent().Name).Split('\\');
            string userName = temp[0] + "\\" + temp[1];

            try
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(folder);
                DirectorySecurity accessControl = directoryInfo.GetAccessControl(AccessControlSections.Access);
                AuthorizationRuleCollection accessRules = accessControl.GetAccessRules(true, true, typeof(NTAccount));

                foreach (AuthorizationRule accessRule in accessRules)
                {
                    if (accessRule.IdentityReference.Value.Equals(userName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        if ((((FileSystemAccessRule)accessRule).FileSystemRights & FileSystemRights.WriteData) > 0)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }
            catch
            {
                return false;
            }

            return false;
        }

        private static bool UserInGroupWithWriteAccess(string folder)
        {
            if (string.IsNullOrEmpty(folder))
            {
                return false;
            }

            try
            {
                AuthorizationRuleCollection accessRules = Directory.GetAccessControl(folder).GetAccessRules(true, true, typeof(SecurityIdentifier));

                using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
                {
                    foreach (FileSystemAccessRule accessRule in accessRules)
                    {
                        if (identity.Groups.Contains(accessRule.IdentityReference))
                        {
                            if ((FileSystemRights.WriteData & accessRule.FileSystemRights) == FileSystemRights.WriteData)
                            {
                                if (accessRule.AccessControlType == AccessControlType.Allow)
                                {
                                    return true;
                                }
                                else
                                {
                                    return false;
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                return false;
            }

            return false;
        }

        public static bool ValidateFileName(string fileName)
        {
            if (fileName == null)
            {
                return false;
            }

            return (fileName.IndexOfAny(Path.GetInvalidFileNameChars()) < 0 && !string.IsNullOrEmpty(fileName));
        }

        public static bool PdfFileExists(string fileName)
        {
            if (fileName.Length > 3)
            {
                if (Path.GetExtension(fileName) != ".pdf")
                {
                    fileName += ".pdf";
                }
            }
            else
            {
                return false;
            }

            return File.Exists(fileName);
        }

        public static string ShowFileDialog()
        {
            using (OpenFileDialog fd = new OpenFileDialog())
            {
                fd.Title = "Select file";
                fd.Filter = "PDF files|*.pdf";
                fd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                fd.Multiselect = false;
                DialogResult result = fd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fd.FileName))
                {
                    return fd.FileName;
                }
                else
                {
                    return null;
                }
            }
        }

        public static string ShowFolderDialog()
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                fbd.Description = "Select folder";
                fbd.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    return fbd.SelectedPath;
                }
                else
                {
                    return null;
                }
            }
        }
    }
}