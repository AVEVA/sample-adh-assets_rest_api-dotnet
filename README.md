# OSIsoft Cloud Services Assets .NET REST API Sample

**Version:** 1.0.7

[![Build Status](https://dev.azure.com/osieng/engineering/_apis/build/status/product-readiness/OCS/osisoft.sample-ocs-assets_rest_api-dotnet?repoName=osisoft%2Fsample-ocs-assets_rest_api-dotnet&branchName=main)](https://dev.azure.com/osieng/engineering/_build/latest?definitionId=3279&repoName=osisoft%2Fsample-ocs-assets_rest_api-dotnet&branchName=main)

Developed against DotNet 5.0.

## Requirements

The [.NET Core CLI](https://docs.microsoft.com/en-us/dotnet/core/tools/) is referenced in this sample, and should be installed to run the sample from the command line.

## About this sample

This sample uses REST API calls to work with assets and asset types in OCS. It follows a set of steps to demonstrate the usage of various asset endpoints.

1. Obtain an OAuth token for OCS, using a client-credentials client
1. Create an SDS type
1. Create an SDS stream
1. Insert data into the stream
1. Create an OCS asset
1. Create an OCS asset type
1. Create an asset from an asset type
1. Retrieve an asset
1. Retrieve a resolved asset
1. Update an asset
1. Retrieve the updated asset
1. Retrieve data from an asset
1. Retrieve status for an asset
1. Search for an asset by asset type id
1. Clean up assets, asset types, stream, and type

## Configuring the sample

The sample is configured using the file [appsettings.placeholder.json](AssetsRestApi/appsettings.placeholder.json). Before editing, rename this file to `appsettings.json`. This repository's `.gitignore` rules should prevent the file from ever being checked in to any fork or branch, to ensure credentials are not compromised.

OSIsoft Cloud Services is secured by obtaining tokens from its identity endpoint. Client credentials clients provide a client application identifier and an associated secret (or key) that are authenticated against the token endpoint. You must replace the placeholders in your `appsettings.json` file with the authentication-related values from your tenant and a client-credentials client created in your OCS tenant.

```json
{
  "NamespaceId": "PLACEHOLDER_REPLACE_WITH_NAMESPACE_ID",
  "TenantId": "PLACEHOLDER_REPLACE_WITH_TENANT_ID",
  "Resource": "https://dat-b.osisoft.com",
  "ClientId": "PLACEHOLDER_REPLACE_WITH_CLIENT_ID",
  "ClientSecret": "PLACEHOLDER_REPLACE_WITH_CLIENT_SECRET",
  "ApiVersion": "v1-preview"
}
```

## Running the sample

To run this example from the command line once the `appsettings.json` is configured, run

```shell
dotnet restore
dotnet run
```

## Running the automated test

To test the sample, run

```shell
dotnet restore
dotnet test
```

---

Tested against DotNet 5.0.  
For the OCS Assets samples page [ReadMe](https://github.com/osisoft/OSI-Samples-OCS/blob/main/docs/ASSETS.md)  
For the main OCS samples page [ReadMe](https://github.com/osisoft/OSI-Samples-OCS)  
For the main OSIsoft samples page [ReadMe](https://github.com/osisoft/OSI-Samples)
