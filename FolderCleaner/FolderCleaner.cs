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

namespace FolderCleaner
{
	public partial class FolderCleaner : ServiceBase
	{
		private bool isRunning;

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
			intervalTimeController = new TimeSpan(0, 1, 0);
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
				string PdfPath = Settings.Default.PdfPath;
				string WordPath = Settings.Default.WordPath;
				string ExelPath = Settings.Default.ExcelPath;
				string DDDPath = Settings.Default.DDDPath;

				var currentList = GetFileList(Pfad);
				if (!currentList.Contains(OutputPath))
				{
					System.IO.Directory.CreateDirectory(OutputPath);
				}

				foreach (var item in currentList)
				{
					if (File.Exists(item))
					{
						string extension = Path.GetExtension(item);
						if (extension.Equals(".png", StringComparison.OrdinalIgnoreCase) || extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase) || extension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase) || extension.Equals(".gif", StringComparison.OrdinalIgnoreCase) || extension.Equals(".bmp", StringComparison.OrdinalIgnoreCase) || extension.Equals(".webp", StringComparison.OrdinalIgnoreCase))
						{
							if (extension.Equals(".webp", StringComparison.OrdinalIgnoreCase)) ConvertWEBPToPNG(item);
							if (extension.Equals(".tiff", StringComparison.OrdinalIgnoreCase)) ConvertTIFFToPNG(item);
							if (extension.Equals(".bmp", StringComparison.OrdinalIgnoreCase)) ConvertBMPToPNG(item);
							File.Move(item, Path.Combine(PicturePath, Path.GetFileName(item)));
						}
						else if (extension.Equals(".zip", StringComparison.OrdinalIgnoreCase) && Settings.Default.DeleteZip)
						{
							ZipFile.ExtractToDirectory(item, item.Substring(0, item.Length - 4));
							File.Delete(item);
						}
						else if (extension.Equals(".pdf", StringComparison.OrdinalIgnoreCase))
						{
							if (!currentList.Contains(PdfPath)) System.IO.Directory.CreateDirectory(PdfPath);
							File.Move(item, Path.Combine(PdfPath, Path.GetFileName(item)));
						}
						else if (extension.Equals(".docx", StringComparison.OrdinalIgnoreCase))
						{
							if (!currentList.Contains(WordPath)) System.IO.Directory.CreateDirectory(WordPath);
							File.Move(item, Path.Combine(WordPath, Path.GetFileName(item)));
						}
						else if (extension.Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
						{
							if (!currentList.Contains(ExelPath)) System.IO.Directory.CreateDirectory(ExelPath);
							File.Move(item, Path.Combine(ExelPath, Path.GetFileName(item)));
						}
						else if (extension.Equals(".DDD", StringComparison.OrdinalIgnoreCase))
						{
							if (!currentList.Contains(DDDPath)) System.IO.Directory.CreateDirectory(DDDPath);
							File.Move(item, Path.Combine(DDDPath, Path.GetFileName(item)));
						}
						else
						{
							File.Move(item, Path.Combine(OutputPath, Path.GetFileName(item)));
						}
					}
				}
				CheckScannedList();
				eventLog1.WriteEntry("Fertig mit Scan");
				stopwatch.Stop();
				interval = intervalTimeController.Subtract(TimeSpan.FromMilliseconds(stopwatch.ElapsedMilliseconds));
			}
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
		public void ConvertWEBPToPNG(string pathToWEBP)
		{
			var folder = Settings.Default.ScanPath;
			using (var image = Aspose.Imaging.Image.Load(Path.Combine(folder, Path.GetFileName(pathToWEBP))))
			{
				var exportOptions = new Aspose.Imaging.ImageOptions.PngOptions() { ColorType = Aspose.Imaging.FileFormats.Png.PngColorType.TruecolorWithAlpha };
				image.Save(Path.Combine(folder, Path.ChangeExtension(Path.GetFileName(pathToWEBP), ".png")), exportOptions);
			}
			File.Delete(pathToWEBP);
		}
		public void ConvertTIFFToPNG(string pathToTIFF)
		{
			Bitmap bm = new Bitmap(Path.GetFileName(pathToTIFF));
			bm.Save(Path.ChangeExtension(Path.GetFileName(pathToTIFF), ".png"), ImageFormat.Png);
			File.Delete(pathToTIFF);
		}
		public void ConvertBMPToPNG(string pathToBMP)
		{
			Bitmap bm = new Bitmap(Path.GetFileName(pathToBMP));
			bm.Save(Path.ChangeExtension(Path.GetFileName(pathToBMP), ".png"), ImageFormat.Png);
			File.Delete(pathToBMP);
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
	}
}

