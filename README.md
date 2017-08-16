# Silverpop .NET API

This is a .NET API wrapper for [Silverpop](http://www.silverpop.com/) Transact XML email sending.

[![NuGet Version](https://img.shields.io/nuget/v/silverpop-dotnet-api.svg)](https://www.nuget.org/packages/silverpop-dotnet-api)

## Installation

The recommended installation method is the [silverpop-dotnet-api](https://www.nuget.org/packages/silverpop-dotnet-api) NuGet package.

```
PM> Install-Package silverpop-dotnet-api
```

## Requirements

- .NET Framework 4.6.1 / `netstandard2.0`

## Usage

**Create the client**

```csharp
// Create the TransactClient with standard authentication only.
// Note: This does not enable OAuth scenarios.
// OAuth is typically used when an application is hosted
// somewhere with a non-static IP address (Azure Websites, etc.),
// or when you don't want to specify IP address(es) with Silverpop.
var client = TransactClient.Create(YourPodNumberInteger, YourUsername, YourPassword);

// ------------------------- OR -------------------------

// Create the TransactClient enabling all features.
// OAuth will be used for non-batch message scenarios.
var client = TransactClient.CreateIncludingOAuth(
    YourPodNumberInteger,
    YourUsername,
    YourPassword,
    YourOAuthClientId,
    YourOAuthClientSecret,
    YourOAuthRefreshToken);

// ------------------------- OR -------------------------

// Create the TransactClient with OAuth authentication only.
// Note: This cannot be used with batch sending.
var client = TransactClient.CreateOAuthOnly(
    YourPodNumberInteger,
    YourOAuthClientId,
    YourOAuthClientSecret,
    YourOAuthRefreshToken);

// ------------------------- OR -------------------------

var client = TransactClient.CreateUsingConfiguration();
```

When using `TransactClient.CreateUsingConfiguration()` the configuration can be defined using **TransactClientConfigurationSection** or **appSettings**. Both can be used, too -- they'll be combined, with **appSettings** overwriting any settings defined in **TransactClientConfigurationSection**.

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <configSections>
    <section name="transactClientConfiguration" type="Silverpop.Client.TransactClientConfigurationSection, Silverpop.Client" />
  </configSections>
  <!-- Configure:
    - podNumber
    - username/password and/or oAuthClientId/oAuthClientSecret/oAuthRefreshToken
      - If both are configured, OAuth is used.
  -->
  <transactClientConfiguration
    podNumber="0"
    username=""
    password=""
    oAuthClientId=""
    oAuthClientSecret=""
    oAuthRefreshToken="" />
  <appSettings>
    <add key="silverpop-dotnet-api:PodNumber" value="" />
    <add key="silverpop-dotnet-api:Username" value="" />
    <add key="silverpop-dotnet-api:Password" value="" />
    <add key="silverpop-dotnet-api:OAuthClientId" value="" />
    <add key="silverpop-dotnet-api:OAuthClientSecret" value="" />
    <add key="silverpop-dotnet-api:OAuthRefreshToken" value="" />
  </appSettings>
</configuration>
```

**Create a simple message**

```csharp
var message = TransactMessage.Create("123456", TransactMessageRecipient.Create("user@example.com");
```

**Send a message using the client *(1-10 recipients)***

```csharp
TransactMessageResponse response = client.SendMessage(message);
```

**Send a message using the client asynchronously *(1-10 recipients)***

```csharp
TransactMessageResponse response = await client.SendMessageAsync(message);
```

**Send a message batch using the client *(1 or more recipients -- intended for bulk usage rather than sending an email to a single user)***

```csharp
// This may send multiple XML files to Silverpop depending on the number of recipients.
IEnumerable<string> batchResponse = client.SendMessageBatch(message);
```

**Send a message batch using the client asynchronously *(1 or more recipients -- intended for bulk usage rather than sending an email to a single user)***

```csharp
// This may send multiple XML files to Silverpop depending on the number of recipients.
IEnumerable<string> batchResponse = await client.SendMessageBatchAsync(message);
```

**Get the status of a message batch**

```csharp
// This checks the status for the first batch only from one of the above calls
// (there may be more than one XML file sent to Silverpop).
TransactMessageResponse response = client.GetStatusOfMessageBatch(batchResponse[0]);
```

**Get the status of a message batch asynchronously**

```csharp
// This checks the status for the first batch only from one of the above calls
// (there may be more than one XML file sent to Silverpop).
TransactMessageResponse response = await client.GetStatusOfMessageBatchAsync(batchResponse[0]);
```

## Messages with personalization tags

**First, we recommend creating a class as a model for the personalization tags.**

```csharp
public class MyPersonalizationTags
{
    // The SilverpopPersonalizationTag attribute is optional
    // and specifies the actual tag name configured in Silverpop.
    //
    // This is useful when you want the model properties to differ
    // from the Silverpop tag names, or you are unable to specify
    // the tag name as a property. For example: spaces are not permitted
    // in C# property names.
    [SilverpopPersonalizationTag("First Name")]
    public string FirstName { get; set; }

    [SilverpopPersonalizationTag("Last Name")]
    public string LastName { get; set; }

    public decimal Amount { get; set; }
}
```

**Then, use the model class when constructing a message.**

```csharp
var message = TransactMessage.Create(
    "123456",
    TransactMessageRecipient.Create(
        "user@example.com",
        new MyPersonalizationTags()
        {
            FirstName = "TheFirstName",
            LastName = "TheLastName",
            Amount = 123.45M
        }));
```

# License

MIT License
