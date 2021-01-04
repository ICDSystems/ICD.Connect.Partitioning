using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Csv;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.IO;
using ICD.Connect.Conferencing.ConferenceManagers.History;
using ICD.Connect.Partitioning.Commercial.Rooms;
using ICD.Connect.Telemetry.Attributes;
using ICD.Connect.Telemetry.Providers;

namespace ICD.Connect.Partitioning.Commercial.CallRatings
{
	[ExternalTelemetry("CallRatingManager Telemetry", typeof(CallRatingManagerExternalTelemetryProvider))]
    public sealed class CallRatingManager : ITelemetryProvider
    {
	    #region Events

	    public event EventHandler<GenericEventArgs<eCallRating>> OnCallRating;

	    #endregion

	    #region Private members

	    private const string CSV_FILE_LOCAL_PATH = "Log-CallRating.csv";

	    private readonly ICommercialRoom m_Room;

	    #endregion

	    #region Properties

	    private static string CallRatingLogPath { get { return PathUtils.GetProgramDataPath(CSV_FILE_LOCAL_PATH); } }

		/// <summary>
	    /// Gets the room.
	    /// </summary>
	    public ICommercialRoom Room { get { return m_Room; } }

		#endregion

		#region Constructor

	    /// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
	    public CallRatingManager(ICommercialRoom parent)
	    {
		    m_Room = parent;
	    }

	    #endregion

	    #region Methods

	    public void InitializeTelemetry()
	    {
	    }

	    public void RateCall(eCallRating rating, [NotNull] IEnumerable<IHistoricalConference> conferences)
	    {
		    IEnumerable<CallRatingParticipant> participants =
			    conferences.SelectMany(c => c.GetParticipants()).Select(p => new CallRatingParticipant
			    {
				    CallType = p.CallType,
				    StartTime = p.StartTime,
				    EndTime = p.EndTime,
				    Name = p.Name,
				    Number = p.Number,
				    AnswerState = p.AnswerState,
				    Direction = p.Direction,
			    });

			CallRating callRating = new CallRating
			{
				Date = IcdEnvironment.GetUtcTime(),
				Rating = (int)rating,
				RoomName = m_Room.Name,
				RoomId = m_Room.Id,
				Participants = participants
			};

			WriteCsvRating(callRating);
			CallRating.Insert(m_Room.Id, callRating);

			OnCallRating.Raise(this, rating);
		}

	    #endregion

	    #region Private Methods

	    private static void WriteCsvRating(CallRating callRating)
	    {
		    bool isNew = !IcdFile.Exists(CallRatingLogPath);

		    using (IcdStreamWriter file = IcdFile.AppendText(CallRatingLogPath))
		    {
			    CsvWriterSettings settings = new CsvWriterSettings
			    {
				    AlwaysEscapeEveryValue = false,
				    InsertSpaceAfterComma = false
			    };

			    string[] header =
			    {
					"Room Name",
					"Room ID",
					"Rating",
					"Start Time",
					"End Time",
					"Part. Call Type",
					"Part. Start Time",
					"Part. End Time",
					"Part. Name",
					"Part. Number",
					"Part. Call Direction",
					"Part. Answer State"
			    };

			    using (CsvWriter writer = new CsvWriter(file, settings))
			    {
					if (isNew)
						writer.AppendRow(header);

				    DateTime? startTime =
					    callRating.Participants
					              .Select(p => p.StartTime)
					              .Order()
					              .FirstOrDefault();

					DateTime? endTime =
						callRating.Participants
						          .Select(p => p.StartTime)
						          .Order()
						          .LastOrDefault();

					bool first = true;

					writer.AppendValue(callRating.RoomName);
				    writer.AppendValue(callRating.RoomId);
				    writer.AppendValue((int)callRating.Rating);
				    writer.AppendValue(GetCallRatingDateTimeString(startTime));
				    writer.AppendValue(GetCallRatingDateTimeString(endTime));

				    foreach (CallRatingParticipant participant in callRating.Participants)
				    {
						if (!first)
							writer.AppendRow(new string[5]);

					    writer.AppendValue(participant.CallType);
					    writer.AppendValue(GetCallRatingDateTimeString(participant.StartTime));
					    writer.AppendValue(GetCallRatingDateTimeString(participant.EndTime));
					    writer.AppendValue(participant.Name);
					    writer.AppendValue(participant.Number);
					    writer.AppendValue(participant.Direction);
					    writer.AppendValue(participant.AnswerState);

					    first = false;
				    }

				    writer.AppendNewline();
			    }
		    }
	    }

	    private static string GetCallRatingDateTimeString(DateTime? dateTime)
	    {
		    return dateTime.HasValue ? dateTime.Value.ToUniversalTime().ToIso() : null;
	    }

	    #endregion
    }
}
