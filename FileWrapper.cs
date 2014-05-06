using System;
using System.IO;

namespace LatestFileReporter
{
	public class FileWrapper : IFileInfo
	{
		private readonly FileSystemInfo _file;

		public FileWrapper(string path) : this(new FileInfo(path)) { }

		public FileWrapper(FileSystemInfo file)
		{
			if (file == null)
				throw new ArgumentNullException("file");
			_file = file;
		}

		public string Name
		{
			get { return _file.Name; }
		}

		public DateTime LastWriteTime
		{
			get { return _file.LastWriteTime; }
		}

	}
}