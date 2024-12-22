# Sharing HTTP and Hangfire Context in a .NET 8 API

## Overview

In this project, we tackle the challenge of sharing context between HTTP requests and Hangfire jobs in a .NET 8 API. Specifically, we focus on accessing and managing contextual data, like `CategoryId`, seamlessly across both environments. 

This repository demonstrates a clean and scalable approach to solving this problem, leveraging:

- **`IContextAccessor` and `IContextDataProvider`**: Interfaces for managing context data.
- **`ContextAccessor`**: Handles context during HTTP requests.
- **`ContextDataProvider`**: Handles context during Hangfire job execution.
- **Dependency Injection**: A strategic setup ensures correct resolution of context depending on whether it's an HTTP request or a Hangfire job.

## Problem Statement

When working with Hangfire, background jobs run outside of the standard HTTP request pipeline, meaning there is no `HttpContext` to access headers or other contextual information. This creates a challenge when shared context data, such as `CategoryId`, is required across both HTTP requests and background jobs.

## Solution

To address this, we use:

1. **Two context implementations:**
    - **`ContextAccessor`**: Retrieves the `CategoryId` from HTTP headers.
    - **`ContextDataProvider`**: Allows explicitly setting the `CategoryId` for background jobs.
2. **A unified interface (`IContextAccessor`)** to abstract the logic for accessing contextual data.
3. **Dynamic resolution of context accessor** through Dependency Injection (DI), ensuring the correct implementation is used based on the execution context.

## Project Structure

### Interfaces

```csharp
public interface IContextAccessor
{
    Category GetCategory();
}

public interface IContextDataProvider : IContextAccessor
{
    void SetCategoryId(int categoryId);
}
```

### `ContextAccessor` (For HTTP Requests)

```csharp
public class ContextAccessor(IHttpContextAccessor httpContextAccessor, NotesDbContext dbContext)
    : IContextAccessor
{
    private Category? category;

    public Category GetCategory()
    {
        if (this.category is not null) return this.category;

        if (!Int32.TryParse(
                (string?)httpContextAccessor.HttpContext?.Request.Headers["CategoryId"],
                out var categoryId))
            throw new Exception("Category not found");

        var category = dbContext.Categories.FirstOrDefault(c => c.Id == categoryId);
        this.category = category ?? throw new Exception("Category not found");

        return this.category;
    }
}
```

### `ContextDataProvider` (For Hangfire Jobs)

```csharp
public class ContextDataProvider(NotesDbContext dbContext) : IContextDataProvider
{
    private int? categoryId;
    private Category? category;

    public Category GetCategory()
    {
        if (this.category is not null) return this.category;

        if (this.categoryId is null) throw new Exception("Category not found");

        var category = dbContext.Categories.FirstOrDefault(c => c.Id == this.categoryId);
        this.category = category ?? throw new Exception("Category not found");

        return this.category;
    }

    public void SetCategoryId(int categoryId)
    {
        this.categoryId = categoryId;
    }
}
```

### Dependency Injection Setup

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddHttpContextAccessor()
        .AddDbContext<NotesDbContext>(options => options.UseInMemoryDatabase("NotesDb"))
        .AddScoped<NotesCleanupService>()
        .AddScoped<NotesService>()
        .AddScoped<IDataSeedProvider, DataSeedProvider>()
        .AddHangfire(config => config.UseInMemoryStorage())
        .AddHangfireServer(options => { options.Queues = ["default"]; })
        .AddSwaggerGen()
        .AddControllers();

    services.AddScoped<ContextDataProvider>();
    services.AddScoped<IContextDataProvider>(
        x => x.GetRequiredService<ContextDataProvider>());

    services.AddScoped<ContextAccessor>();
    services.AddScoped<IContextAccessor>(
        sp =>
        {
            var httpContext = sp.GetService<IHttpContextAccessor>()?.HttpContext;
            if (httpContext != null)
                return sp.GetRequiredService<ContextAccessor>();

            return sp.GetRequiredService<IContextDataProvider>();
        });
}
```

### Example Hangfire Filter (Alternative Approach)

Another possible solution involves using Hangfire filters (`IServerFilter`) to mark and detect Hangfire-specific execution contexts:

```csharp
public class HangfireContextFilter : IServerFilter
{
    public void OnPerforming(PerformingContext context)
    {
        HangfireContext.IsHangfireContext = true;
    }

    public void OnPerformed(PerformedContext context)
    {
        HangfireContext.IsHangfireContext = false;
    }
}

public static class HangfireContext
{
    private static readonly AsyncLocal<bool> IsHangfireContextValue = new AsyncLocal<bool>();

    public static bool IsHangfireContext
    {
        get => IsHangfireContextValue.Value;
        set => IsHangfireContextValue.Value = value;
    }
}
```

## How It Works

- **HTTP Context:** When a request arrives, `ContextAccessor` reads `CategoryId` from the HTTP headers and fetches the category from the database.
- **Hangfire Context:** For background jobs, `ContextDataProvider` allows explicitly setting the `CategoryId` during the job setup. 
- **Dynamic Resolution:** The DI setup determines whether to use `ContextAccessor` or `ContextDataProvider` based on the availability of `HttpContext`.

## Running the Project

1. Clone this repository.
2. Restore NuGet packages.
3. Run the application and test HTTP requests using the provided controllers.
4. Trigger background jobs via the Hangfire dashboard or API endpoints.

## Key Learnings

- **Avoiding Circular Dependencies:** Ensure `IContextAccessor` and services like `NotesService` don’t call each other directly to prevent infinite resolution loops.
- **Scalability:** The solution is extensible for additional contexts or new requirements.
- **Flexibility:** Explicitly passing contextual data like `CategoryId` in Hangfire jobs provides more control than global flags.

---

Feel free to explore the code and adapt it to your projects. Contributions and feedback are welcome!

# Technologies Used
- .NET 8
- Hangfire
- Entity Framework Core (In-Memory Database)
- ASP.NET Core

---

## References
- [How to Configure Hangfire in a .NET 8 API with Secure Dashboard Access and Job Prioritization](https://elguerre.com/2024/09/30/how-to-configure-hangfire-in-a-net-8-api-with-secure-dashboard-access-and-job-prioritization/)
