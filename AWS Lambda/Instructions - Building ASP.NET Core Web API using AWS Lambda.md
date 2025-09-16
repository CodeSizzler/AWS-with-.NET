# Building ASP.NET Core Web API using AWS Lambda

### Objective

Deploy an [ASP.NET](https://asp.net/) Core Web API application to AWS Lambda using the serverless architecture pattern.

**Pre-requisites**

*   An active AWS account
*   Visual Studio
*   AWS Toolkit for Visual Studio
*   ASP.NET Core 6 or higher

**Step 1: Create** [**ASP.NET**](https://asp.net/) **Core Web API Project**

1.  Open Visual Studio and select **Create a new project**
2.  Choose [**ASP.NET**](https://asp.net/) **Core Web API** template
3.  Configure your project:
    *   Project name: YourProjectName
    *   Framework: .NET 6.0 or higher
    *   Authentication: None
    *   Configure for HTTPS: Enabled
    *   Enable OpenAPI support: Enabled

## Step 2: Install Required NuGet Package

1.  Right-click on your project in Solution Explorer
2.  Select **Manage NuGet Packages**
3.  Search for and install:
    *   Amazon.Lambda.AspNetCoreServer.Hosting

## Step 3: Configure AWS Lambda Support

1.  Open Program.cs file
2.  Add the following code after
```
var builder = WebApplication.CreateBuilder(args);

// Add AWS Lambda support

builder.Services.AddAWSLambdaHosting(LambdaEventSource.HttpApi);
```

Open your .csproj file and add the ProjectType inside the PropertyGroup section 

Read more [Introducing the .NET 6 runtime for AWS Lambda](https://aws.amazon.com/blogs/compute/introducing-the-net-6-runtime-for-aws-lambda/)

Now, You’re done with the API stuff for this demo.

## Step 4: Create Lambda Function in AWS Console

1.  Sign in to AWS Management Console
2.  Navigate to **Lambda Service**
3.  Click **Create function**
4.  Choose **Author from scratch**
5.  Configure function:
    *   Function name: YourFunctionName
    *   Runtime: **.NET 8** (or your .NET version)
    *   Architecture: x86\_64
    *   Execution role: Create new role with basic Lambda permissions

## Step 5: Deploy Your Application

### Option A: Deploy from Visual Studio (if you have AWS credentials configured)

1.  Right-click project in Solution Explorer
2.  Select **Publish to AWS Lambda**
3.  Follow the deployment wizard

### Option B: Manual Deployment via AWS Console

1.  Build your project in **Release** mode
2.  Locate the published files in bin/Release/net6.0/publish folder
3.  Zip the contents (not the folder itself)
4.  In AWS Lambda console:
    *   Upload the zip file
    *   Set handler: YourAssemblyName::YourAssemblyName.LambdaEntryPoint::FunctionHandlerAsync
    *   Save and test

Add a Function name, select Runtime - .NET 8 and other configurations needed.

## Important Notes

*   Ensure your IAM role has proper permissions for Lambda execution
*   The AWSProjectType tag helps Visual Studio recognize it as a Lambda project
*   Handler name must match your assembly name
*   For production, set up proper logging and monitoring using AWS CloudWatch

# Conclusion

This lab successfully demonstrated the process of deploying an [ASP.NET](https://asp.net/) Core Web API application to AWS Lambda, showcasing the powerful combination of modern .NET development with serverless cloud architecture. Through this implementation, we achieved a fully functional, scalable web API that operates without traditional server management overhead.

The integration of [ASP.NET](https://asp.net/) Core with AWS Lambda proves to be an effective solution for building cost-efficient, highly available APIs that automatically scale with demand. By leveraging the Amazon.Lambda.AspNetCoreServer.Hosting package and proper configuration, we maintained the familiar [ASP.NET](https://asp.net/) Core development experience while gaining the benefits of serverless execution.

Key successes include:

*   Seamless transition from traditional hosting to serverless architecture
*   Proper configuration of Lambda-specific project settings
*   Successful deployment and testing of API endpoints
*   Demonstration of AWS's pay-per-use pricing model for API solutions

This approach is particularly valuable for applications with variable traffic patterns, microservices architectures, and projects requiring rapid deployment with minimal operational overhead. The lab reinforces that AWS Lambda provides an excellent platform for running .NET applications while abstracting infrastructure management concerns, allowing developers to focus on business logic rather than server maintenance.

The skills developed in this lab are directly applicable to real-world scenarios where organizations are migrating to serverless architectures to improve scalability, reduce costs, and increase deployment agility.
