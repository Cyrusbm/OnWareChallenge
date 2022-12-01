using Dapper;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace health_path.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NewsletterController : ControllerBase
{
    private readonly ILogger<NewsletterController> _logger;
    private readonly IDbConnection _connection;

    public NewsletterController(ILogger<NewsletterController> logger, IDbConnection connection)
    {
        _logger = logger;
        _connection = connection;
    }

    [HttpPost]
    public ActionResult Subscribe(string Email)
    {
        string _query = @"
            INSERT INTO NewsletterSubscription (Email)
            SELECT *
            FROM ( VALUES (@Email) ) AS V(Email)
            WHERE NOT EXISTS ( SELECT * FROM NewsletterSubscription e WHERE ";

        if (!Email.ToLower().EndsWith("@gmail.com"))
            _query += "e.Email = v.Email )";
        else
            _query += "REPLACE(e.Email,'.','') = REPLACE(v.Email,'.',''))";
        
        var inserted = _connection.Execute(_query, new { Email = Email });
        return inserted == 0 ? Conflict("email is already subscribed") : Ok();
    }
}
