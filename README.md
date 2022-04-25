# AVEVA Data Hub Assets .NET REST API Sample

| :loudspeaker: **Notice**: Samples have been updated to reflect that they work on AVEVA Data Hub. The samples also work on OSIsoft Cloud Services unless otherwise noted. |
| -----------------------------------------------------------------------------------------------|  

**Version:** 1.0.11

[![Build Status](https://dev.azure.com/osieng/engineering/_apis/build/status/product-readiness/ADH/aveva.sample-adh-assets_rest_api-dotnet?branchName=main)](https://dev.azure.com/osieng/engineering/_build/latest?definitionId=3279&branchName=main)

Developed against DotNet 5.0.

## Requirements

The [.NET Core CLI](https://docs.microsoft.com/en-us/dotnet/core/tools/) is referenced in this sample, and should be installed to run the sample from the command line.

## About this sample

This sample uses REST API calls to work with assets and asset types in ADH. It follows a set of steps to demonstrate the usage of various asset endpoints.

1. Obtain an OAuth token for ADH, using a client-credentials client
1. Create an SDS type
1. Create an SDS stream
1. Insert data into the stream
1. Create an ADH asset
1. Create an ADH asset type
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

AVEVA Data Hub is secured by obtaining tokens from its identity endpoint. Client credentials clients provide a client application identifier and an associated secret (or key) that are authenticated against the token endpoint. You must replace the placeholders in your `appsettings.json` file with the authentication-related values from your tenant and a client-credentials client created in your ADH tenant.

```json
{
  "NamespaceId": "PLACEHOLDER_REPLACE_WITH_NAMESPACE_ID",
  "TenantId": "PLACEHOLDER_REPLACE_WITH_TENANT_ID",
  "Resource": "https://uswe.datahub.connect.aveva.com",
  "ClientId": "PLACEHOLDER_REPLACE_WITH_CLIENT_ID",
  "ClientSecret": "PLACEHOLDER_REPLACE_WITH_CLIENT_SECRET",
  "ApiVersion": "v1"
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
For the ADH Assets samples page [ReadMe](https://github.com/osisoft/OSI-Samples-OCS/blob/main/docs/ASSETS.md)  
For the main ADH samples page [ReadMe](https://github.com/osisoft/OSI-Samples-OCS)  
For the main AVEVA samples page [ReadMe](https://github.com/osisoft/OSI-Samples)
