using Amazon.Lambda.Core;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace UserCreation;

public class Function
{
    public async Task<LambdaResponse> FunctionHandler(UserInput input, ILambdaContext context)
    {
        // Simulate DB write
        var userId = Guid.NewGuid().ToString();

        return new LambdaResponse($"User created with ID : {userId}", input.Email, userId);
    }
}

public record UserInput(string Email, string Password);

public record LambdaResponse(string Message, string Email, string? UserId = null);