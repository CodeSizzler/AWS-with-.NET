using Amazon.Lambda.Core;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace NotifyFailure;

public class Function
{
    public async Task<LambdaResponse> FunctionHandler(FailureInput input, ILambdaContext context)
    {
        // Optionally: send alert to admin, push to SNS, etc.
        await Task.CompletedTask;

        return new LambdaResponse("Failure logged");
    }
}

public record FailureInput(string Error, string Cause);

public record LambdaResponse(string Message);