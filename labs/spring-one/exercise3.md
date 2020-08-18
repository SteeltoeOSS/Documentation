[vs-add-efcore]: /site-data/labs/spring-one/images/vs-add-efcore.png "SqlServer EFCore nuget dependency"

[home-page-link]: /labs/spring-one
[exercise-1-link]: /labs/spring-one/exercise1
[exercise-2-link]: /labs/spring-one/exercise2
[exercise-3-link]: /labs/spring-one/exercise3
[exercise-4-link]: /labs/spring-one/exercise4
[exercise-5-link]: /labs/spring-one/exercise5

## Adding a cloud connector with SQL

### Goal

Add a ToDo list data context and item model to the app to see how Steeltoe manages the connection.

### Expected Results

App initializes the database and serves new endpoint for interacting with Todo list items.

### Get Started

We're going to add a database connection and context using entity framework code, in the previously created application. To get started add the Steeltoe package `Steeltoe.Connector.SqlServer.EFCore`.

![vs-add-efcore]

Or use the dotnet cli:

```powershell
dotnet add package Steeltoe.Connector.SqlServer.EFCore
```

Now create a new folder named `Models` and within create a new class named `TodoContext.cs`. This class will server as our context for interacting with the database. Paste the following in the class.

```csharp
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models {
  public class TodoContext : DbContext { 
    public TodoContext(): base(){ }  
    public TodoContext(DbContextOptions<TodoContext> options)
        : base(options) {
    }

    public DbSet<TodoItem> TodoItems { get; set; }
  }
}
```

Also in the `Models` folder, create a class named `TodoItem.cs` and paste the following within. This will serve a definition of the things that make up a ToDo list item.

```csharp
using System;

namespace WebApplication1.Models {
  public class TodoItem {
    public long Id { get; set; }
    public string Name { get; set; }
    public bool IsComplete { get; set; }
  }
}
```

Now head over to `Startup.cs` and add the 'DBContext'. Visual Studio should prompt you to add `using Steeltoe.Connector.SqlServer.EFCore` package. Note - there is not need to add any other packages like a SqlClient. The Steeltoe package takes care of everything.

```csharp
public void ConfigureServices(IServiceCollection services) {
  services.AddDbContext<TodoContext>(options => options.UseSqlServer(Configuration));

  //...
}
```

Because we are going to interacting with a brand new database server we'll need to make sure the database has been initialized before the application can fully start up. In  `Startup.cs` adjust the input parmaeters of the `Configure` function to include the TodoContext and add the `EnsureCreated` command as the last line in the function.

```csharp
public void Configure(IApplicationBuilder app, IWebHostEnvironment env, TodoContext context) {
  
  //...

  context.Database.EnsureCreated();
}
```

Create a new class in the `Controllers` folder named `TodoItemsController.cs` and paste the following within.

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TodoItemsController : ControllerBase
    {
        private readonly TodoContext _context;

        public TodoItemsController(TodoContext context)
        {
            _context = context;
        }

        // GET: api/TodoItems1
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TodoItem>>> GetTodoItems()
        {
            return await _context.TodoItems.ToListAsync();
        }

        // GET: api/TodoItems1/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TodoItem>> GetTodoItem(long id)
        {
            var todoItem = await _context.TodoItems.FindAsync(id);

            if (todoItem == null)
            {
                return NotFound();
            }

            return todoItem;
        }

        // PUT: api/TodoItems1/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTodoItem(long id, TodoItem todoItem)
        {
            if (id != todoItem.Id)
            {
                return BadRequest();
            }

            _context.Entry(todoItem).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TodoItemExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/TodoItems1
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<TodoItem>> PostTodoItem(TodoItem todoItem)
        {
            _context.TodoItems.Add(todoItem);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTodoItem", new { id = todoItem.Id }, todoItem);
        }

        // DELETE: api/TodoItems1/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<TodoItem>> DeleteTodoItem(long id)
        {
            var todoItem = await _context.TodoItems.FindAsync(id);
            if (todoItem == null)
            {
                return NotFound();
            }

            _context.TodoItems.Remove(todoItem);
            await _context.SaveChangesAsync();

            return todoItem;
        }

        private bool TodoItemExists(long id)
        {
            return _context.TodoItems.Any(e => e.Id == id);
        }
    }
}

```

Finally overwrite default values in `appsettings.json` that the Steeltoe package will use when connecting to the database.

```json
{
  "$schema": "https://steeltoe.io/schema/latest/schema.json",
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  },
  "AllowedHosts": "*",
  "sqlserver": {
    "credentials": {
      //"server": null,
      //"port": null,
      "username": "<PROVIDE_VALUE>",
      "password": "<PROVIDE_VALUE>"
    }
  },
  "management": {
    "endpoints": {
      "actuator": {
        "exposure": {
          "include": [ "*" ]
        }
      }
    }
  }
}
```

Before we see everything in action lets review what has been done. With the Steeltoe EFCore package added, we create a definition of a database context and list item, and then used them in startup to be a part of dependency injection. But instead of bringing in the typical SqlClient packages to help define things, we used `Steeltoe.Connector.SqlServer.EFCore`. This package not only has all needed sub-packages included but also introduces easily configurable settings with default values. To learn more about what values can be customised, [have a look at the docs](https://steeltoe.io/docs/3/connectors/microsoft-sql-server). In our example we're using the default port of '1433' and a server name of 'localhost'. If you wanted the app to connect to a SQL database hosted elsewhere you could privde different values in appsettings.json. We've provided the required credentials and server name in appsettings. The database name will be derived from our ToDo context. They key is to give Steeltoe connector a valid healthy connection to SQL, it will do the rest.

To get everything going you're going to need a runing instance of SQL. Depending on how that instance is made available, you'll want to adjust the values in `appsettings.json`.

- [Running SQL locally](https://www.sqlservertutorial.net/install-sql-server/): Mostly likely the instance is running on `localhost` through port `1433`. If so then continue on, thats the default.
- [Running in docker (desktop)](https://hub.docker.com/_/microsoft-mssql-server): If it's available on 'localhost' port '1433' then continue on. Otherwise you'll need to uncomment the `server` and `port` parameters in appsettings and provide valid values.
- Running outside your local desktop: If SQL is running somewhere else you'll need its URI and port number. Uncomment the `server` and `port` parameters in appsettings and provide valid values.

As for the `username` and `password` values in `appsettings.json`, replace the placeholder with valid values. The account will need enough permission to create a new database and add tabels within. An example docker command you could run locally to get everything going is:

```powershell
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=IheartSteeltoe1" -p 1433:1433 -d mcr.microsoft.com/mssql/server:2019-latest
```

Assuming all the SQL requirements are taken care of, start up the application by clicking the `Debug > Start Debugging` top menu item.

![vs-run-application]

Or use the dotnet cli:

```powershell
dotnet run
```

Once started your default browser should open and automatically load the weather forecast endpoint.

![run-weatherforecast]

First lets make sure t

|[<< Previous Exercise][exercise-2-link]|[Next Exercise >>][exercise-4-link]|
|:--|--:|
