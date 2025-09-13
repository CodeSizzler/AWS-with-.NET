# DynamoDB + .NET Lab — Getting Started

## Overview

This lab walks you through building a simple ASP.NET Core Web API that performs CRUD operations against **Amazon DynamoDB**. The entity we'll use is **Student** (Id, Name, Class, Country). Steps follow a hands-on flow: create a DynamoDB table, configure AWS credentials locally, scaffold a .NET Web API, add AWS SDK packages, implement data access, and test endpoints.

## Learning objectives

- Create a DynamoDB table with partition (and optional sort) key.
- Configure AWS credentials for local development.
- Build an ASP.NET Core Web API that uses AWS SDK for .NET to perform CRUD operations.
- Understand basic DynamoDB operations (PutItem, GetItem, Query/Scan, UpdateItem, DeleteItem).

## Prerequisites

- .NET 6 or later SDK installed.
- AWS account (free tier recommended) and access to the AWS Console.
- AWS CLI installed and configured (optional but recommended for local credential setup).
- IDE (VS Code / Visual Studio) and Postman or curl for testing.

---

## Lab steps

### 1. Create DynamoDB table

1. Open AWS Console → Services → **DynamoDB** → **Create table**.
2. Table name: `students`.
3. Partition key: `Id` (String). Optionally add a sort key if you plan multi-key access.
4. Keep default settings for now (on-demand billing is fine for lab), then create.

> Note: DynamoDB is schemaless — items may contain different attributes, but keys defined above are required for access.

### 2. Configure AWS credentials (local machine)

Option A — AWS CLI (recommended):

```bash
aws configure
# Enter AWS Access Key ID, Secret Access Key, default region (e.g. us-east-1), output json
```

Credentials will be saved to `~/.aws/credentials` and `~/.aws/config` and picked up by the AWS SDK for .NET automatically.

Option B — Environment variables (temporary for session):

```bash
export AWS_ACCESS_KEY_ID=your_key
export AWS_SECRET_ACCESS_KEY=your_secret
export AWS_REGION=us-east-1
```

### 3. Create ASP.NET Core Web API project

```bash
dotnet new webapi -n StudentDynamoLab
cd StudentDynamoLab
```

### 4. Add AWS SDK packages

Install the AWS SDK packages for DynamoDB and extensions you plan to use.

```bash
dotnet add package AWSSDK.DynamoDBv2
# Optional helpers
dotnet add package Amazon.DynamoDBv2.DataModel
```

### 5. Define the model

Create `Models/Student.cs`:

```csharp
using Amazon.DynamoDBv2.DataModel;

namespace StudentDynamoLab.Models
{
    [DynamoDBTable("students")]
    public class Student
    {
        [DynamoDBHashKey] // Partition key
        public string Id { get; set; }

        [DynamoDBProperty]
        public string Name { get; set; }

        [DynamoDBProperty]
        public string Class { get; set; }

        [DynamoDBProperty]
        public string Country { get; set; }
    }
}
```

### 6. Create a repository/service for DynamoDB access

Create `Services/StudentService.cs` (using the Object Persistence Model):

```csharp
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using StudentDynamoLab.Models;

namespace StudentDynamoLab.Services
{
    public class StudentService
    {
        private readonly DynamoDBContext _context;

        public StudentService(IAmazonDynamoDB client)
        {
            _context = new DynamoDBContext(client);
        }

        public async Task CreateAsync(Student s) => await _context.SaveAsync(s);

        public async Task<Student> GetAsync(string id) => await _context.LoadAsync<Student>(id);

        public async Task<IEnumerable<Student>> GetAllAsync()
        {
            var conditions = new List<ScanCondition>();
            var search = _context.ScanAsync<Student>(conditions);
            return await search.GetRemainingAsync();
        }

        public async Task UpdateAsync(Student s) => await _context.SaveAsync(s);

        public async Task DeleteAsync(string id) => await _context.DeleteAsync<Student>(id);
    }
}
```

### 7. Register AWS client and service in Program.cs

In `Program.cs` (or `Startup.cs` depending on template), add:

```csharp
using Amazon.DynamoDBv2;
using StudentDynamoLab.Services;

var builder = WebApplication.CreateBuilder(args);

// Registers the default IAmazonDynamoDB client which uses local credentials/profile
builder.Services.AddAWSService<IAmazonDynamoDB>();
builder.Services.AddSingleton<StudentService>();

builder.Services.AddControllers();
var app = builder.Build();
app.MapControllers();
app.Run();
```

### 8. Create the Students controller

Create `Controllers/StudentsController.cs`:

```csharp
using Microsoft.AspNetCore.Mvc;
using StudentDynamoLab.Models;
using StudentDynamoLab.Services;

[ApiController]
[Route("api/[controller]")]
public class StudentsController : ControllerBase
{
    private readonly StudentService _svc;
    public StudentsController(StudentService svc) => _svc = svc;

    [HttpPost]
    public async Task<IActionResult> Create(Student s)
    {
        if (string.IsNullOrEmpty(s.Id)) s.Id = Guid.NewGuid().ToString();
        await _svc.CreateAsync(s);
        return CreatedAtAction(nameof(Get), new { id = s.Id }, s);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(string id)
    {
        var student = await _svc.GetAsync(id);
        if (student == null) return NotFound();
        return Ok(student);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var all = await _svc.GetAllAsync();
        return Ok(all);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, Student s)
    {
        s.Id = id;
        await _svc.UpdateAsync(s);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        await _svc.DeleteAsync(id);
        return NoContent();
    }
}
```

### 9. Run and test

```bash
dotnet run
```

- POST `http://localhost:5000/api/students` with JSON body to create a student.
- GET `http://localhost:5000/api/students/{id}` to fetch.
- GET `http://localhost:5000/api/students` to list (Scan operation).
- PUT / DELETE correspondingly.

### 10. Verification & expected behavior

- Creating a student should add an item to the `students` table visible from DynamoDB Console.
- Get should return the item by `Id`.
- Listing uses Scan (costly for large tables) — prefer Query with proper keys in production.

---

## Cleanup

- Delete the `students` table from AWS Console to avoid charges.
- Remove local credentials if temporary.

## Troubleshooting

- `AccessDenied` or authentication errors — verify `~/.aws/credentials` or environment variables.
- `ResourceNotFoundException` — confirm table name matches `[DynamoDBTable("students")]` attribute.
- Performance issues with scans — design appropriate keys and secondary indexes.

---

## Extensions / Next steps

- Add Global Secondary Index (GSI) for querying by non-key attributes (e.g., Class).
- Replace Scan with Query patterns using partition/sort key combinations.
- Add paging and filtering for listing endpoints.
- Integrate with AWS SDK's low-level client for fine-grained control.

---


*End of lab document.*

