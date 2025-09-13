# Step-by-step guide — Access AWS services from an ASP.NET Core app using Amazon Cognito

This guide is based on the AWS Prescriptive Guidance pattern and official AWS sample repo. It shows how to configure **Cognito User Pools + Identity Pools**, integrate with an **ASP.NET Core app**, exchange a user token for temporary AWS credentials, and call AWS services (example: S3).

---

## Prerequisites
- AWS account with permissions for Cognito, IAM, and S3.
- ASP.NET Core 2.0.0 or later.
- Visual Studio or VS Code, with `dotnet` SDK installed.
- Git (for cloning the sample repo).

---

## High-level flow
1. Create a **Cognito User Pool**.
2. Add an **App client** to the user pool.
3. Create a **Cognito Identity Pool**.
4. Assign IAM roles to the identity pool.
5. Clone AWS sample app and configure it.
6. Exchange Cognito User Pool token for AWS credentials.
7. Call AWS services (S3 example).

---

## Step 1 — Create a Cognito User Pool
1. Open AWS Console → Cognito → **Manage user pools** → **Create a user pool**.
2. Configure attributes, password policy, MFA as required.
3. Note the **User Pool ID**.

---

## Step 2 — Add an App Client
1. In User Pool → **App clients** → **Add an app client**.
2. Note the **App client ID** and **App client secret**.

---

## Step 3 — Create a Cognito Identity Pool
1. Go to **Manage identity pools** → **Create new identity pool**.
2. Name it, enable unauthenticated identities if desired.
3. In **Authentication providers**, provide User Pool ID and App client ID.
4. Note the **Identity Pool ID**.

---

## Step 4 — Configure IAM Roles
Example IAM policy snippet for authenticated role:

```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Action": [
        "mobileanalytics:PutEvents",
        "cognito-sync:*",
        "cognito-identity:*",
        "s3:ListAllMyBuckets*"
      ],
      "Resource": ["*"]
    }
  ]
}
```

---

## Step 5 — Clone AWS ASP.NET Cognito Sample
```bash
git clone https://github.com/aws/aws-aspnet-cognito-identity-provider.git
cd aws-aspnet-cognito-identity-provider/samples
```

---

## Step 6 — Add NuGet Packages
```bash
dotnet add package Amazon.AspNetCore.Identity.Cognito
dotnet add package Amazon.Extensions.CognitoAuthentication
dotnet add package AWSSDK.CognitoIdentity
dotnet add package AWSSDK.CognitoIdentityProvider
dotnet add package AWSSDK.Extensions.NETCore.Setup
dotnet add package AWSSDK.S3
```

---

## Step 7 — Configure `appsettings.json`
```json
"AWS": {
  "Region": "<region>",
  "UserPoolClientId": "<client id>",
  "UserPoolClientSecret": "<client secret>",
  "UserPoolId": "<user pool id>"
}
```

---

## Step 8 — Register Cognito in `Startup`/`Program`
```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddCognitoIdentity();
    services.AddDefaultAWSOptions(Configuration.GetAWSOptions());
    services.AddAWSService<IAmazonS3>();
    services.AddControllersWithViews();
}
```

```csharp
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseRouting();
    app.UseEndpoints(endpoints => endpoints.MapDefaultControllerRoute());
}
```

---

## Step 9 — Add Razor Page for S3 Buckets
Add a Razor page (`MyS3Buckets`) that lists S3 buckets once the user is authenticated.

---

## Step 10 — Exchange Token for AWS Credentials
```csharp
using Amazon;
using Amazon.CognitoIdentity;
using Amazon.CognitoIdentity.Model;
using Amazon.Runtime;
using Amazon.S3;

public async Task UseTemporaryCredentialsAsync(string idToken, string identityPoolId, string userPoolId, string region)
{
    var regionEndpoint = RegionEndpoint.GetBySystemName(region);
    var cognitoIdentityClient = new AmazonCognitoIdentityClient(regionEndpoint);

    var loginKey = $"cognito-idp.{region}.amazonaws.com/{userPoolId}";

    var getIdResp = await cognitoIdentityClient.GetIdAsync(new GetIdRequest
    {
        IdentityPoolId = identityPoolId,
        Logins = new Dictionary<string, string> { { loginKey, idToken } }
    });

    var getCredsResp = await cognitoIdentityClient.GetCredentialsForIdentityAsync(new GetCredentialsForIdentityRequest
    {
        IdentityId = getIdResp.IdentityId,
        Logins = new Dictionary<string, string> { { loginKey, idToken } }
    });

    var creds = getCredsResp.Credentials;
    var sessionCreds = new SessionAWSCredentials(creds.AccessKeyId, creds.SecretKey, creds.SessionToken);
    using var s3 = new AmazonS3Client(sessionCreds, regionEndpoint);

    var listResp = await s3.ListBucketsAsync();
    foreach (var b in listResp.Buckets) Console.WriteLine(b.BucketName);
}
```

---

## Step 11 — Run & Test
1. Fill in `appsettings.json` with correct values.
2. Run the app (`dotnet run`).
3. Create a test user in the Cognito User Pool.
4. Sign in and navigate to the S3 page.

---

## Troubleshooting
- Remove project references if NuGet package conflicts occur.
- Check that `IdentityPoolId` and region match.
- Ensure IAM roles have correct permissions.
- Handle token and credential expiration.

---

## Security Recommendations
- Do not hardcode secrets.
- Use least privilege for IAM roles.
- Avoid logging tokens.

---


