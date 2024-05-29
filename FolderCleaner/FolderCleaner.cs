using System;
using System.Collections.Generic;
using System.ServiceProcess;
using System.Runtime.InteropServices;
using System.IO;
using System.IO.Compression;
using FolderCleaner.Properties;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using Aspose.Imaging.FileFormats.OpenDocument.Objects.Graphic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;

namespace FolderCleaner
{
	public partial class FolderCleaner : ServiceBase
	{
		private bool isRunning;
		private string result;

		CancellationTokenSource cancellationTokenSource;
		CancellationToken cancellationToken;
		TimeSpan interval;
		TimeSpan intervalTimeController;
		AutoResetEvent resetInterval;
		public FolderCleaner()
		{
			InitializeComponent();
			eventLog1 = new System.Diagnostics.EventLog();
			if (!System.Diagnostics.EventLog.SourceExists("Folder Cleaner"))
			{
				System.Diagnostics.EventLog.CreateEventSource(
					"Folder_Cleaner", "FolderCleanerLOG");
			}
			eventLog1.Source = "Folder_Cleaner";
			eventLog1.Log = "FolderCleanerLOG";
		}
		public void startService(string[] args)
		{
			OnStart(args);
		}
		protected override void OnStart(string[] args)
		{
			isRunning = true;
			interval = new TimeSpan();
			intervalTimeController = new TimeSpan(0, 0, 10);
			resetInterval = new AutoResetEvent(false);
			cancellationTokenSource = new CancellationTokenSource();
			cancellationToken = cancellationTokenSource.Token;
			eventLog1.WriteEntry("Successfully Started");
			Task task = Task.Run(() => ScanFolder(cancellationToken), cancellationToken);
		}
		protected override void OnStop()
		{
			isRunning = false;
			cancellationTokenSource.Cancel();
			eventLog1.WriteEntry("In OnStop.");
		}
		protected override void OnContinue()
		{
			eventLog1.WriteEntry("In OnContinue.");
		}

		public void ScanFolder(CancellationToken cancellationToken)
		{
			if (cancellationToken.IsCancellationRequested)
			{
				Console.WriteLine("Task was canceled before it started.");
				return;
			}
			while (isRunning)
			{
				if (cancellationToken.IsCancellationRequested)
				{
					Console.WriteLine("Task was canceled during execution.");
					return;
				}
				if (interval < TimeSpan.Zero)
				{
					interval = TimeSpan.FromSeconds(1);
				}
				if (resetInterval.WaitOne(interval))
					continue;
				Stopwatch stopwatch = Stopwatch.StartNew();
				eventLog1.WriteEntry("Scan started");
				string Pfad = Settings.Default.ScanPath;
				string OutputPath = Settings.Default.OutputPath;
				string PicturePath = Settings.Default.PathForPictures;
				string PdfPath = Settings.Default.OutputPath + "/pdf";
				string WordPath = Settings.Default.OutputPath + "/word";
				string ExelPath = Settings.Default.OutputPath + "/excel";	
				string DDDPath = Settings.Default.OutputPath + "/ddd";
				string exePath = Settings.Default.OutputPath + "/exe";

				var currentList = GetFileList(Pfad);
				if (!currentList.Contains(OutputPath) && Settings.Default.ScanPath != Settings.Default.OutputPath)
				{
					System.IO.Directory.CreateDirectory(OutputPath);
				}

				foreach (var item in currentList)
				{
					if (File.Exists(item))
					{
						//JFIF
						string extension = Path.GetExtension(item);
						if (extension.Equals(".png", StringComparison.OrdinalIgnoreCase) || extension.Equals(".jfif", StringComparison.OrdinalIgnoreCase) || extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase) || extension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase) || extension.Equals(".gif", StringComparison.OrdinalIgnoreCase) || extension.Equals(".bmp", StringComparison.OrdinalIgnoreCase) || extension.Equals(".webp", StringComparison.OrdinalIgnoreCase))
						{
							string path = item;
							if (extension.Equals(".webp", StringComparison.OrdinalIgnoreCase)) path = OutputPath + "/" + ConvertWEBPToPNG(item);
							if (extension.Equals(".tiff", StringComparison.OrdinalIgnoreCase)) path = OutputPath + "/" + ConvertTIFFToPNG(item);
							if (extension.Equals(".bmp", StringComparison.OrdinalIgnoreCase)) path = OutputPath + "/" + ConvertBMPToPNG(item);
							if (extension.Equals(".jfif", StringComparison.OrdinalIgnoreCase)) path = OutputPath + "/" + ConvertJfifToPng(item);
							CheckAndReplaceFile(PicturePath, path);
						}
						else if (extension.Equals(".zip", StringComparison.OrdinalIgnoreCase) && Settings.Default.DeleteZip)
						{
							var path = Path.GetDirectoryName(item) + "\\" + CheckFile(Path.GetDirectoryName(item), Path.GetFileName(item));
							path = path.Substring(0, path.Length - 4);
							ZipFile.ExtractToDirectory(item, path);
							File.Delete(item);
						}
						else if (extension.Equals(".pdf", StringComparison.OrdinalIgnoreCase))
						{
							if (!currentList.Contains(PdfPath)) System.IO.Directory.CreateDirectory(PdfPath);
							CheckAndReplaceFile(PdfPath, item);

						}
						else if (extension.Equals(".docx", StringComparison.OrdinalIgnoreCase))
						{
							if (!currentList.Contains(WordPath)) System.IO.Directory.CreateDirectory(WordPath);
							CheckAndReplaceFile(WordPath, item);
						}
						else if (extension.Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
						{
							if (!currentList.Contains(ExelPath)) System.IO.Directory.CreateDirectory(ExelPath);
							CheckAndReplaceFile(ExelPath, item);
						}
						else if (extension.Equals(".DDD", StringComparison.OrdinalIgnoreCase))
						{
							if (!currentList.Contains(DDDPath)) System.IO.Directory.CreateDirectory(DDDPath);
							CheckAndReplaceFile(DDDPath, item);
						}
						else if (extension.Equals(".exe", StringComparison.OrdinalIgnoreCase))
						{
							if (!currentList.Contains(exePath)) System.IO.Directory.CreateDirectory(exePath);
							CheckAndReplaceFile(exePath, item);
						}
						else
						{
							CheckAndReplaceFile(OutputPath, item);
						}
					}
				}
				CheckScannedList();
				eventLog1.WriteEntry("Fertig mit Scan");
				stopwatch.Stop();
				interval = intervalTimeController.Subtract(TimeSpan.FromMilliseconds(stopwatch.ElapsedMilliseconds));
			}
		}
		public void CheckAndReplaceFile(string path, string item, int x = 2)
		{
			File.Move(item, Path.Combine(path, CheckFile(path, Path.GetFileName(item))));
		}
		public string CheckFile(string path, string item, int x = 2)
		{
			if(path == Settings.Default.OutputPath || path == "C:\\Users\\akoesters\\Downloads")
			{
				int count = CountFileNameOccurrences(path, item);
				if (count <= 1)
				{
					return Path.GetFileName(item);
				}
			}
			if (!File.Exists(Path.Combine(path, Path.GetFileName(item))) && !Directory.Exists(Path.Combine(path, Path.GetFileName(item))))
			{
				result = Path.GetFileName(item);
			}
			else if (!File.Exists(Path.Combine(path, Path.GetFileName(item).Substring(0, Path.GetFileName(item).Length-Path.GetExtension(item).Length) + "(" + x+")"+Path.GetExtension(item))) && !Directory.Exists(Path.Combine(path, Path.GetFileName(item).Substring(0, Path.GetFileName(item).Length - Path.GetExtension(item).Length) + "(" + x + ")" + Path.GetExtension(item))))
			{
				result = Path.GetFileName(item).Substring(0, Path.GetFileName(item).Length - Path.GetExtension(item).Length) + "(" + x + ")" + Path.GetExtension(item);
			}
			else
			{
				x++;
				CheckFile(path, item, x);
			}
			return result;
		}
		public void CheckScannedList()
		{
			var scannedList = GetFileList(Settings.Default.OutputPath);
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
		public string ConvertWEBPToPNG(string pathToWEBP)
		{
			var folder = Settings.Default.ScanPath;
			using (var image = Aspose.Imaging.Image.Load(Path.Combine(folder, Path.GetFileName(pathToWEBP))))
			{
				var exportOptions = new Aspose.Imaging.ImageOptions.PngOptions() { ColorType = Aspose.Imaging.FileFormats.Png.PngColorType.TruecolorWithAlpha };
				image.Save(Path.Combine(folder, Path.ChangeExtension(Path.GetFileName(pathToWEBP), ".png")), exportOptions);
			}
			File.Delete(pathToWEBP);
			return Path.ChangeExtension(Path.GetFileName(pathToWEBP), ".png");
		}
		public string ConvertTIFFToPNG(string pathToTIFF)
		{
			Bitmap bm = new Bitmap(Path.GetFileName(pathToTIFF));
			bm.Save(Path.ChangeExtension(Path.GetFileName(pathToTIFF), ".png"), ImageFormat.Png);
			File.Delete(pathToTIFF);
			return Path.ChangeExtension(Path.GetFileName(pathToTIFF), ".png");
		}

		public string ConvertBMPToPNG(string pathToBMP)
		{
            string newFilePath = Path.ChangeExtension(pathToBMP, ".png");

            using (Bitmap bm = new Bitmap(pathToBMP))
            {
                bm.Save(newFilePath, System.Drawing.Imaging.ImageFormat.Png);
            }
            File.Delete(pathToBMP);
            return Path.GetFileName(newFilePath);
        }
		public string ConvertJfifToPng(string jfifFilePath)
		{
			using (Image jfifImage = Image.FromFile(jfifFilePath))
			{
				using (Bitmap pngImage = new Bitmap(jfifImage.Width, jfifImage.Height))
				{
					using (Graphics g = Graphics.FromImage(pngImage))
					{
						g.DrawImage(jfifImage, 0, 0);
					}
					pngImage.Save(Path.Combine(Settings.Default.ScanPath, Path.ChangeExtension(Path.GetFileName(jfifFilePath), ".png")), ImageFormat.Png);
				}
			}
			File.Delete(jfifFilePath);
			return Path.ChangeExtension(Path.GetFileName(jfifFilePath), ".png");
		}
		public List<string> GetFileList(string Root)
		{
			List<string> FileArray = new List<string>();

			string[] Files = System.IO.Directory.GetFiles(Root);
			string[] Folders = System.IO.Directory.GetDirectories(Root);
			FileArray.AddRange(Files);
			FileArray.AddRange(Folders);
			return FileArray;
		}
		public int CountFileNameOccurrences(string directoryPath, string fileName)
		{
			DirectoryInfo directory = new DirectoryInfo(directoryPath);

			int count = directory.GetFiles().Count(file => file.Name.Equals(fileName, StringComparison.OrdinalIgnoreCase));

			return count;
		}
	}
}

