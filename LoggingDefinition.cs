using System;
using LatestFileReporter.Interfaces;

namespace LatestFileReporter
{
	public class LoggingDefinition : IProgramDefinition
	{

		public bool CopySourceFile(string fileName)
		{
			throw new NotImplementedException();
		}

		public bool RunBatchFile(string fileName)
		{
			throw new NotImplementedException();
		}

		public bool DoesLogFileIndicateCommonError(string fileName)
		{
			throw new NotImplementedException();
		}
	}

}