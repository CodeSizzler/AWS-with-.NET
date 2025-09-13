# Lab: Using Amazon SQS in a .NET Application


## Overview

In this lab you'll learn how to integrate **Amazon Simple Queue Service (SQS)** into a .NET application. Specifically, you will:

- Create and configure an SQS queue in AWS
- Send messages to that queue
- Receive and process messages from the queue
- Delete messages after processing
- Handle visibility timeouts and dead-letter queue (DLQ) configurations
- Use AWS SDK for .NET to programmatically work with SQS

---

## Prerequisites

- AWS Account with permissions for SQS
- .NET 6 or later installed
- IDE (Visual Studio / VS Code)
- AWS CLI installed and configured (or valid AWS credentials via environment variables or SDK profile)
- Basic knowledge of async/await in C# and .NET

---

## Lab Steps

### 1. Create an SQS Queue in AWS

1. Sign in to the AWS Console → **SQS** service.
2. Create a new queue. Choose **Standard** unless you need FIFO semantics.
3. Set queue name, e.g., `MyDotNetQueue`.
4. Configure other settings:
   - Visibility timeout (e.g., 30 seconds)
   - Message retention period (e.g., 4 days)
   - Delivery delay (optional)
5. (Optional) Create a Dead-Letter Queue (DLQ) and configure redrive policy for the main queue.

### 2. Setup Local Environment / Credentials

- Using AWS CLI:

  ```bash
  aws configure
  ```

- Or via environment variables:

  ```bash
  export AWS_ACCESS_KEY_ID=YOUR_KEY
  export AWS_SECRET_ACCESS_KEY=YOUR_SECRET
  export AWS_REGION=us-east-1  # or your chosen region
  ```

### 3. Create a .NET Project

```bash
dotnet new console -n SqsDemo
cd SqsDemo
```

Or if you prefer, a Web API or Worker Service project can be used.

### 4. Install AWS SDK for SQS

```bash
dotnet add package AWSSDK.SQS
```

### 5. Write Code to Send Messages

In `Program.cs` (or dedicated service class):

```csharp
using System;
using System.Threading.Tasks;
using Amazon.SQS;
using Amazon.SQS.Model;

namespace SqsDemo
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var sqsClient = new AmazonSQSClient(); // uses default credentials/profile

            var queueUrlResponse = await sqsClient.GetQueueUrlAsync("MyDotNetQueue");
            var queueUrl = queueUrlResponse.QueueUrl;

            var sendRequest = new SendMessageRequest
            {
                QueueUrl = queueUrl,
                MessageBody = "Hello from .NET at " + DateTime.UtcNow
            };

            var sendResponse = await sqsClient.SendMessageAsync(sendRequest);
            Console.WriteLine($"Message sent. MessageId: {sendResponse.MessageId}");
        }
    }
}
```

### 6. Write Code to Receive and Process Messages

Still in the same or another class / project:

```csharp
using System;
using System.Threading.Tasks;
using Amazon.SQS;
using Amazon.SQS.Model;

namespace SqsDemo
{
    class Receiver
    {
        private readonly IAmazonSQS _sqsClient;
        private readonly string _queueUrl;

        public Receiver(IAmazonSQS sqsClient, string queueUrl)
        {
            _sqsClient = sqsClient;
            _queueUrl = queueUrl;
        }

        public async Task ReceiveAndDeleteAsync()
        {
            var receiveRequest = new ReceiveMessageRequest
            {
                QueueUrl = _queueUrl,
                MaxNumberOfMessages = 5,
                WaitTimeSeconds = 10
            };

            var response = await _sqsClient.ReceiveMessageAsync(receiveRequest);

            foreach (var msg in response.Messages)
            {
                Console.WriteLine($"Received message: {msg.Body}");
                // Process message here

                // Delete after processing
                await _sqsClient.DeleteMessageAsync(new DeleteMessageRequest
                {
                    QueueUrl = _queueUrl,
                    ReceiptHandle = msg.ReceiptHandle
                });
                Console.WriteLine($"Deleted message {msg.MessageId}");
            }
        }
    }
}
```

### 7. Handling Visibility Timeout & DLQ

- Ensure the visibility timeout is long enough to process the message.
- If processing fails repeatedly, after a configured number of attempts messages are moved to the DLQ.
- Optionally, simulate error to see DLQ behaviour (throw exception before delete, or fail to process).

### 8. Optional: Continuous Polling / Worker Pattern

Implement a loop or background service:

```csharp
while(true)
{
    await receiver.ReceiveAndDeleteAsync();
    await Task.Delay(TimeSpan.FromSeconds(5));
}
```

Be careful with infinite loops and resource usage.

### 9. Testing

- Send several messages and verify they are received correctly.
- Test with failing processing to ensure message reappear after visibility timeout.
- Confirm messages that exceed max receive attempts go to DLQ (if configured).

---

## Expected Outcome

- Messages should be successfully sent to the SQS queue.
- Receiver code should fetch messages, process them, and delete them.
- Visibility timeout should prevent other consumers from seeing the message during processing.
- Messages that fail processing multiple times should land in DLQ (if configured).

---

## Cleanup

- Delete the SQS queue(s) you created (main + DLQ).
- Clean up any credentials if using temporary env variables.
- Remove .NET project folder if not needed further.

---

## Extensions / Further Work

- Use a FIFO queue (with message group IDs & deduplication).
- Batch operations (`SendMessageBatch`, `DeleteMessageBatch`).
- Integrate the queue into a microservices architecture (one service producing, another consuming).
- Add monitoring/metrics: CloudWatch for SQS (queue length, age, etc.).
- Add retries, exponential backoff, handling poison messages.

---

## Troubleshooting

| Problem | Likely Cause | Solution |
|---|---|---|
| `QueueDoesNotExist` error | Wrong queue URL or region mismatch | Ensure queue name + region match and you fetched the correct URL |
| Messages not being deleted (reappear) | Visibility timeout expired or delete request failed | Check ReceiptHandle, ensure delete is called after processing |
| DLQ not receiving messages | Redrive policy misconfigured or max receive count too high | Configure redrive policy correctly; set maxReceiveCount |
| Permission errors | IAM policy lacks SQS permissions | Add policy for `sqs:SendMessage`, `sqs:ReceiveMessage`, `sqs:DeleteMessage` etc. |


---

*End of lab*
