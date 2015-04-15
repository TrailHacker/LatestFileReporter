using LatestFileReporter.Interfaces;
using log4net;

namespace LatestFileReporter
{
	public class LoggingDefinition : IProgramDefinition
	{
		private readonly ILog _logger;

		public LoggingDefinition()
		{
			_logger = LogManager.GetLogger(typeof(IProgramSettings));
		}

		public bool CopySourceFile(string fileName)
		{
			_logger.Info("CopySourceFile called: " + fileName);
			return false;
		}

		public bool RunBatchFile(string fileName)
		{
			_logger.Info("RunBatchFile called: " + fileName);
			return false;
		}

		public bool DoesLogFileIndicateCommonError(string fileName)
		{
			_logger.Info("DoesLogFileIndicateCommonError called: " + fileName);
			return true;
		}
	}

}