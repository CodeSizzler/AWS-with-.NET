using Amazon.Lambda.Core;
using System.Text.RegularExpressions;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace ValidateUserInput;
public class Function
{
    public async Task<LambdaResponse> FunctionHandler(UserInput input, ILambdaContext context)
    {
        if (string.IsNullOrWhiteSpace(input.Email) || string.IsNullOrWhiteSpace(input.Password))
            throw new Exception("Email and Password are required.");

        if (!IsValidEmail(input.Email))
            throw new Exception("Invalid email format.");

        if (input.Password.Length < 6)
            throw new Exception("Password must be at least 6 characters.");

        return new LambdaResponse("Validation passed", input.Email, input.Password);
    }

    private bool IsValidEmail(string email)
    {
        return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
    }
}

public record UserInput(string Email, string Password);

public record LambdaResponse(string Message, string Email, string Password);
