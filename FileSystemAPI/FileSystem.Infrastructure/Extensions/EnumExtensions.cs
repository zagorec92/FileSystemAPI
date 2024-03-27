using FileSystem.Infrastructure.Enums;
using Microsoft.Extensions.Logging;

namespace FileSystem.Infrastructure.Extensions
{
	internal static class EnumExtensions
	{
		internal static EventId ToEventId(this LogEvent logEvent) => new((int)logEvent);
	}
}
