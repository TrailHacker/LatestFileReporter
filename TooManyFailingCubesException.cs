using System;
using LatestFileReporter.Interfaces;

namespace LatestFileReporter
{
	internal class TooManyFailingCubesException : Exception
	{
		private readonly IFileInfo[] _files;

		public TooManyFailingCubesException(IFileInfo[] files) : base("Too many files are outdated. Please check the server for a bigger problem.")
		{
			_files = files;
		}

		public IFileInfo[] FailingFiles
		{
			get { return _files; }
		}
	}
}