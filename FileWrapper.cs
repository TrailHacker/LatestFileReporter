﻿using System;
using System.IO;
using LatestFileReporter.Interfaces;

namespace LatestFileReporter
{
	public class FileWrapper : IFileInfo
	{
		private readonly FileSystemInfo _file;

		#region Ctor

		public FileWrapper(string path) : this(new FileInfo(path)) {}

		public FileWrapper(FileSystemInfo file)
		{
			if (file == null)
				throw new ArgumentNullException("file");
			_file = file;
		}

		#endregion

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