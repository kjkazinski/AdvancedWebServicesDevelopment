using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Modas.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Authorization;
namespace Modas.Controllers
{
  [Route("api/[controller]"), Authorize]
  public class EventController : Controller
  {
    private EventDbContext eventDbContext;
    public EventController(EventDbContext db) => eventDbContext = db;
    [HttpGet, Route("count"), AllowAnonymous]
    // returns number of members in events collections
    public int GetCount() => eventDbContext.Events.Count();
    [HttpGet]
    // returns all events (sorted)
    public IEnumerable<Event> Get() => eventDbContext.Events
      .Include(e => e.Location).OrderBy(e => e.TimeStamp);
    [HttpGet("{id}")]
    // return specific event
    public Event Get(int id) => eventDbContext.Events
      .Include(e => e.Location)
      .FirstOrDefault(e => e.EventId == id);
    [HttpGet("pageSize/{pageSize:int}/page/{page:int}")]
    // returns all events by page
    public EventPage GetPage(int page = 1, int pageSize = 10) => new EventPage
    {
      Events = eventDbContext.Events
        .Select(e => new EventJson
        {
          id = e.EventId,
          flag = e.Flagged,
          stamp = e.TimeStamp,
          loc = e.Location.Name
        })
        .OrderByDescending(e => e.stamp)
        .Skip((page - 1) * pageSize)
        .Take(pageSize),
      PagingInfo = new PageInfo
      {
        CurrentPage = page,
        ItemsPerPage = pageSize,
        TotalItems = eventDbContext.Events.Count()
      }
    };
    [HttpPost]
    // add event
    public Event Post([FromBody] Event evt) => eventDbContext.AddEvent(new Event
    {
      TimeStamp = evt.TimeStamp,
      Flagged = evt.Flagged,
      LocationId = evt.LocationId
    });
    [HttpPut]
    // update event
    public Event Put([FromBody] Event evt) => eventDbContext.UpdateEvent(evt);
    [HttpPatch("{id}")]
    // update event (specific fields)
    public void Patch(int id, [FromBody]JsonPatchDocument<Event> patch) => eventDbContext.PatchEvent(id, patch);
    [HttpDelete("{id}")]
    // delete event
    public void Delete(int id) => eventDbContext.DeleteEvent(id);
  }
}
