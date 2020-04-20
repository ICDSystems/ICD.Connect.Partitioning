using System;
using System.Collections.Generic;
using ICD.Connect.Telemetry;
using ICD.Connect.Telemetry.Attributes;

namespace ICD.Connect.Partitioning.Rooms
{
	public interface IRoomExternalTelemetryProvider : IExternalTelemetryProvider
	{
		[EventTelemetry("OnOriginatorIdsChanged")]
		event EventHandler OnOriginatorIdsChanged;

		[DynamicPropertyTelemetry("OriginatorIds", null, "OnOriginatorIdsChanged")]
		IEnumerable<Guid> OriginatorIds { get; }
	}
}
