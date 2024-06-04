using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FolderCleaner.Properties;
using System.Drawing.Imaging;
using System.Drawing;
using System.Diagnostics;

namespace FolderCleaner
{
    public class FileProcessor
    {
        private string _Downloadpath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Downloads";
        private string _PicturePath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
        private string _PdfPath;
        private string _WordPath;
        private string _ExelPath;
        private string _DDDPath;
        private string _exePath;
        private EventLog _eventlog;
        private List<string> currentList;

        public FileProcessor(EventLog eventLog)
        {
            _eventlog = eventLog;
            setUpPathAndFolder();
            this.currentList = GetFileList(_Downloadpath);
        }
        private void setUpPathAndFolder()
        {
            _PdfPath = Path.Combine(_Downloadpath, "pdf");
            _WordPath = Path.Combine(_Downloadpath, "word");
            _ExelPath = Path.Combine(_Downloadpath, "excel");
            _DDDPath = Path.Combine(_Downloadpath, "ddd");
            _exePath = Path.Combine(_Downloadpath, "exe");
            CreateDirectoryIfNotExists(_PicturePath);
            CreateDirectoryIfNotExists(_PdfPath);
            CreateDirectoryIfNotExists(_WordPath);
            CreateDirectoryIfNotExists(_ExelPath);
            CreateDirectoryIfNotExists(_DDDPath);
            CreateDirectoryIfNotExists(_exePath);
        }
        public void ProcessFiles()
        {
            _eventlog.WriteEntry("Scan started");
            foreach (var item in currentList)
            {
                if (File.Exists(item))
                {
                    string extension = Path.GetExtension(item).ToLowerInvariant();
                    if (extensionActions.ContainsKey(extension))
                    {
                        extensionActions[extension](item);
                    }
                    else
                    {
                        CheckAndReplaceFile(_Downloadpath, item);
                    }
                }
            }
            _eventlog.WriteEntry("Scan ended");
            CheckScannedList();
            _eventlog.WriteEntry("Rescan of Files ended");
        }

        private void ProcessImageFile(string path)
        {
            CheckAndReplaceFile(_PicturePath, path);
        }

        private void ProcessZipFile(string item)
        {
            var destinationFolder = Path.Combine(Path.GetDirectoryName(item), CheckFile(Path.GetDirectoryName(item), Path.GetFileName(item)));
            destinationFolder = destinationFolder.Substring(0, destinationFolder.Length - 4);
            ZipFile.ExtractToDirectory(item, destinationFolder);
            File.Delete(item);
        }
        private void CreateDirectoryIfNotExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
        private void ProcessFile(string path, string item)
        {
            CheckAndReplaceFile(path, item);
        }
        private Dictionary<string, Action<string>> extensionActions => new Dictionary<string, Action<string>>
    {
        { ".png", ProcessImageFile },
        { ".jfif", ConvertJfifToPng },
        { ".jpg", ProcessImageFile },
        { ".jpeg", ProcessImageFile },
        { ".gif", ProcessImageFile },
        { ".bmp", ConvertBMPToPNG },
        { ".webp", ConvertWEBPToPNG },
        { ".tiff", ConvertTIFFToPNG },
        { ".zip", ProcessZipFile },
        { ".pdf", item => ProcessFile(_PdfPath, item) },
        { ".docx", item => ProcessFile(_WordPath, item) },
        { ".xlsx", item => ProcessFile(_ExelPath, item) },
        { ".ddd", item => ProcessFile(_DDDPath, item) },
        { ".exe", item => ProcessFile(_exePath, item) }
    };

        private void CheckAndReplaceFile(string path, string item)
        {
            File.Move(item, Path.Combine(path, CheckFile(path, Path.GetFileName(item))));
        }
        private string CheckFile(string path, string item, int counter = 2)
        {
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(item);
            string extension = Path.GetExtension(item);
            string newFileName = Path.Combine(path, item);
            int fileoccurance = CountFileNameOccurrences(path, fileNameWithoutExtension);

            while (File.Exists(newFileName) && fileoccurance > 1 || Directory.Exists(newFileName) && fileoccurance > 1)
            {
                newFileName = Path.Combine(path, $"{fileNameWithoutExtension}({counter}){extension}");
                fileoccurance = CountFileNameOccurrences(path, newFileName);
                counter++;
            }
            return newFileName;
        }
        private void CheckScannedList()
        {
            var scannedList = GetFileList(_Downloadpath);
            if (scannedList.Count > 0)
            {
                foreach (var item in scannedList)
                {
                    if (File.GetAttributes(item).HasFlag(FileAttributes.Directory))
                    {
                        foreach (var file in Directory.EnumerateFiles(item))
                        {
                            FileInfo fileInfo = new FileInfo(file);
                            if ((DateTime.Now - fileInfo.CreationTime).TotalDays > Settings.Default.DaysUntilDeleting)
                            {
                                File.Delete(file);
                            }
                        }
                        if (Directory.GetFiles(item).Length == 0 && Directory.GetDirectories(item).Length == 0)
                        {
                            Directory.Delete(item);
                        }
                    }
                    else
                    {
                        FileInfo fileInfo = new FileInfo(item);
                        if ((DateTime.Now - fileInfo.CreationTime).TotalDays > Settings.Default.DaysUntilDeleting)
                        {
                            File.Delete(item);
                        }
                    }
                }
            }
        }
        private void ConvertWEBPToPNG(string pathToWEBP)
        {
            var folder = _Downloadpath;
            using (var image = Aspose.Imaging.Image.Load(Path.Combine(folder, Path.GetFileName(pathToWEBP))))
            {
                var exportOptions = new Aspose.Imaging.ImageOptions.PngOptions() { ColorType = Aspose.Imaging.FileFormats.Png.PngColorType.TruecolorWithAlpha };
                image.Save(Path.Combine(folder, Path.ChangeExtension(Path.GetFileName(pathToWEBP), ".png")), exportOptions);
            }
            File.Delete(pathToWEBP);
            string path = _Downloadpath + "/" + Path.ChangeExtension(Path.GetFileName(pathToWEBP), ".png");
            CheckAndReplaceFile(path, pathToWEBP);
        }
        private void ConvertTIFFToPNG(string pathToTIFF)
        {
            Bitmap bm = new Bitmap(Path.GetFileName(pathToTIFF));
            bm.Save(Path.ChangeExtension(Path.GetFileName(pathToTIFF), ".png"), ImageFormat.Png);
            File.Delete(pathToTIFF);
            string path = _Downloadpath + "/" + Path.ChangeExtension(Path.GetFileName(pathToTIFF), ".png");
            CheckAndReplaceFile(path, pathToTIFF);
        }

        private void ConvertBMPToPNG(string pathToBMP)
        {
            string newFilePath = Path.ChangeExtension(pathToBMP, ".png");

            using (Bitmap bm = new Bitmap(pathToBMP))
            {
                bm.Save(newFilePath, System.Drawing.Imaging.ImageFormat.Png);
            }
            File.Delete(pathToBMP);
            string path = _Downloadpath + "/" + Path.GetFileName(newFilePath);
            CheckAndReplaceFile(path, pathToBMP);
        }
        private void ConvertJfifToPng(string jfifFilePath)
        {
            using (Image jfifImage = Image.FromFile(jfifFilePath))
            {
                using (Bitmap pngImage = new Bitmap(jfifImage.Width, jfifImage.Height))
                {
                    using (Graphics g = Graphics.FromImage(pngImage))
                    {
                        g.DrawImage(jfifImage, 0, 0);
                    }
                    pngImage.Save(Path.Combine(_Downloadpath, Path.ChangeExtension(Path.GetFileName(jfifFilePath), ".png")), ImageFormat.Png);
                }
            }
            File.Delete(jfifFilePath);
            string path = _Downloadpath + "/" + Path.ChangeExtension(Path.GetFileName(jfifFilePath), ".png");
            CheckAndReplaceFile(path, jfifFilePath);
        }
        private List<string> GetFileList(string Root)
        {
            List<string> FileArray = new List<string>();

            string[] Files = System.IO.Directory.GetFiles(Root);
            string[] Folders = System.IO.Directory.GetDirectories(Root);
            FileArray.AddRange(Files);
            FileArray.AddRange(Folders);
            return FileArray;
        }
        private int CountFileNameOccurrences(string directoryPath, string fileName)
        {
            DirectoryInfo directory = new DirectoryInfo(directoryPath);

            int count = directory.GetFiles().Count(file => file.Name.Equals(fileName, StringComparison.OrdinalIgnoreCase));

            return count;
        }
    }
}
