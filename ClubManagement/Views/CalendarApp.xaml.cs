using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ClubManagement.Views
{
    public partial class CalendarApp : Page
    {
        static string[] Scopes = { CalendarService.Scope.Calendar };
        static string ApplicationName = "Google Calendar Integration";
        CalendarService service;

        public CalendarApp()
        {
            InitializeComponent();
            InitializeGoogleCalendarService();
        }

        private async void InitializeGoogleCalendarService()
        {
            try
            {
                UserCredential credential;

                using (var stream = new FileStream("client_secret_921999378493-3mjcj8s7l020j6pfdlmlja5qi3h375ji.apps.googleusercontent.com.json", FileMode.Open, FileAccess.Read))
                {
                    string credPath = "token.json";
                    credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.Load(stream).Secrets,
                        Scopes,
                        "user",
                        CancellationToken.None,
                        new FileDataStore(credPath, true));
                }

                service = new CalendarService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = ApplicationName,
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }

        private async void EventCalendar_SelectedDatesChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (service == null)
                return;

            var selectedDate = EventCalendar.SelectedDate;
            if (selectedDate == null)
                return;

            DateTime startDate = selectedDate.Value.Date;
            DateTime endDate = startDate.AddDays(1);

            EventsResource.ListRequest request = service.Events.List("primary");
            request.TimeMin = startDate;
            request.TimeMax = endDate;
            request.ShowDeleted = false;
            request.SingleEvents = true;
            request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

            Events events = await request.ExecuteAsync();
            EventListBox.Items.Clear();

            if (events.Items != null && events.Items.Count > 0)
            {
                foreach (var eventItem in events.Items)
                {
                    EventListBox.Items.Add(new EventWrapper(eventItem));
                }
            }
            else
            {
                EventListBox.Items.Add("No events found.");
            }
        }

        private void AddEventButton_Click(object sender, RoutedEventArgs e)
        {
            AddOrEditEvent(null);
        }

        private void EditEventButton_Click(object sender, RoutedEventArgs e)
        {
            if (EventListBox.SelectedItem is EventWrapper selectedEventWrapper)
            {
                AddOrEditEvent(selectedEventWrapper.Event);
            }
            else
            {
                MessageBox.Show("Please select an event to edit.");
            }
        }

        private async void DeleteEventButton_Click(object sender, RoutedEventArgs e)
        {
            if (EventListBox.SelectedItem is EventWrapper selectedEventWrapper)
            {
                await service.Events.Delete("primary", selectedEventWrapper.Event.Id).ExecuteAsync();
                MessageBox.Show("Event deleted.");
                EventCalendar_SelectedDatesChanged(null, null); // Refresh the event list
            }
            else
            {
                MessageBox.Show("Please select an event to delete.");
            }
        }

        private async void AddOrEditEvent(Event existingEvent)
        {
            EventDialog dialog = new EventDialog();

            if (existingEvent != null)
            {
                dialog.SummaryTextBox.Text = existingEvent.Summary;
                dialog.StartDatePicker.SelectedDate = existingEvent.Start.DateTime ?? DateTime.Parse(existingEvent.Start.Date);
                dialog.StartHourTextBox.Text = (existingEvent.Start.DateTime ?? DateTime.Parse(existingEvent.Start.Date)).Hour.ToString();
                dialog.EndDatePicker.SelectedDate = existingEvent.End.DateTime ?? DateTime.Parse(existingEvent.End.Date);
                dialog.EndHourTextBox.Text = (existingEvent.End.DateTime ?? DateTime.Parse(existingEvent.End.Date)).Hour.ToString();
                dialog.DescriptionTextBox.Text = existingEvent.Description;
                dialog.LocationTextBox.Text = existingEvent.Location;
            }

            if (dialog.ShowDialog() == true)
            {
                Event newEvent = existingEvent ?? new Event();
                newEvent.Summary = dialog.EventSummary;
                newEvent.Start = new EventDateTime() { DateTime = dialog.StartTime };
                newEvent.End = new EventDateTime() { DateTime = dialog.EndTime };
                newEvent.Description = dialog.EventDescription;
                newEvent.Location = dialog.EventLocation;

                if (existingEvent == null)
                {
                    await service.Events.Insert(newEvent, "primary").ExecuteAsync();
                    MessageBox.Show("Event added.");
                }
                else
                {
                    await service.Events.Update(newEvent, "primary", existingEvent.Id).ExecuteAsync();
                    MessageBox.Show("Event updated.");
                }

                EventCalendar_SelectedDatesChanged(null, null); // Refresh the event list
            }
        }

        private void EventListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (EventListBox.SelectedItem is EventWrapper selectedEventWrapper)
            {
                var eventItem = selectedEventWrapper.Event;
                EventDetailsTextBlock.Text = $"Summary: {eventItem.Summary}\n" +
                                             $"Start: {(eventItem.Start.DateTime.HasValue ? eventItem.Start.DateTime.Value.ToString("g") : eventItem.Start.Date)}\n" +
                                             $"End: {(eventItem.End.DateTime.HasValue ? eventItem.End.DateTime.Value.ToString("g") : eventItem.End.Date)}\n" +
                                             $"Description: {eventItem.Description ?? "N/A"}\n" +
                                             $"Location: {eventItem.Location ?? "N/A"}";
            }
            else
            {
                EventDetailsTextBlock.Text = string.Empty;
            }
        }

        private class EventWrapper
        {
            public EventWrapper(Event eventItem)
            {
                Event = eventItem;
            }

            public Event Event { get; }

            public override string ToString()
            {
                return $"{Event.Summary} ({Event.Start.DateTime?.ToString("g") ?? Event.Start.Date})";
            }
        }
    }
}
