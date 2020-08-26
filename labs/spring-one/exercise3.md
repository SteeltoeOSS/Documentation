---
uid: labs/spring-one/exercise3
_disableToc: true
---
[vs-add-efcore]: ~/labs/images/vs-add-efcore.png "SqlServer EFCore nuget dependency"
[single-todoitem]: ~/labs/images/single-todoitem.png "ToDo item retrieved from the database"
[run-weatherforecast]: ~/labs/images/weatherforecast-endpoint.png "Weatherforecast endpoint"
[vs-run-application]: ~/labs/images/vs-run-application.png "Run the project"

[home-page-link]: index.md
[exercise-1-link]: exercise1.md
[exercise-2-link]: exercise2.md
[exercise-3-link]: exercise3.md
[exercise-4-link]: exercise4.md

|  |
|---------:|
|[Back to intro](index.md)&nbsp;&nbsp;&nbsp;|

# Adding a cloud connector with SQL

## Goal

Add a ToDo list data context and item model to the app to see how Steeltoe manages the connection.

## Expected Results

App initializes the database and serves new endpoint for interacting with Todo list items.

## Get Started

We're going to add a database connection and context using entity framework code to the previously created application. To get started add the Steeltoe package `Steeltoe.Connector.SqlServer.EFCore`.

# [.NET CLI](#tab/dotnet-cli)

```powershell
dotnet add package Steeltoe.Connector.SqlServer.EFCore
```

# [Visual Studio](#tab/visual-studio)

![vs-add-efcore]

***

## Add database context and model

Now create a new folder named `Models`. Within create a new class named `TodoContext.cs`. This class will serve as our context for interacting with the database. Paste the following in the class.

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

Also in the `Models` folder, create a class named `TodoItem.cs` and paste the following within. This will serve as a definition of the things that make up a ToDo list item.

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

## Implement the database context and ensure its creation

Now head over to `Startup.cs` and add the 'DBContext'. Visual Studio should prompt you to add `using Steeltoe.Connector.SqlServer.EFCore` package.

[NOTE!]
There is not need to add any other packages like a SqlClient. The Steeltoe package takes care of everything.

```csharp
public void ConfigureServices(IServiceCollection services) {
  services.AddDbContext<TodoContext>(options => options.UseSqlServer(Configuration));

  //...
}
```

Because we are going to be interacting with a brand new database instance we'll need to make sure the database has been initialized before the application can fully start up. In `Startup.cs` adjust the input parameters of the `Configure` function to include the TodoContext and add the `EnsureCreated` command as the last line in the function.

```csharp
public void Configure(IApplicationBuilder app, IWebHostEnvironment env, TodoContext context) {
  
  //...

  context.Database.EnsureCreated();
}
```

## Create a Todo controller

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
        private readonly ILogger<TodoItemsController> _logger;

        public TodoItemsController(TodoContext context, ILogger<TodoItemsController> logger)
        {
            _context = context;
            _logger = logger;
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
            if (id == 0) {
              var newItem = new TodoItem() {
                IsComplete = false,
                Name = "A new auto-generated todo item"
              };

              _context.TodoItems.Add(newItem);
              await _context.SaveChangesAsync();

              _logger.LogInformation("Super secret id==0 was provided, so a new item was auto-added.");
            }

            var todoItem = await _context.TodoItems.FindAsync(id);

            if (todoItem == null)
            {
                return NotFound();
            }

            return todoItem;
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

        private bool TodoItemExists(long id)
        {
            return _context.TodoItems.Any(e => e.Id == id);
        }
    }
}

```

## Update appsettings.json with database connection

To get a running instance of SQL, you could go a few different paths. Depending on how the instance is made available, you'll want to adjust the values in `appsettings.json`.

# [Visual Studio LocalDB](#tab/Visual-Studio-LocalDB)

Using Visual Studio's built in SQL: If your Visual Studio installation has the ".NET Desktop Development" feature enabled and within the "SQL Server Express LocalBD" option is selected, then you have a ready to go SQL instance. Uncomment the `server` option and provide a value of `(localdb)\MSSQLLocalDB`. The port will default to 1433. If you would like to confirm the database server is running, open powershell and run `sqllocaldb i MSSQLLocalDB`.

# [Local & Docker SQL](#tab/Local-SQL)

[Running SQL locally](https://www.sqlservertutorial.net/install-sql-server/): Mostly likely the instance is running on `localhost` through port `1433`. If so then continue on, thats the default.
[Running in docker (desktop)](https://hub.docker.com/_/microsoft-mssql-server): If it's available on 'localhost' port '1433' then continue on. Otherwise you'll need to uncomment the `server` and `port` parameters in appsettings and provide valid values.

An example docker command you could run locally to get everything going is:

```powershell
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=IheartSteeltoe1" -p 1433:1433 -d mcr.microsoft.com/mssql/server:2019-latest
```

# [Other](#tab/other-sql)

Running outside your local desktop: If SQL is running somewhere else you'll need its URI and port number. Uncomment the `server` and `port` parameters in appsettings and provide valid values.

***

As for the `username` and `password` values in `appsettings.json` if you are using Visual Studio's built in SQL leave them commented out. Otherwise replace the placeholder with valid values. The account will need enough permission to create a new database and add tables within. 

Overwrite default values in `appsettings.json` so that Steeltoe can connect to the database instance.

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
      "server": %%SQL_SERVER_ADDRESS%%,
      "port": %%SQL_SERVER_PORT%%,
      "username": %%SQL_SERVER_USERNAME%%,
      "password": %%SQL_SERVER_PASSWORD%%
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

## Review what was done

Before we see everything in action lets review what has been done. With the Steeltoe EFCore package added we create a definition of a database context and list item. Then we used them in startup to be a part of dependency injection. Instead of bringing in the typical SqlClient packages to help define things, we used `Steeltoe.Connector.SqlServer.EFCore`. This package not only has all needed sub-packages included but also introduces easily configurable settings. To learn more about what values can be customised, [have a look at the docs](https://steeltoe.io/docs/3/connectors/microsoft-sql-server). In our example we're using the default port of '1433' and a server name of 'localhost'. If you wanted the app to connect to a SQL database hosted elsewhere you could provide different values. Also we've provided the required credentials and server name in `appsettings.json`. The database name will be derived from our ToDo context. They key is to give the Steeltoe connector a valid healthy connection to a SQL instance, it will do the rest.

## Run the application

# [.NET CLI](#tab/dotnet-cli)

```powershell
dotnet run
```

# [Visual Studio](#tab/visual-studio)

![vs-run-application]

***

Once started your default browser should open and automatically load the weather forecast endpoint.

![run-weatherforecast]

## Work with saved ToDo items

To test the database connection, navigate to the "GET" endpoint where all saved ToDo list items will be retrieved. Oh wait! It's a new database there aren't any items saved yet. Let add a new ToDo list item. If you noticed in the `GetTodoItem` method of the `TodoItemsController`, there is a super secret value you can provide to add new list items. Replace `WeatherForecast` with `api/TodoItems/0` in the browser address bar. This page should load successfully but not provide much feedback. Behind the scenes you've just added a new list item. To confirm, lets retrieve the saved list of items by removing the `\0` in the address and loading the page. Wow! Now there is 1 list item retrieved from the database. Awesome!

![single-todoitem]

## Summary

We've done quite a bit in this exercise but notice it was mostly focused on working with the ToDo list. Thats the purpose of this service. Working on business logic and spending less time with the boiler plate stuff is one of Steeltoe's super powers.

|[<< Previous Exercise][exercise-2-link]|[Next Exercise >>][exercise-4-link]|
|:--|--:|
