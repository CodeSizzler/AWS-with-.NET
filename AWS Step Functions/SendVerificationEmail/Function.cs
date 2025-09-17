using Amazon.Lambda.Core;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace SendVerificationEmail;

public class Function
{
    public async Task<LambdaResponse> FunctionHandler(UserInput input, ILambdaContext context)
    {
        // Simulate email sending delay
        await Task.Delay(500);

        return new LambdaResponse("Verification email sent");
    }
}

public record UserInput(string Email);

public record LambdaResponse(string Message);