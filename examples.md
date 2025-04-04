---
uid: examples
title: Example markdown
---
# These are example markdown items

> [!NOTE]
> Information the user should notice even if skimming.

> [!TIP]
> Optional information to help a user be more successful.

> [!IMPORTANT]
> Essential information required for user success.

> [!CAUTION]
> Negative potential consequences of an action.

> [!WARNING]
> Dangerous certain consequences of an action.

## Sectioned code

> [!div class="tabbedCodeSnippets" data-resources="OutlookServices.Calendar"]
> ```cs
> <cs code text>
> ```
> ```javascript
> <js code text>
> ```

## Tabs

# [.NET CLI](#tab/dotnet-cli)

```powershell
dotnet new webapi -n WebApplication1
cd WebApplication1
```

# [Visual Studio](#tab/visual-studio)

|![vs-new-proj] Choose ASP.NET Core Web Application from the default templates. |![vs-name-proj] The default project name WebApplication1 will be used throughout, but you can rename.|![vs-create-proj] Choose an application type of API, everything else can keep its default value.|
|:--|

***
## Embed a video
> [!Video https://www.youtube.com/embed/qftu_ku8jmM]

## Code Snippets

Using the `example-program.cs` file in the same folder as this markdown...

Show the entire file as formatted code:
[!code-csharp[<Main>](example-program.cs "<title>")]

Show only line 15 of the file:
[!code-csharp[<Main>](example-program.cs#L15 "<title>")]

Show like lines 9 thru 17:
[!code-csharp[<Main>](example-program.cs?start=9&end=17 "<title>")]

Show a combination of single lines and a range of lines:
[!code-csharp[<Main>](example-program.cs?range=2,5-7,9- "<title>")]

Highlight certain lines of code:
[!code-csharp[<Main>](example-program.cs?highlight=2,5,11 "<title>")]

Given the `#region` in the csharp, only show that snippet:
[!code-csharp[<Main>](example-program.cs?name=webhostsnippet "<title>")]
