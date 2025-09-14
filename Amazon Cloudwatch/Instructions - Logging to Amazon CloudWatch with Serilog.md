# Logging to Amazon CloudWatch with Serilog --- Lab-driven step‑by‑step

## Quick overview

Goal: configure a .NET 6 Web API to send structured logs to Amazon
CloudWatch using Serilog and the AWS Serilog sink.

What this doc contains (high level):

1.  Prerequisites and AWS IAM minimum permissions
2.  Create the sample Web API project
3.  Install required NuGet packages (both `dotnet` CLI and Package
    Manager Console commands)
4.  Program.cs Serilog wiring (minimal hosting model)
5.  `appsettings.json` Serilog configuration for CloudWatch
6.  Example controller to generate logs
7.  AWS setup (aws configure) and how to verify logs in CloudWatch
    Console
8.  Cost-saving option: restrict only `Error` level to CloudWatch
9.  Troubleshooting (NU1605 package-downgrade, `NoWarn` workaround, how
    to check package graph)
10. Useful commands & references

------------------------------------------------------------------------

## 1) Prerequisites

-   .NET 6 SDK installed (the lab uses .NET 6).
-   An AWS account and credentials (Access Key ID / Secret Access Key)
    with permissions to create and write to CloudWatch Logs.
-   AWS CLI installed and configured (`aws configure`).
-   Visual Studio 2022 or VS Code (optional --- any editor will do).

------------------------------------------------------------------------

## 2) Create the project

``` bash
# create a new Web API project
dotnet new webapi -n AWSCloudwatch.Serilog
cd AWSCloudwatch.Serilog
```

Or in Visual Studio, create a new **ASP.NET Core Web API** project
targeting .NET 6.

------------------------------------------------------------------------

## 3) Install required NuGet packages

**Package Manager Console** (Visual Studio):

``` powershell
Install-Package AWS.Logger.SeriLog
Install-Package Serilog.AspNetCore
Install-Package Serilog.Settings.Configuration
Install-Package Serilog.Sinks.Console
```

**dotnet CLI equivalent:**

``` bash
dotnet add package AWS.Logger.SeriLog
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Settings.Configuration
dotnet add package Serilog.Sinks.Console
```

> NOTE: If you hit package downgrade errors (NU1605) like
> `Detected package downgrade: Serilog.Formatting.Compact from 3.0.0 to 1.1.0`,
> see the **Troubleshooting** section below.

------------------------------------------------------------------------

## 4) Wire Serilog into `Program.cs` (minimal hosting)

Open `Program.cs` and add the `UseSerilog` call early in the host build
pipeline. Example (place after `builder.Services.AddSwaggerGen()` or
similar):

``` csharp
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ... existing builder configuration

// Attach Serilog as the logging provider
builder.Host.UseSerilog((_, loggerConfig) =>
{
    loggerConfig
        .WriteTo.Console()
        .ReadFrom.Configuration(builder.Configuration);
});

var app = builder.Build();
// ... rest of the pipeline
```

This makes Serilog the application's logging provider and tells it to
read sinks & settings from `appsettings.json`.

------------------------------------------------------------------------

## 5) Add Serilog section to `appsettings.json`

Add a `Serilog` section that tells the AWS sink which Log Group and
Region to use. The lab example uses `/log/demo` as the log group name.

``` json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft": "Warning"
    }
  },
  "AllowedHosts": "*",

  "Serilog": {
    "LogGroup": "/log/demo",
    "Region": "ap-south-1",
    "MinimumLevel": {
      "Default": "Debug",
      "Override": {
        "Microsoft": "Error",
        "System": "Error",
        "Microsoft.Hosting.Lifetime": "Information"
      }
    },
    "WriteTo": [
      {
        "Name": "AWSSeriLog",
        "Args": {
          "textFormatter": "Serilog.Formatting.Json.JsonFormatter, Serilog"
        }
      }
    ]
  }
}
```

Notes: - `LogGroup` is the CloudWatch Log Group name (the sink will
create it if needed when the IAM credentials allow it). - `Region` is
optional; if omitted, the AWS SDK will use the region from your
configured profile.

------------------------------------------------------------------------

## 6) Add a test controller

Add a simple controller to generate some logs. Example:
`Controllers/HelloController.cs`:

``` csharp
using Microsoft.AspNetCore.Mvc;

namespace AWSCloudwatch.Serilog.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HelloController : ControllerBase
    {
        private readonly ILogger<HelloController> _logger;

        public HelloController(ILogger<HelloController> logger)
        {
            _logger = logger;
        }

        [HttpGet("{id:int}")]
        public IActionResult GetAsync(int id)
        {
            _logger.LogDebug("Received Request with id as {id}", id);
            _logger.LogInformation("Processing your request");
            _logger.LogError("Some Errors occured.");
            return Ok("Logged");
        }
    }
}
```

------------------------------------------------------------------------

## 7) Configure AWS CLI / credentials

If you haven't already, run:

``` bash
aws configure
```

Provide your **AWS Access Key ID**, **Secret Access Key**, default
region (e.g., `ap-south-1`), and default output format. Ensure the IAM
user/role has CloudWatch Logs permissions (see sample policy below).

### Minimal IAM policy required (example)

This example policy grants the actions typically required for a Serilog
sink to create log groups/streams and put log events:

``` json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Action": [
        "logs:CreateLogGroup",
        "logs:CreateLogStream",
        "logs:PutLogEvents",
        "logs:DescribeLogStreams"
      ],
      "Resource": "arn:aws:logs:*:*:log-group:/log/demo:*"
    }
  ]
}
```

Adjust the `Resource` ARN to your log group and region as needed.

------------------------------------------------------------------------

## 8) Run & verify

1.  `dotnet run` (or run from Visual Studio).
2.  Invoke your test endpoint (example):
    `GET https://localhost:5001/api/hello/1`.
3.  On success, the app will write logs.
4.  In AWS Console \> CloudWatch \> Logs \> Log groups, look for
    `/log/demo` (or the `LogGroup` you set). Open the newest log stream
    to inspect events.

------------------------------------------------------------------------

## 9) Cost-saving option: send only `Error` to CloudWatch

If you want to log everything locally (Console) but only send `Error`
messages to CloudWatch, modify the sink args:

``` json
"WriteTo": [
  {
    "Name": "AWSSeriLog",
    "Args": {
      "textFormatter": "Serilog.Formatting.Json.JsonFormatter, Serilog",
      "restrictedToMinimumLevel": "Error"
    }
  }
]
```

This reduces CloudWatch storage & ingestion costs by sending only
higher‑severity events.

------------------------------------------------------------------------

## 10) Troubleshooting --- common issues and fixes

### A) NU1605: Detected package downgrade (the error you posted)

**Problem:** NuGet detects that two dependency paths require different
versions of the same package (one requires a newer version, another
references an older one), and your restore treats warnings as errors.

**Best fix:** align package versions. Steps:

1.  Inspect current packages and dependency graph:

``` bash
# dotnet CLI
dotnet list package

# or Package Manager Console
Get-Package -ProjectName AWSCloudwatch.Serilog
```

2.  Explicitly install the newer required packages so NuGet resolves to
    the higher version. Example (Package Manager Console):

``` powershell
Install-Package Serilog.Formatting.Compact -Version 3.0.0
Install-Package Serilog.Settings.Configuration -Version 9.0.0
Install-Package Serilog.Sinks.Console -Version 6.0.0
```

or dotnet CLI:

``` bash
dotnet add package Serilog.Formatting.Compact --version 3.0.0
dotnet add package Serilog.Settings.Configuration --version 9.0.0
dotnet add package Serilog.Sinks.Console --version 6.0.0
```

3.  Restore and rebuild:

``` bash
dotnet restore
dotnet build
```

**Temporary/workaround (not recommended long-term):** suppress the
specific NU1605 warning by editing your `.csproj` and adding:

``` xml
<PropertyGroup>
  <NoWarn>$(NoWarn);NU1605</NoWarn>
</PropertyGroup>
```

This hides the warning but does not resolve the root cause --- prefer
aligning package versions.

### B) Credentials / permissions errors

-   Make sure `aws configure` credentials correspond to an IAM principal
    with CloudWatch Logs permissions.
-   Check CloudWatch Console for `AccessDenied` style messages; update
    IAM policy accordingly.

### C) Logs not appearing

-   Confirm the application used the correct profile/region (check
    `Region` in `appsettings.json` or AWS CLI default region).
-   Use CloudWatch Console to search for the configured `LogGroup` name.
-   Check for network issues or SDK errors in the app output.

------------------------------------------------------------------------

## Useful commands quick list

-   `dotnet new webapi -n AWSCloudwatch.Serilog`
-   `dotnet add package AWS.Logger.SeriLog`
-   `dotnet add package Serilog.AspNetCore`
-   `dotnet list package`
-   `dotnet run`
-   `aws configure`

------------------------------------------------------------------------

*End of playbook --- happy logging!*
