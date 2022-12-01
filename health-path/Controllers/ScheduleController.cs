using Dapper;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using health_path.Model;

namespace health_path.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScheduleController : ControllerBase
{
    private readonly ILogger<ScheduleController> _logger;
    private readonly IDbConnection _connection;

    public ScheduleController(ILogger<ScheduleController> logger, IDbConnection connection)
    {
        _logger = logger;
        _connection = connection;
    }

    [HttpGet]
    public ActionResult<IEnumerable<ScheduleEvent>> Fetch()
    {
        var dbResults = ReadData();

        // var preparedResults = dbResults.Select((t) => {
        //     t.Item1.Recurrences.Add(t.Item2);
        //     return t.Item1;
        // });
        
        // First Solution
        var preparedResults = dbResults.GroupBy(g=>g.Item1.Id)
        .Select(x=> new ScheduleEvent {
            Id = x.Key,
            Name = x.ToList()[0].Item1.Name,
            Description = x.ToList()[0].Item1.Description,
            Recurrences = x.Select(r=>r.Item2).ToList()
        });

        // Second Solution
        // List<ScheduleEvent> preparedResults = new List<ScheduleEvent>();
        // var groupedResults = dbResults.GroupBy(p => p.Item1.Id);

        // foreach(var group in groupedResults)
        // {
        //     List<ScheduleEventRecurrence> _recurrences = new List<ScheduleEventRecurrence>();

        //     foreach(var item in group)
        //     {
        //         _recurrences.Add(item.Item2);
        //     }

        //     preparedResults.Add(new ScheduleEvent(){
        //         Id = group.Key,
        //         Name = group.ToList()[0].Item1.Name,
        //         Description = group.ToList()[0].Item1.Description,
        //         Recurrences = _recurrences
        //     });
        // }
        

        return Ok(preparedResults);
    }

    private IEnumerable<(ScheduleEvent, ScheduleEventRecurrence)> ReadData() {
        var sql = @"
            SELECT e.*, r.*
            FROM Event e
            JOIN EventRecurrence r ON e.Id = r.EventId
            ORDER BY e.Id, r.DayOfWeek, r.StartTime, r.EndTime
        ";
        return _connection.Query<ScheduleEvent, ScheduleEventRecurrence, (ScheduleEvent, ScheduleEventRecurrence)>(sql, (e, r) => (e, r));
    }
}
