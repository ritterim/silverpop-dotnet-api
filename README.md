# Silverpop .NET API

This is a .NET API wrapper for [Silverpop](http://www.silverpop.com/) Transact XML email sending.

## Usage

**Prepare the client *(fill in the necessary details)***

```csharp
var configuration = new TransactClientConfiguration()
{
    PodNumber = your_pod_number,
    Username = "username",
    Password = "password"
};

var client = new TransactClient(configuration);
```

**Create a message**

```csharp
var message = new TransactMessage()
{
    CampaignId = "123456",
    TransactionId = "Test-" + Guid.NewGuid().ToString()),
    Recipients = new List<TransactMessageRecipient>()
    {
        new TransactMessageRecipient()
        {
            BodyType = TransactMessageRecipientBodyType.Html,
            EmailAddress = "user@example.com",
            PersonalizationTags = new Dictionary<string, string>()
            {
                { "tag1", "tag1-value" }
            }
        }
    }
};
```

**Send a message using the client *(1-10 recipients)***

```csharp
TransactMessageResponse response = client.SendMessage(message);
```

**Send a message using the client asynchronously *(1-10 recipients)***

```csharp
TransactMessageResponse response = await client.SendMessageAsync(message);
```

**Send a message batch using the client *(11 - 5,000 recipients)***

```csharp
IEnumerable<string> batchResponse = client.SendMessageBatch(message);
```

**Send a message batch using the client asynchronously *(11 - 5,000 recipients)***

```csharp
IEnumerable<string> batchResponse = await client.SendMessageBatchAsync(message);
```

**Get the status of a message batch**

```csharp
TransactMessageResponse response = client.GetStatusOfMessageBatch("a_filename_from_batchResponse.xml");
```

**Get the status of a message batch asynchronously**

```csharp
TransactMessageResponse response = await client.GetStatusOfMessageBatchAsync("a_filename_from_batchResponse.xml");
```

# License

MIT License
