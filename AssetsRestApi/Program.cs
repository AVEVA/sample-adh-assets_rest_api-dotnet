using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace AssetsRestApi
{
    public static class Program
    {
        private static IConfiguration _configuration;
        private static SdsSecurityHandler _securityHandler;
        private static Exception _toThrow;

        public static void Main() => MainAsync().GetAwaiter().GetResult();

        public static async Task<bool> MainAsync(bool test = false)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");
            _configuration = builder.Build();

            // ==== Client constants ====
            string tenantId = _configuration["TenantId"];
            string namespaceId = _configuration["NamespaceId"];
            string resource = _configuration["Resource"];
            string clientId = _configuration["ClientId"];
            string clientSecret = _configuration["ClientSecret"];
            string apiVersion = _configuration["ApiVersion"];

            // ==== IDs ====
            const string StreamId = "WaveStreamId";
            const string TypeId = "WaveDataTypeId";
            const string SimpleAssetId = "simpleAsset";
            const string AssetId = "SampleAssetId";
            const string AssetTypeId = "SampleAssetTypeId";
            const string StreamReferenceId = "streamRefOnAsset";
            const string MetadataOnAssetTypeId = "MetadataOnAssetType";
            const string MetadataOnAssetId = "MetadataOnAsset";

            // ====== Names =====
            const string SimpleAssetName = "simpleAssetName";
            const string AssetName = "myAssetName";
            const string AssetTypeName = "myAssetTypeName";
            const string StreamReferenceName = "StreamNameSetOnType";
            const string MetadataOnAssetTypeName = "MetadataWasSetOnType";
            const string MetadataOnAssetName = "MetadataWasSetOnAsset";
            const string UomOnAssetType = "V";
            const string UomOnAsset = "mV";

            // Step 1
            _securityHandler = new SdsSecurityHandler(resource, clientId, clientSecret);
            using (HttpClient httpClient = new (_securityHandler) { BaseAddress = new Uri(resource) })
            {
                httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip");

                Console.WriteLine(@"----------------------------------");
                Console.WriteLine(@"   ___                   _        ");
                Console.WriteLine(@"  / _ \                 | |      ");
                Console.WriteLine(@" / /_\ \ ___  ___   ___ | |_  ___ ");
                Console.WriteLine(@" |  _  |/ __|/ __| / _ \| __|/ __|");
                Console.WriteLine(@" | | | |\__ \\__ \|  __/| |_ \__ \");
                Console.WriteLine(@" \_| |_/|___/|___/ \___| \__||___/");
                Console.WriteLine(@"----------------------------------");
                Console.WriteLine();
                Console.WriteLine($"SDS endpoint at {resource}");
                Console.WriteLine();

                try
                {
                    // Step 2
                    // create a SdsType
                    Console.WriteLine("Creating an SdsType");
                    SdsType waveType = BuildWaveDataType(TypeId);
                    using StringContent content2 = new (JsonConvert.SerializeObject(waveType));
                    HttpResponseMessage response =
                        await httpClient.PostAsync(
                            new Uri($"api/{apiVersion}/Tenants/{tenantId}/Namespaces/{namespaceId}/Types/{waveType.Id}", UriKind.Relative),
                            content2)
                        .ConfigureAwait(false);
                    CheckIfResponseWasSuccessful(response);

                    // Step 3
                    // create a SdsStream
                    Console.WriteLine("Creating an SdsStream");
                    SdsStream waveStream = new ()
                    {
                        Id = StreamId,
                        Name = "WaveStream",
                        TypeId = waveType.Id,
                    };
                    using StringContent content3 = new (JsonConvert.SerializeObject(waveStream));
                    response = await httpClient.PostAsync(
                        new Uri($"api/{apiVersion}/Tenants/{tenantId}/Namespaces/{namespaceId}/Streams/{waveStream.Id}", UriKind.Relative),
                        content3)
                        .ConfigureAwait(false);
                    CheckIfResponseWasSuccessful(response);

                    // Step 4
                    // insert data
                    Console.WriteLine("Inserting data");

                    // insert a single event
                    List<WaveData> waves = new ();
                    for (int i = 0; i < 20; i += 2)
                    {
                        WaveData newEvent = GetWave(i, 2.0);
                        waves.Add(newEvent);
                    }

                    using StringContent content4b = new (JsonConvert.SerializeObject(waves));
                    response = await httpClient.PostAsync(
                            new Uri($"api/{apiVersion}/Tenants/{tenantId}/Namespaces/{namespaceId}/Streams/{waveStream.Id}/Data", UriKind.Relative),
                            content4b)
                        .ConfigureAwait(false);
                    CheckIfResponseWasSuccessful(response);

                    // ASSETS:
                    // Step 5
                    // Create Simple Asset
                    Console.WriteLine("Creating Basic Asset");
                    Asset simpleAsset = new ()
                    {
                        Id = SimpleAssetId,
                        Name = SimpleAssetName,
                        Description = "My First Asset!",
                    };

                    using StringContent simpleAssetString = new (JsonConvert.SerializeObject(simpleAsset));
                    simpleAssetString.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    response = await httpClient.PostAsync(
                            new Uri($"api/{apiVersion}/Tenants/{tenantId}/Namespaces/{namespaceId}/Assets/{SimpleAssetId}", UriKind.Relative),
                            simpleAssetString)
                        .ConfigureAwait(false);
                    CheckIfResponseWasSuccessful(response);

                    // Step 6
                    // Create AssetType + Asset
                    TypeReferenceDto typeReference = new ()
                    {
                        StreamReferenceId = StreamReferenceId,
                        StreamReferenceName = StreamReferenceName,
                        TypeId = TypeId,
                    };

                    MetadataDto metadataOnType = new ()
                    {
                        Id = MetadataOnAssetTypeId,
                        Name = MetadataOnAssetTypeName,
                        Description = "We are going to use this metadata to show inheritance",
                        Uom = UomOnAssetType,
                        SdsTypeCode = SdsTypeCode.Int64,
                    };

                    StatusConfigurationDto statusMapping = new ()
                    {
                        Definition = new StatusMappingDto
                        {
                            StreamReferenceId = typeReference.StreamReferenceId,
                            StreamPropertyId = nameof(WaveData.Order),
                            ValueStatusMappings = new List<ValueStatusMappingDto>
                            {
                                new ValueStatusMappingDto
                                {
                                    Value = 0,
                                    Status = StatusEnum.Warning,
                                },
                                new ValueStatusMappingDto
                                {
                                    Value = 10,
                                    Status = StatusEnum.Good,
                                },
                                new ValueStatusMappingDto
                                {
                                    Value = 18,
                                    Status = StatusEnum.Bad,
                                },
                            },
                        },
                    };

                    Console.WriteLine("Creating AssetType");
                    AssetType assetType = new ()
                    {
                        Id = AssetTypeId,
                        Name = AssetTypeName,
                        Description = "My first AssetType!",
                        TypeReferences = new List<TypeReferenceDto> { typeReference },
                        Metadata = new List<MetadataDto> { metadataOnType },
                        Status = statusMapping,
                    };

                    using StringContent assetTypeString = new (JsonConvert.SerializeObject(assetType));
                    assetTypeString.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    response = await httpClient.PostAsync(
                            new Uri($"api/{apiVersion}/Tenants/{tenantId}/Namespaces/{namespaceId}/AssetTypes/{AssetTypeId}", UriKind.Relative),
                            assetTypeString)
                        .ConfigureAwait(false);
                    CheckIfResponseWasSuccessful(response);

                    // Step 7
                    Console.WriteLine("Creating Asset with AssetType");
                    StreamReferenceDto streamReference = new ()
                    {
                        Id = StreamReferenceId,
                        StreamId = waveStream.Id,
                    };

                    MetadataDto metadataInherited = new () { Id = MetadataOnAssetTypeId };

                    MetadataDto metadataOnAsset = new ()
                    {
                        Id = MetadataOnAssetId,
                        Name = MetadataOnAssetName,
                        Description = "Simple Metadata Set on Asset",
                        Uom = UomOnAsset,
                        SdsTypeCode = SdsTypeCode.Double,
                    };

                    Asset asset = new ()
                    {
                        Id = AssetId,
                        Name = AssetName,
                        AssetTypeId = AssetTypeId,
                        StreamReferences = new List<StreamReferenceDto> { streamReference },
                        Metadata = new List<MetadataDto> { metadataOnAsset, metadataInherited },
                    };

                    using StringContent assetString = new (JsonConvert.SerializeObject(asset));
                    assetString.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    response = await httpClient.PostAsync(
                            new Uri($"api/{apiVersion}/Tenants/{tenantId}/Namespaces/{namespaceId}/Assets/{AssetId}", UriKind.Relative),
                            assetString)
                        .ConfigureAwait(false);
                    CheckIfResponseWasSuccessful(response);

                    // Step 8
                    // Get Asset 
                    Console.WriteLine("Getting Asset Back");
                    response = await httpClient.GetAsync(
                            new Uri($"api/{apiVersion}/Tenants/{tenantId}/Namespaces/{namespaceId}/Assets/{AssetId}", UriKind.Relative))
                        .ConfigureAwait(false);
                    CheckIfResponseWasSuccessful(response);

                    Asset returnedAsset = JsonConvert.DeserializeObject<Asset>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
                    Console.WriteLine($"Returned Asset has Id {returnedAsset.Id} and Name {returnedAsset.Name} \n");

                    // Step 9
                    // Get Resolved Asset
                    // We did not set a Description on Asset, and it should be inherited from AssetType
                    Console.WriteLine("Getting Resolved Asset Back");
                    response = await httpClient.GetAsync(
                            new Uri($"api/{apiVersion}/Tenants/{tenantId}/Namespaces/{namespaceId}/Assets/{AssetId}/resolved", UriKind.Relative))
                        .ConfigureAwait(false);
                    CheckIfResponseWasSuccessful(response);

                    returnedAsset = JsonConvert.DeserializeObject<Asset>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
                    Console.WriteLine($"Asset null values get overriden by its AssetType (if one exists) when resolved: Asset Description = {returnedAsset.Description} \n");

                    // Step 10
                    // Update Asset
                    // Changing the Description 
                    asset.Description = "My First Asset with AssetType!";
                    Console.WriteLine("Updating Asset To fix Description.");
                    using StringContent updatedAssetString = new (JsonConvert.SerializeObject(asset));
                    updatedAssetString.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    response = await httpClient.PutAsync(
                            new Uri($"api/{apiVersion}/Tenants/{tenantId}/Namespaces/{namespaceId}/Assets/{AssetId}", UriKind.Relative),
                            updatedAssetString)
                        .ConfigureAwait(false);
                    CheckIfResponseWasSuccessful(response);

                    // Step 11
                    // Getting Asset Back and looking at Inheritance 
                    Console.WriteLine("Getting the updated asset back");
                    response = await httpClient.GetAsync(
                            new Uri($"api/{apiVersion}/Tenants/{tenantId}/Namespaces/{namespaceId}/Assets/{AssetId}", UriKind.Relative))
                        .ConfigureAwait(false);
                    CheckIfResponseWasSuccessful(response);
                    object updatedAssetReturned = JsonConvert.DeserializeObject<object>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));

                    Console.WriteLine(updatedAssetReturned.ToString());

                    // Step 12
                    // Actions on Asset or AssetType
                    // We can take data directly from an asset
                    Console.WriteLine("\n Getting Last data on Asset");
                    response = await httpClient.GetAsync(
                            new Uri($"api/{apiVersion}/Tenants/{tenantId}/Namespaces/{namespaceId}/Assets/{AssetId}/Data/Last", UriKind.Relative))
                        .ConfigureAwait(false);
                    CheckIfResponseWasSuccessful(response);

                    object data = JsonConvert.DeserializeObject<object>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
                    Console.WriteLine(data.ToString());

                    // Step 13
                    Console.WriteLine("\n Getting Last Status On Asset");
                    response = await httpClient.GetAsync(
                            new Uri($"api/{apiVersion}/Tenants/{tenantId}/Namespaces/{namespaceId}/Assets/{AssetId}/Status/Last", UriKind.Relative))
                        .ConfigureAwait(false);
                    CheckIfResponseWasSuccessful(response);

                    object status = JsonConvert.DeserializeObject<object>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
                    Console.WriteLine(status.ToString());

                    // Step 14
                    Console.WriteLine("\n Searching for Asset");
                    Console.WriteLine($"Searching for asset with AssetTypeId '{AssetTypeId}'");
                    response = await httpClient.GetAsync(
                            new Uri($"api/{apiVersion}/Tenants/{tenantId}/Namespaces/{namespaceId}/Assets?query=AssetTypeId:{AssetTypeId}", UriKind.Relative))
                        .ConfigureAwait(false);
                    CheckIfResponseWasSuccessful(response);

                    object foundAsset = JsonConvert.DeserializeObject<object>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
                    Console.WriteLine(foundAsset.ToString());
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    _toThrow = e;
                }
                finally
                {
                    // Step 15
                    Console.WriteLine();
                    Console.WriteLine("Cleaning up");

                    Console.WriteLine("Deleting Simple Asset");
                    RunInTryCatch(httpClient.DeleteAsync, $"api/{apiVersion}/Tenants/{tenantId}/Namespaces/{namespaceId}/Assets/{SimpleAssetId}");
                    Console.WriteLine("Deleting Asset");
                    RunInTryCatch(httpClient.DeleteAsync, $"api/{apiVersion}/Tenants/{tenantId}/Namespaces/{namespaceId}/Assets/{AssetId}");
                    Console.WriteLine("Deleting AssetType");
                    RunInTryCatch(httpClient.DeleteAsync, $"api/{apiVersion}/Tenants/{tenantId}/Namespaces/{namespaceId}/AssetTypes/{AssetTypeId}");
                    Console.WriteLine("Deleting stream");
                    RunInTryCatch(httpClient.DeleteAsync, $"api/{apiVersion}/Tenants/{tenantId}/Namespaces/{namespaceId}/Streams/{StreamId}");
                    Console.WriteLine("Deleting types");
                    RunInTryCatch(httpClient.DeleteAsync, $"api/{apiVersion}/Tenants/{tenantId}/Namespaces/{namespaceId}/Types/{TypeId}");
                    Console.WriteLine("Complete!");
                }
            }

            if (test && _toThrow != null)
                throw _toThrow;
            return _toThrow == null;
        }

        private static void CheckIfResponseWasSuccessful(HttpResponseMessage response)
        {
            // If support is needed please know the Operation-ID header information for support purposes (it is included in the exception below automatically too)
            // string operationId = response.Headers.GetValues("Operation-Id").First();
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(response.ToString());
            }
        }

        /// <summary>
        /// Use this to run a method that you don't want to stop the program if there is an error
        /// </summary>
        /// <param name="methodToRun">The method to run.</param>
        /// <param name="value">The value to put into the method to run</param>
        private static void RunInTryCatch(Func<string, Task> methodToRun, string value)
        {
            try
            {
                methodToRun(value).Wait(10000);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Got error in {methodToRun.Method.Name} with value {value} but continued on:" + ex.Message);
                _toThrow ??= ex;
            }
        }

        private static SdsType BuildWaveDataType(string id)
        {
            SdsType intSdsType = new ()
            {
                Id = "intSdsType",
                SdsTypeCode = SdsTypeCode.Int32,
            };

            SdsType doubleSdsType = new ()
            {
                Id = "doubleSdsType",
                SdsTypeCode = SdsTypeCode.Double,
            };

            SdsTypeProperty orderProperty = new ()
            {
                Id = "Order",
                SdsType = intSdsType,
                IsKey = true,
            };

            SdsTypeProperty tauProperty = new ()
            {
                Id = "Tau",
                SdsType = doubleSdsType,
            };

            SdsTypeProperty radiansProperty = new ()
            {
                Id = "Radians",
                SdsType = doubleSdsType,
            };

            SdsTypeProperty sinProperty = new ()
            {
                Id = "Sin",
                SdsType = doubleSdsType,
            };

            SdsTypeProperty cosProperty = new ()
            {
                Id = "Cos",
                SdsType = doubleSdsType,
            };

            SdsTypeProperty tanProperty = new ()
            {
                Id = "Tan",
                SdsType = doubleSdsType,
            };

            SdsTypeProperty sinhProperty = new ()
            {
                Id = "Sinh",
                SdsType = doubleSdsType,
            };

            SdsTypeProperty coshProperty = new ()
            {
                Id = "Cosh",
                SdsType = doubleSdsType,
            };

            SdsTypeProperty tanhProperty = new ()
            {
                Id = "Tanh",
                SdsType = doubleSdsType,
            };

            SdsType waveType = new ()
            {
                Id = id,
                Name = "WaveData",
                Properties = new List<SdsTypeProperty>
                {
                    orderProperty,
                    tauProperty,
                    radiansProperty,
                    sinProperty,
                    cosProperty,
                    tanProperty,
                    sinhProperty,
                    coshProperty,
                    tanhProperty,
                },
                SdsTypeCode = SdsTypeCode.Object,
            };

            return waveType;
        }

        private static WaveData GetWave(int order, double multiplier)
        {
            double radians = order * (Math.PI / 32);

            return new WaveData
            {
                Order = order,
                Radians = radians,
                Tau = radians / (2 * Math.PI),
                Sin = multiplier * Math.Sin(radians),
                Cos = multiplier * Math.Cos(radians),
                Tan = multiplier * Math.Tan(radians),
                Sinh = multiplier * Math.Sinh(radians),
                Cosh = multiplier * Math.Cosh(radians),
                Tanh = multiplier * Math.Tanh(radians),
            };
        }
    }
}
