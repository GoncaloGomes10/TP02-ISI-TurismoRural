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

		/// <summary>
		/// Serviço responsável pela integração com o Google Calendar.
		/// Utiliza uma Service Account para criar, atualizar e eliminar eventos
		/// num calendário específico configurado no ficheiro de configuração.
		/// </summary>
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

		/// <summary>
		/// Cria um novo evento no Google Calendar.
		/// </summary>
		/// <param name="summary">Título do evento.</param>
		/// <param name="startUtc">Data e hora de início do evento (UTC).</param>
		/// <param name="endUtc">Data e hora de fim do evento (UTC).</param>
		/// <param name="description">Descrição detalhada do evento.</param>
		/// <returns>
		/// Retorna o identificador (ID) do evento criado no Google Calendar.
		/// </returns>
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

		/// <summary>
		/// Atualiza um evento existente no Google Calendar.
		/// </summary>
		/// <param name="eventId">Identificador do evento a atualizar.</param>
		/// <param name="summary">Novo título do evento.</param>
		/// <param name="description">Nova descrição do evento.</param>
		/// <param name="startUtc">Nova data e hora de início do evento (UTC).</param>
		/// <param name="endUtc">Nova data e hora de fim do evento (UTC).</param>
		/// <returns>
		/// Tarefa assíncrona sem valor de retorno.
		/// </returns>
		public async Task UpdateEventAsync(string eventId, string summary, string description, DateTime startUtc, DateTime endUtc)
		{
			var ev = await _calendar.Events.Get(_calendarId, eventId).ExecuteAsync();
			ev.Summary = summary;
			ev.Description = description;
			ev.Start = new EventDateTime { DateTime = startUtc, TimeZone = "UTC" };
			ev.End = new EventDateTime { DateTime = endUtc, TimeZone = "UTC" };

			await _calendar.Events.Update(ev, _calendarId, eventId).ExecuteAsync();
		}

		/// <summary>
		/// Elimina um evento existente do Google Calendar.
		/// </summary>
		/// <param name="eventId">Identificador do evento a eliminar.</param>
		/// <returns>
		/// Tarefa assíncrona sem valor de retorno.
		/// </returns>
		public async Task DeleteEventAsync(string eventId)
		{
			await _calendar.Events.Delete(_calendarId, eventId).ExecuteAsync();
		}
	}
}
