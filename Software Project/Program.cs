using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

public class EventsCollection
{
    public List<Event> Events { get; set; } = new List<Event>();

    public void AddEvent(Event newEvent)
    {
        Events.Add(newEvent);
    }

    public void RemoveEvent(int index)
    {
        if (index >= 0 && index < Events.Count)
            Events.RemoveAt(index);
        else
            Console.WriteLine("Invalid event number.");
    }

    public void SaveEvents(string filename)
    {
        using (StreamWriter writer = new StreamWriter(filename))
        {
            foreach (var ev in Events)
            {
                writer.WriteLine($"Title: {ev.Title}");
                writer.WriteLine($"Time: {ev.Time}");
                writer.WriteLine($"Locatin: {ev.Location}");
                writer.WriteLine();
            }
        }
    }

    public static EventsCollection LoadEvents(string filename)
    {
        EventsCollection events = new EventsCollection();

        if (!File.Exists(filename))
        {
            Console.WriteLine($"File not found: {filename}");
            return events;
        }

        try
        {
            using (StreamReader reader = new StreamReader(filename))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.StartsWith("Title:"))
                    {
                        Event newEvent = new Event();
                        newEvent.Title = line.Substring("Title:".Length).Trim();

                        // Read next lines for Time and Location
                        if ((line = reader.ReadLine()) != null && line.StartsWith("Time:"))
                        {
                            if (DateTime.TryParse(line.Substring("Time:".Length).Trim(), out DateTime time))
                            {
                                newEvent.Time = time;
                            }
                        }

                        if ((line = reader.ReadLine()) != null && line.StartsWith("Location:"))
                        {
                            newEvent.Location = line.Substring("Location:".Length).Trim();
                        }

                        events.AddEvent(newEvent);
                    }
                }
            }

            return events;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading events from file: {ex.Message}");
            return new EventsCollection();
        }
    }

    public List<Event> GetEventsOnDate(DateTime date)
    {
        return Events.FindAll(ev => ev.Time.Date == date.Date);
    }
}

public class Event
{
    public string Title { get; set; }
    public DateTime Time { get; set; }
    public string Location { get; set; }
}

public class Program
{
    static EventsCollection eventsCollection;
    static string filename = "events.txt";

    public static void Main(string[] args)
    {
        eventsCollection = EventsCollection.LoadEvents(filename);
        MainMenuAsync(args).GetAwaiter().GetResult();
    }

    public static async Task MainMenuAsync(string[] args)
    {
        bool finish = false;

        while (!finish)
        {
            Console.WriteLine("Gigs and Events Calendar");
            Console.WriteLine("-------------------------");
            Console.WriteLine("1. View all events");
            Console.WriteLine("2. Add an event");
            Console.WriteLine("3. Edit an event");
            Console.WriteLine("4. Remove an event");
            Console.WriteLine("5. Save events");
            Console.WriteLine("6. Search events by date");
            Console.WriteLine("7. Import events from file");
            Console.WriteLine("8. Exit");
            Console.Write("Enter your choice: ");

            int choice;
            if (int.TryParse(Console.ReadLine(), out choice))
            {
                switch (choice)
                {
                    case 1:
                        ViewEvents();
                        break;

                    case 2:
                        await AddEventAsync();
                        break;

                    case 3:
                        EditEvent();
                        break;

                    case 4:
                        RemoveEvent();
                        break;

                    case 5:
                        SaveEvents();
                        break;

                    case 6:
                        await SearchEventsByDateAsync();
                        break;

                    case 7:
                        ImportEventsFromFile();
                        break;

                    case 8:
                        finish = true;
                        break;

                    default:
                        Console.WriteLine("Invalid choice. Please choose again.");
                        break;
                }
            }
            else
            {
                Console.WriteLine("Please only enter a valid number.");
            }
        }
    }

    static void ViewEvents()
    {
        if (eventsCollection.Events.Count == 0)
        {
            Console.WriteLine("\n No events to display. \n");
            return;
        }

        Console.WriteLine("Events:");
        for (int i = 0; i < eventsCollection.Events.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {eventsCollection.Events[i].Title} - {eventsCollection.Events[i].Time.ToShortDateString()} - {eventsCollection.Events[i].Location}");
        }
    }

    static async Task AddEventAsync()
    {
        Console.Write("Enter title of event: ");
        string title = Console.ReadLine();

        Console.Write("Enter a date (MM/dd/yyyy): ");
        DateTime date;
        if (!DateTime.TryParse(Console.ReadLine(), out date))
        {
            Console.WriteLine("Use correct date format (MM/dd/yyyy). \n");
            return;
        }

        Console.Write("Event location: ");
        string location = Console.ReadLine();

        eventsCollection.AddEvent(new Event { Title = title, Time = date, Location = location });
        Console.WriteLine("\n Event added \n");
    }

    static void EditEvent()
    {
        ViewEvents();

        Console.Write("Enter the number of the event to edit: ");
        int index;
        if (!int.TryParse(Console.ReadLine(), out index) || index < 1 || index > eventsCollection.Events.Count)
        {
            Console.WriteLine("Invalid event number.");
            return;
        }

        Console.WriteLine($"Editing event: {eventsCollection.Events[index - 1].Title}");
        Console.Write("Enter new title (leave empty to keep current): ");
        string newTitle = Console.ReadLine();

        Console.Write("Enter new date (MM/dd/yyyy format) (leave empty to keep current): ");
        string dateInput = Console.ReadLine();
        DateTime newDate;
        if (!string.IsNullOrWhiteSpace(dateInput) && DateTime.TryParse(dateInput, out newDate))
        {
            eventsCollection.Events[index - 1].Time = newDate;
        }

        Console.Write("Enter new location (leave empty to keep current): ");
        string newLocation = Console.ReadLine();

        if (!string.IsNullOrWhiteSpace(newTitle))
        {
            eventsCollection.Events[index - 1].Title = newTitle;
        }

        if (!string.IsNullOrWhiteSpace(newLocation))
        {
            eventsCollection.Events[index - 1].Location = newLocation;
        }

        Console.WriteLine("\n Event updated \n");
    }

    static void RemoveEvent()
    {
        ViewEvents();

        Console.Write("Enter the number of the event you want to remove: ");
        int index;
        if (!int.TryParse(Console.ReadLine(), out index) || index < 1 || index > eventsCollection.Events.Count)
        {
            Console.WriteLine("Invalid event number.");
            return;
        }

        eventsCollection.RemoveEvent(index - 1);

        Console.WriteLine("\nEvent removed \n");
    }

    static async Task SearchEventsByDateAsync()
    {
        Console.Write("Enter date to search events (MM/dd/yyyy): ");
        DateTime searchDate;
        if (!DateTime.TryParse(Console.ReadLine(), out searchDate))
        {
            Console.WriteLine("Use correct date format (MM/dd/yyyy). \n");
            return;
        }

        List<Event> eventsOnDate = eventsCollection.GetEventsOnDate(searchDate);
        if (eventsOnDate.Count == 0)
        {
            Console.WriteLine($"No events found on {searchDate.ToShortDateString()}");
            return;
        }

        Console.WriteLine($"Events on {searchDate.ToShortDateString()}:");
        foreach (var ev in eventsOnDate)
        {
            Console.WriteLine($"{ev.Title} - {ev.Time.ToShortTimeString()} - {ev.Location}");
        }
    }

    static void SaveEvents()
    {
        Console.Write("Enter the filename to save events to (e.g., events.txt): ");
        string saveFilename = Console.ReadLine();

        eventsCollection.SaveEvents(saveFilename);
        Console.WriteLine("\n Events saved. \n");
    }

    static void ImportEventsFromFile()
    {
        Console.Write("Enter the filename to import events from (e.g., Saved_Events.txt): ");
        string importFilename = Console.ReadLine();

        if (!File.Exists(importFilename))
        {
            Console.WriteLine("File not found.");
            return;
        }

        EventsCollection importedEvents = EventsCollection.LoadEvents(importFilename);
        eventsCollection.Events.AddRange(importedEvents.Events);

        Console.WriteLine("\n Events imported successfully. \n");
    }
}
