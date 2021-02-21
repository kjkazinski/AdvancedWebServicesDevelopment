using System;
namespace Modas.Models
{
  public class Event
  {
    public int EventId { get; set; }
    public DateTime TimeStamp { get; set; }
    public bool Flagged { get; set; }
    // foreign key for location 
    public int LocationId { get; set; }
    // navigation property
    public Location Location { get; set; }
  }
  public class EventJson
  {
    public int id { get; set; }
    public DateTime stamp { get; set; }
    public bool flag { get; set; }
    public string loc { get; set; }
  }
}
