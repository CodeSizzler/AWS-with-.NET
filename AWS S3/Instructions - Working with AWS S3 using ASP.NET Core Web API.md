# Working with AWS S3 using ASP.NET Core Web API

**Objective:**
This guide will walk you through creating an ASP.NET Core Web API to interact with Amazon S3 (Simple Storage Service). You will learn how to:
*   Create an AWS IAM user with appropriate permissions.
*   Generate secure access keys for your application.
*   Create and manage S3 buckets via the AWS Console.
*   Build a .NET 6 API to upload, download, and delete files from S3.

---

## Part 1: Setting Up AWS Security (IAM)

To allow your .NET application to access AWS services, you must create a dedicated user with limited permissions.

### Step 1: Create a New IAM User
1.  Log in to the **[AWS Management Console](https://aws.amazon.com/console/)**.
2.  In the search bar at the top, type **`IAM`** and select it from the results.
3.  In the IAM dashboard, navigate to **Access Management > Users**.
4.  Click the **`Create user`** button.

### Step 2: Set User Details
1.  On the "Specify user details" page, enter a **User name** (e.g., `aspnet-s3-demo-user`).
2.  Under "Select AWS credential type," check **`Access key - Programmatic access`**. This ensures the user gets an Access Key ID and Secret Access Key for API calls.
3.  Click **`Next: Permissions`**.

### Step 3: Attach Permissions Policies
1.  On the "Set permissions" page, select **`Attach policies directly`**.
2.  In the policy search bar, type **`S3`**.
3.  Find and select the **`AmazonS3FullAccess`** policy. This grants the user full control over S3 operations.
4.  Click **`Next: Tags`**. (Adding tags is optional for this demo; you can skip this step).
5.  Click **`Next: Review`**.

### Step 4: Review and Create User
1.  Review the user details and assigned policy.
2.  Click **`Create user`**.

### Step 5: Save Your Credentials
1.  **This is a critical step.** On the success screen, you will see your **Access key ID** and **Secret access key**.
2.  Click the **`Download .csv`** button to securely download these credentials.
3.  **Store this file safely.** The Secret access key cannot be retrieved again later. You will need these values to configure your .NET application.

---

## Part 2: Creating an S3 Bucket via AWS Console

### Step 1: Navigate to S3
1.  Back in the AWS Console, search for and select **`S3`**.

### Step 2: Create a Bucket
1.  Click the **`Create bucket`** button.
2.  **Bucket name:** Choose a name that is **globally unique** across all of AWS.
3.  **AWS Region:** Select the geographic region closest to you or your users for optimal performance.
4.  **Block Public Access:** Keep the default setting, **"Block all public access,"** enabled. Our application will generate temporary, pre-signed URLs for secure access to private files.
5.  Leave all other settings at their defaults.
6.  Click **`Create bucket`** at the bottom of the page.

Your new bucket will now appear in your S3 bucket list.

---

## Part 3: Building the ASP.NET Core Web API

### Step 1: Create a New Project
1.  Open Visual Studio (2022 or later).
2.  Click **`Create a new project`**.
3.  Select the **`ASP.NET Core Web API`** template and click **Next**.
4.  Configure your project:
    *   **Project name:** `AwsS3Demo`
    *   **Framework:** **`.NET 6.0 (Long-term support)`**
    *   Ensure **`Configure for HTTPS`** is checked.
    *   Ensure **`Use controllers`** is checked.
    *   Ensure **`Enable OpenAPI support`** is checked (this enables Swagger).
5.  Click **`Create`**.

### Step 2: Install Required NuGet Packages
Install the following packages via the NuGet Package Manager or the Package Manager Console:

```bash
Install-Package AWSSDK.S3
Install-Package AWSSDK.Extensions.NETCore.Setup
```

### Step 3: Configure AWS Credentials
Open the `appsettings.json` file. **Never commit real credentials to source control.** For development, you can add them to this file. Replace the entire content with the following configuration, making sure to use your own values:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "AWS": {
    "Profile": "default",
    "Region": "us-east-1", // Change this to your bucket's region (e.g., eu-west-1, ap-south-1)
    "AccessKey": "YOUR_ACCESS_KEY_ID_HERE",
    "SecretKey": "YOUR_SECRET_ACCESS_KEY_HERE"
  }
}
```

* Replace YOUR_ACCESS_KEY_ID_HERE and YOUR_SECRET_ACCESS_KEY_HERE with the credentials you saved from Part 1.
* Replace the Region value with the one you selected when creating your bucket.

### Step 4: Register the AWS S3 Service
Open the `Program.cs` file and add the following code:

```csharp
using Amazon.S3;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register the AWS S3 service
builder.Services.AddAWSService<IAmazonS3>(); // Reads from the "AWS" section in appsettings.json

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
```
### Step 5: Create an S3 Service Controller
Create a new file `S3StorageController.cs` in the `Controllers` folder.

```csharp
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace AwsS3Demo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class S3StorageController : ControllerBase
    {
        private readonly IAmazonS3 _s3Client;

        public S3StorageController(IAmazonS3 s3Client)
        {
            _s3Client = s3Client;
        }

        // POST: api/S3Storage/create-bucket/{bucketName}
        [HttpPost("create-bucket/{bucketName}")]
        public async Task<IActionResult> CreateBucket(string bucketName)
        {
            var bucketExists = await _s3Client.DoesS3BucketExistAsync(bucketName);
            if (bucketExists)
            {
                return BadRequest($"Bucket {bucketName} already exists.");
            }

            await _s3Client.PutBucketAsync(bucketName);
            return Ok($"Bucket {bucketName} created successfully.");
        }

        // GET: api/S3Storage/list-buckets
        [HttpGet("list-buckets")]
        public async Task<IActionResult> ListBuckets()
        {
            var data = await _s3Client.ListBucketsAsync();
            var bucketNames = data.Buckets.Select(b => b.BucketName);
            return Ok(bucketNames);
        }

        // POST: api/S3Storage/upload-file/{bucketName}
        [HttpPost("upload-file/{bucketName}")]
        public async Task<IActionResult> UploadFile(string bucketName, IFormFile file)
        {
            var bucketExists = await _s3Client.DoesS3BucketExistAsync(bucketName);
            if (!bucketExists)
            {
                return NotFound($"Bucket {bucketName} does not exist.");
            }

            var request = new PutObjectRequest()
            {
                BucketName = bucketName,
                Key = file.FileName,
                InputStream = file.OpenReadStream(),
                ContentType = file.ContentType,
                AutoCloseStream = true
            };

            var response = await _s3Client.PutObjectAsync(request);

            if (response.HttpStatusCode == HttpStatusCode.OK)
            {
                return Ok($"File {file.FileName} uploaded to S3 successfully!");
            }
            else
            {
                return StatusCode((int)response.HttpStatusCode, "Upload failed.");
            }
        }

        // GET: api/S3Storage/get-file/{bucketName}/{key}
        [HttpGet("get-file/{bucketName}/{key}")]
        public async Task<IActionResult> GetFileUrl(string bucketName, string key)
        {
            var bucketExists = await _s3Client.DoesS3BucketExistAsync(bucketName);
            if (!bucketExists)
            {
                return NotFound($"Bucket {bucketName} does not exist.");
            }

            var url = _s3Client.GetPreSignedURL(new GetPreSignedUrlRequest()
            {
                BucketName = bucketName,
                Key = key,
                Expires = DateTime.UtcNow.AddMinutes(60)
            });

            return Ok(new { PreSignedUrl = url });
        }

        // DELETE: api/S3Storage/delete-file/{bucketName}/{key}
        [HttpDelete("delete-file/{bucketName}/{key}")]
        public async Task<IActionResult> DeleteFile(string bucketName, string key)
        {
            var bucketExists = await _s3Client.DoesS3BucketExistAsync(bucketName);
            if (!bucketExists)
            {
                return NotFound($"Bucket {bucketName} does not exist.");
            }

            await _s3Client.DeleteObjectAsync(bucketName, key);
            return NoContent();
        }
    }
}
```

### Part 4: Testing the API
Run your application (F5 in Visual Studio). The Swagger page will open in your browser.

Use the Swagger UI to test the endpoints:

POST /api/S3Storage/create-bucket/{bucketName}: Create a new bucket.

GET /api/S3Storage/list-buckets: List all your buckets.

## Conclusion

This lab successfully demonstrated how to integrate Amazon S3 with an ASP.NET Core Web API. We established secure programmatic access via AWS IAM, created an S3 bucket, and built a functional API to manage buckets and objects.

The key takeaway is the ability to perform core operations—create, list, upload, and delete—securely from .NET code. Crucially, we implemented secure file sharing using **pre-signed URLs**, providing temporary access without compromising bucket security.

This provides a solid foundation for building scalable, cloud-native applications that leverage AWS's robust storage infrastructure.
