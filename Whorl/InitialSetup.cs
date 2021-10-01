using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Whorl
{
    public class InitialSetup
    {
        public const string SettingFilesFolder = "SettingsFiles";
        public static string StandardTextsFileName { get; } = Path.Combine(SettingFilesFolder, "StandardFormulaTexts.xml");

        public static void InitializeSettings(out bool loadErrors)
        {
            loadErrors = false;
            try
            {
                SettingsXML.PopulateSettingsFromXml(WhorlSettings.Instance, out List<string> errors);
                if (errors.Any())
                {
                    loadErrors = true;
                    MessageBox.Show(string.Join(Environment.NewLine, errors), "Errors reading settings.");
                }
                var standardFormulaTexts = new StandardFormulaTextList();
                string filePath = Path.Combine(WhorlSettings.Instance.FilesFolder, StandardTextsFileName);
                if (File.Exists(filePath))
                {
                    Tools.ReadFromXml(filePath, standardFormulaTexts);
                }
                frmFormulaInsert.Instance = new frmFormulaInsert(standardFormulaTexts);
            }
            catch (Exception ex)
            {
                loadErrors = true;
                Tools.HandleException(ex);
            }
        }

        public static bool InitialSetupNeeded()
        {
            if (WhorlSettings.Instance.FilesFolder == null || !Directory.Exists(WhorlSettings.Instance.FilesFolder))
                return true;
            string fileName = Path.Combine(WhorlSettings.Instance.FilesFolder, WhorlSettings.Instance.PatternChoicesFileName);
            return !File.Exists(fileName);
        }

        public static bool PerformInitialSetup()
        {
            var cursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                if (WhorlSettings.Instance.FilesFolder == null)
                    throw new NullReferenceException("WhorlSettings.Instance.FilesFolder cannot be null.");
                if (!Directory.Exists(WhorlSettings.Instance.FilesFolder))
                {
                    Directory.CreateDirectory(WhorlSettings.Instance.FilesFolder);
                }
                string choicesXmlFileName = Path.Combine(WhorlSettings.Instance.FilesFolder, WhorlSettings.Instance.PatternChoicesFileName);
                if (!File.Exists(choicesXmlFileName))
                {
                    string zipFileName = Path.Combine(Application.StartupPath, "WhorlFiles", "WhorlFiles.zip");
                    if (!File.Exists(zipFileName))
                    {
                        MessageBox.Show($"The required installation file {zipFileName} was not found.");
                        return false;
                    }
                    //ZipFile.ExtractToDirectory(zipFileName, WhorlSettings.Instance.FilesFolder);
                    using (ZipArchive archive = ZipFile.OpenRead(zipFileName))
                    {
                        foreach (var entry in archive.Entries)
                        {
                            if (!entry.FullName.EndsWith("/"))  //Not a folder.
                            {
                                int pos = entry.FullName.IndexOf('/');
                                string entryName = pos >= 0 ? entry.FullName.Substring(pos + 1) : entry.FullName;
                                string fileName = Path.Combine(WhorlSettings.Instance.FilesFolder, entryName);
                                string folder = Path.GetDirectoryName(fileName);
                                if (!Directory.Exists(folder))
                                    Directory.CreateDirectory(folder);
                                if (!File.Exists(fileName))
                                {
                                    try
                                    {
                                        entry.ExtractToFile(fileName);
                                    }
                                    catch (Exception ex)
                                    {
                                        throw new Exception($"Error extracting file {fileName}", ex);
                                    }
                                }
                            }
                        }
                    }
                }
                CreateThumbnailsFolder(WhorlSettings.Instance.FilesFolder);
                foreach (string folder in Directory.EnumerateDirectories(Path.Combine(WhorlSettings.Instance.FilesFolder,
                                                                                      WhorlSettings.Instance.CustomDesignParentFolder)))
                {
                    CreateThumbnailsFolder(folder);
                }
                return true;
            }
            finally
            {
                Cursor.Current = cursor;
            }
        }

        private static void CreateThumbnailsFolder(string baseFolder)
        {
            string thumbnailFolder = Path.Combine(baseFolder, WhorlSettings.Instance.DesignThumbnailsFolder);
            if (!Directory.Exists(thumbnailFolder))
                Directory.CreateDirectory(thumbnailFolder);
        }
    }
}
