using System;
using System.Collections.Generic;
using ICD.Connect.Telemetry.Attributes;
using ICD.Connect.Telemetry.Providers.External;

namespace ICD.Connect.Partitioning.Rooms
{
	public interface IRoomExternalTelemetryProvider : IExternalTelemetryProvider
	{
		[EventTelemetry("OnOriginatorIdsChanged")]
		event EventHandler OnOriginatorIdsChanged;

		[PropertyTelemetry("OriginatorIds", null, "OnOriginatorIdsChanged")]
		IEnumerable<Guid> OriginatorIds { get; }
	}
}
