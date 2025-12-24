using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Microsoft.Extensions.Configuration;


namespace TurismoRural.Services
{
	public class GoogleCalendarService
	{
		private readonly CalendarService _calendar;
		private readonly string _calendarId;

		public GoogleCalendarService(IConfiguration config)
		{
			_calendarId = config["GoogleCalendar:CalendarId"]
						  ?? throw new Exception("GoogleCalendar:CalendarId em falta.");

			var jsonPath = config["GoogleCalendar:ServiceAccountJsonPath"]
						   ?? throw new Exception("GoogleCalendar:ServiceAccountJsonPath em falta.");

			var credential = GoogleCredential
				.FromFile(jsonPath)
				.CreateScoped(CalendarService.Scope.Calendar);

			_calendar = new CalendarService(new BaseClientService.Initializer
			{
				HttpClientInitializer = credential,
				ApplicationName = "TurismoRural ISI"
			});
		}

		public async Task<string> CreateEventAsync(string summary, DateTime startUtc, DateTime endUtc, string description)
		{
			var ev = new Event
			{
				Summary = summary,
				Description = description,
				Start = new EventDateTime { DateTime = startUtc, TimeZone = "UTC" },
				End = new EventDateTime { DateTime = endUtc, TimeZone = "UTC" }
			};

			var created = await _calendar.Events.Insert(ev, _calendarId).ExecuteAsync();
			return created.Id;
		}

		public async Task UpdateEventAsync(string eventId, string summary, string description, DateTime startUtc, DateTime endUtc)
		{
			var ev = await _calendar.Events.Get(_calendarId, eventId).ExecuteAsync();
			ev.Summary = summary;
			ev.Description = description;
			ev.Start = new EventDateTime { DateTime = startUtc, TimeZone = "UTC" };
			ev.End = new EventDateTime { DateTime = endUtc, TimeZone = "UTC" };

			await _calendar.Events.Update(ev, _calendarId, eventId).ExecuteAsync();
		}

		public async Task DeleteEventAsync(string eventId)
		{
			await _calendar.Events.Delete(_calendarId, eventId).ExecuteAsync();
		}
	}
}
