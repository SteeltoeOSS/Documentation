---
uid: labs/spring-one/exercise3
_disableContribution: true
_disableToc: true
_disableFooter: true
_homePath: "./"
_disableNav: true
---
[vs-add-efcore]: ~/labs/images/vs-add-efcore.png "Steeltoe EFCore nuget dependency"
[single-todoitem]: ~/labs/images/single-todoitem.png "ToDo item retrieved from the database"
[run-weatherforecast]: ~/labs/images/weatherforecast-endpoint.png "Weatherforecast endpoint"
[vs-run-application]: ~/labs/images/vs-run-application.png "Run the project"
[vs-new-folder]: ~/labs/images/vs-new-folder.png "Create a new project folder"
[vs-new-class]: ~/labs/images/vs-new-class.png "Create a new project class"
[vs-add-efsqlserver]: ~/labs/images/vs-add-efsqlserver.png "Microsoft SqlServer EFCore nuget dependency"

[home-page-link]: index.md
[exercise-1-link]: exercise1.md
[exercise-2-link]: exercise2.md
[exercise-3-link]: exercise3.md
[exercise-4-link]: exercise4.md

|[<< Previous Exercise][exercise-2-link]|[Next Exercise >>][exercise-4-link]|
|:--|--:|

# Adding a cloud connector with SQL

## Goal

Add a ToDo list data context and item model to the app to see how Steeltoe manages the connection.

## Expected Results

App initializes the database and serves new endpoint for interacting with Todo list items.

> [!NOTE]
> For this exercise an MS SQL database have already been initialized. The settings have been preloaded below.

## Get Started

We're going to add a database connection and context using entity framework to the previously created application.

# [Visual Studio](#tab/visual-studio)

Right click on the project name in the solution explorer and choose "Manage NuGet packages...". In the package manger window choose "Browse", then search for `Steeltoe.Connector.EFCore`, and install.
![vs-add-efcore]

Then search for the `Microsoft.EntityFrameworkCore.SqlServer` package and install.
![vs-add-efsqlserver]

# [.NET CLI](#tab/dotnet-cli)

```powershell
dotnet add package Steeltoe.Connector.EFCore
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
```

***

## Add database context and model

Now create a new folder in the project named 'Models'.

# [Visual Studio](#tab/visual-studio)

Right click on the project name in the solution explorer and choose "Add" > "New Folder" and name it `Models`.

![vs-new-folder]

# [.NET CLI](#tab/dotnet-cli)

```powershell
mkdir "Models"
cd "Models"
```

***

Within that folder create a new class named 'TodoContext.cs'. This class will serve as our context for interacting with the database.

# [Visual Studio](#tab/visual-studio)

Right click on the 'Models' folder and choose "Add" > "Class..." and name it `TodoContext.cs`.

![vs-new-class]

# [.NET CLI](#tab/dotnet-cli)

```powershell
dotnet new classlib -n "TodoContext.cs"
```

***

Open the newly created class file in your IDE and include the 'EntityFrameworkCore' package.

```csharp
using Microsoft.EntityFrameworkCore;
```

Also replace the class statement with this. Don't change the 'namespace' part, just the class within the namespace.

```csharp
public class TodoContext : DbContext { 
  public TodoContext(): base(){ }  
  public TodoContext(DbContextOptions<TodoContext> options)
      : base(options) {
  }

  public DbSet<TodoItem> TodoItems { get; set; }
}
```

Also in the 'Models' folder, create a class named 'TodoItem.cs'. This will serve as a definition of the things that make up a ToDo list item.

# [Visual Studio](#tab/visual-studio)

Right click on the 'Models' folder and choose "Add" > "Class..." and name it `TodoItem.cs`.

![vs-new-class]

# [.NET CLI](#tab/dotnet-cli)

```powershell
dotnet new classlib -n "TodoItem.cs"
```

***

Open the newly created class file in your IDE and replace th class statement with this. Don't change the 'namespace' part, just the class within the namespace.

```csharp
public class TodoItem {
  public long Id { get; set; }
  public string Name { get; set; }
  public bool IsComplete { get; set; }
}
```

> [!TIP]
> The 'TodoItem' class is whats known as a POCO. Plain Old Csharp Object. No fancy stuff... don't even need the preloaded 'using' statements if you'd like to remove them.

## Implement the database context and ensure its creation

Now that we have created the 'TodoContext' we need to add it to the services container.

> [NOTE!]
> If prompted, there is not need to add any other packages like a SqlClient. The Steeltoe package takes care of everything.

Open "Startup.cs" in your IDE and add the using statement

```csharp
using Steeltoe.Connector.SqlServer.EFCore;
```

Then append the 'add db' statement to the 'ConfigureServices' method and save the changes

```csharp
public void ConfigureServices(IServiceCollection services) {
  services.AddDbContext<Models.TodoContext>(options => options.UseSqlServer(Configuration));

  //...
}
```

Because we are going to be interacting with a brand new database instance we'll need to make sure the database has been initialized before the application can fully start up. In "Startup.cs" adjust the input parameters of the 'Configure' function to include the TodoContext and add the 'EnsureCreated' command as the last line in the function.

```csharp
public void Configure(IApplicationBuilder app, IWebHostEnvironment env, Models.TodoContext context) {
  
  //...

  context.Database.EnsureCreated();
}
```

## Create a Todo controller

Create a new class in the 'Controllers' folder named `TodoItemsController.cs`.

# [Visual Studio](#tab/visual-studio)

Right click on the 'Controllers' folder and choose "Add" > "Class..." and name it `TodoItemsController.cs`.

![vs-new-class]

# [.NET CLI](#tab/dotnet-cli)

```powershell
cd ../Controllers
dotnet new classlib -n "TodoItemsController.cs"
```

***

Open the newly created class file in your IDE and replace the 'using' statements in the file with the below.

```csharp
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
```

Replace the class statement with this. Don't change the 'namespace' part, just the class within the namespace.

```csharp
[Route("api/[controller]")]
[ApiController]
public class TodoItemsController : ControllerBase
{
  private readonly Models.TodoContext _context;
  private readonly ILogger<TodoItemsController> _logger;

  public TodoItemsController(Models.TodoContext context, ILogger<TodoItemsController> logger)
  {
    _context = context;
    _logger = logger;
  }

  // GET: api/TodoItems1
  [HttpGet]
  public async Task<ActionResult<IEnumerable<Models.TodoItem>>> GetTodoItems()
  {
    return await _context.TodoItems.ToListAsync();
  }

  // GET: api/TodoItems/5
  [HttpGet("{id}")]
  public async Task<ActionResult<Models.TodoItem>> GetTodoItem(long id)
  {
    if (id == 0) {
      var newItem = new Models.TodoItem() {
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
}
```

## Update appsettings.json with database connection

<!-- To get a running instance of SQL, you could go a few different paths. Depending on how the instance is made available you'll want to adjust the values in `appsettings.json`.

# [Visual Studio LocalDB](#tab/Visual-Studio-LocalDB)

If your Visual Studio installation has this feature enabled (there's a very good chance), then you have a ready to go SQL instance. Uncomment the `ConnectionString` option and remove all other parameters (server, port, etc). If you would like to confirm the database server is running, open powershell and run `sqllocaldb i MSSQLLocalDB`.

# [Local & Docker SQL](#tab/Local-SQL)

Mostly likely the instance is running on `localhost` through port `1433`. If so then remove all parameters (server, port, etc) and let Steeltoe use buitin defaults. Otherwise use the provided parameters to configure the connection correctly.

An example docker command you could run an instance locally is:

```powershell
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=IheartSteeltoe1" -p 1433:1433 -d mcr.microsoft.com/mssql/server:2019-latest
```

# [Other](#tab/other-sql)

If your SQL instance is running somewhere else you'll need its URI port number and credentials. Use the provided parameters to configure the connection correctly.

*** -->
Add the below json to 'appsettings.json', just after the 'management' section. This will give Steeltoe connection information for the database instance as well as name the new database.

```json
,"sqlserver": {
  "credentials": {
    //"ConnectionString": "Server=(localdb)\\mssqllocaldb;database=Todo;Trusted_Connection=True;",
    "server": %%SQL_SERVER_ADDRESS%%,
    "port": %%SQL_SERVER_PORT%%,
    "username": %%SQL_SERVER_USERNAME%%,
    "password": %%SQL_SERVER_PASSWORD%%
  }
}
```

## Review what was done

Before we see everything in action lets review what has been done. With the Steeltoe EFCore package added we create a definition of a database context and list item. Then we used them in startup to be a part of dependency injection. Instead of bringing in the typical SqlClient packages to help define things, we used `Steeltoe.Connector.SqlServer.EFCore`. This package not only has all needed sub-packages included but also introduces easily configurable settings. To learn more about what values can be customized, [have a look at the docs](https://steeltoe.io/docs/3/connectors/microsoft-sql-server). In our example we're using the default port of '1433' and a server name of 'localhost'. If you wanted the app to connect to a SQL database hosted elsewhere you could provide different values. Also we've provided the required credentials and server name in `appsettings.json`. The database name will be derived from our ToDo context. They key is to give the Steeltoe connector a valid healthy connection to a SQL instance, it will do the rest.

## Run the application

With the data context in place, we are ready to see everything in action. Run the application.

# [Visual Studio](#tab/visual-studio)

Clicking the `Debug > Start Debugging` top menu item. You may be prompted to "trust the IIS Express SSL certificate" and install the certificate. It's safe, trust us. Once started your default browser should open and automatically load the weather forecast endpoint.

![vs-run-application]

# [.NET CLI](#tab/dotnet-cli)

Executing the below command will start the application. You will see a log message written telling how to navigate to the application. It should be [http://localhost:5000/weatherforecast](http://localhost:5000/weatherforecast).

```powershell
dotnet run
```

***

With the application running and the weather forecast endpoint loaded your browser should show the following

![run-weatherforecast]

## Work with saved ToDo items

To test the database connection, navigate to the "GET" endpoint where all saved ToDo list items will be retrieved. **Oh wait!** It's a new database there aren't any items saved yet. Let add a new ToDo list item.

You may have noticed in the 'TodoItemsController.GetTodoItem' method, there is a super secret value you can provide to add new list items. Replace `WeatherForecast` with `api/TodoItems/0` in the browser address bar. This page should load successfully but not provide much feedback. Behind the scenes you've just added a new list item. To confirm lets retrieve the saved list of items. Remove the `/0` in the address and loading the page. Wow! Now there is 1 list item retrieved from the database. Awesome!

![single-todoitem]

## Stop the application

# [Visual Studio](#tab/visual-studio)

Either close the browser window or click the red stop button in the top menu.

# [.NET CLI](#tab/dotnet-cli)

Use the key combination "ctrl+c" on windows/linux or "cmd+c" on Mac.

***

## Summary

We've done quite a bit in this exercise but notice it was mostly focused on working with the ToDo list. You never had to open a SQL editor, create a database, test the database, etc etc. Thats the purpose of this Steeltoe Connectors. They take care of all the messy behind-the-scenes work and let you focus on the business logic. Yeah we know, it's pretty awesome. Being awesome is one of Steeltoe's super powers.

|[<< Previous Exercise][exercise-2-link]|[Next Exercise >>][exercise-4-link]|
|:--|--:|
