using Microsoft.EntityFrameworkCore;
using System.Linq;
using Microsoft.AspNetCore.JsonPatch;

namespace Modas.Models
{
  public class EventDbContext : DbContext
  {
    public EventDbContext(DbContextOptions<EventDbContext> options) : base(options) { }
    public DbSet<Event> Events { get; set; }
    public DbSet<Location> Locations { get; set; }

    public Event AddEvent(Event evt)
    {
      this.Events.Add(evt);
      this.SaveChanges();
      return evt;
    }
    public Event UpdateEvent(Event evt)
    {
      Event Event = this.Events.FirstOrDefault(e => e.EventId == evt.EventId);
      Event.TimeStamp = evt.TimeStamp;
      Event.Flagged = evt.Flagged;
      Event.LocationId = evt.LocationId;
      this.SaveChanges();
      return Event;
    }
    public void PatchEvent(int id, JsonPatchDocument<Event> patch)
    {
      Event evt = this.Events.FirstOrDefault(e => e.EventId == id);
      patch.ApplyTo(evt);
      this.SaveChanges();
    }
    public void DeleteEvent(int eventId)
    {
      Event evt = this.Events.FirstOrDefault(e => e.EventId == eventId);
      this.Events.Remove(evt);
      this.SaveChanges();
    }
  }
}
