using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LatestFileReporter.Interfaces;

namespace LatestFileReporter
{
	public class FileExtractor : IFileExtractor
	{
		private readonly IProgramSettings _settings;

		public FileExtractor(IProgramSettings settings)
		{
			if (settings == null)
				throw new ArgumentNullException("settings");

			_settings = settings;

		}

		public IEnumerable<IFileInfo> GetFilesToProcess()
		{
			var ignoreList = _settings.IgnoreFileList;
			var directory = new DirectoryInfo(_settings.SourceFileDirectoryPath);
			return from fileInfo in directory.GetFiles("*" + _settings.SearchFileExtension)
				where !ignoreList.Contains(fileInfo.Name, StringComparer.InvariantCultureIgnoreCase)
				select (IFileInfo) new FileWrapper(fileInfo);
		}

	}
}