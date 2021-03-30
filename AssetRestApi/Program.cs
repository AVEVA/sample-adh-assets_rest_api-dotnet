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
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");
            _configuration = builder.Build();

            // ==== Client constants ====
            var tenantId = _configuration["TenantId"];
            var namespaceId = _configuration["NamespaceId"];
            var resource = _configuration["Resource"];
            var clientId = _configuration["ClientId"];
            var clientKey = _configuration["ClientKey"];
            var apiVersion = _configuration["ApiVersion"];

            // ==== IDs ====
            const string streamId = "WaveStreamId";
            const string typeId = "WaveDataTypeId";
            const string simpleAssetId = "simpleAsset";
            const string assetId = "SampleAssetId";
            const string assetTypeId = "SampleAssetTypeId";
            const string streamReferenceId = "streamRefOnAsset";
            const string metadataOnAssetTypeId = "MetadataOnAssetType";
            const string metadataOnAssetId = "MetadataOnAsset";

            // ====== Names =====
            const string simpleAssetName = "simpleAssetName";
            const string assetName = "myAssetName";
            const string assetTypeName = "myAssetTypeName";
            const string streamReferenceName = "StreamNameSetOnType";
            const string metadataOnAssetTypeName = "MetadataWasSetOnType";
            const string metadataOnAssetName = "MetadataWasSetOnAsset";
            const string uomOnAssetType = "V";
            const string uomOnAsset = "mV";
            const string statusName = "OrderStatus";

            // Step 1
            _securityHandler = new SdsSecurityHandler(resource, clientId, clientKey);
            using (var httpClient = new HttpClient(_securityHandler) { BaseAddress = new Uri(resource) })
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
                    Console.WriteLine(clientId);
                    var waveType = BuildWaveDataType(typeId);
                    using var content2 = new StringContent(JsonConvert.SerializeObject(waveType));
                    var response =
                        await httpClient.PostAsync(
                            new Uri($"api/{apiVersion}/Tenants/{tenantId}/Namespaces/{namespaceId}/Types/{waveType.Id}", UriKind.Relative),
                            content2)
                        .ConfigureAwait(false);
                    CheckIfResponseWasSuccessful(response);

                    // Step 3
                    // create a SdsStream
                    Console.WriteLine("Creating an SdsStream");
                    var waveStream = new SdsStream
                    {
                        Id = streamId,
                        Name = "WaveStream",
                        TypeId = waveType.Id,
                    };
                    using var content3 = new StringContent(JsonConvert.SerializeObject(waveStream));
                    response = await httpClient.PostAsync(
                        new Uri($"api/{apiVersion}/Tenants/{tenantId}/Namespaces/{namespaceId}/Streams/{waveStream.Id}", UriKind.Relative),
                        content3)
                        .ConfigureAwait(false);
                    CheckIfResponseWasSuccessful(response);

                    // Step 4
                    // insert data
                    Console.WriteLine("Inserting data");

                    // insert a single event
                    var singleWaveList = new List<WaveData>();
                    var wave = GetWave(0, 2.0);
                    singleWaveList.Add(wave);
                    using var content4a = new StringContent(JsonConvert.SerializeObject(singleWaveList));
                    response = await httpClient.PostAsync(
                        new Uri($"api/{apiVersion}/Tenants/{tenantId}/Namespaces/{namespaceId}/Streams/{waveStream.Id}/Data", UriKind.Relative),
                        content4a)
                        .ConfigureAwait(false);
                    CheckIfResponseWasSuccessful(response);

                    // insert a list of events
                    List<WaveData> waves = new List<WaveData>();
                    for (int i = 2; i < 20; i += 2)
                    {
                        WaveData newEvent = GetWave(i, 2.0);
                        waves.Add(newEvent);
                    }

                    using var content4b = new StringContent(JsonConvert.SerializeObject(waves));
                    response = await httpClient.PostAsync(
                            new Uri($"api/{apiVersion}/Tenants/{tenantId}/Namespaces/{namespaceId}/Streams/{waveStream.Id}/Data", UriKind.Relative),
                            content4b)
                        .ConfigureAwait(false);

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new HttpRequestException();
                    }

                    // ASSETS:
                    // Create Simple Asset
                    Console.WriteLine("Creating Basic Asset");
                    var simpleAsset = new Asset
                    {
                        Id = simpleAssetId,
                        Name = simpleAssetName,
                        Description = "My First Asset!",
                    };

                    using var simpleAssetString = new StringContent(JsonConvert.SerializeObject(simpleAsset));
                    simpleAssetString.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    response = await httpClient.PostAsync(
                            new Uri($"api/{apiVersion}/Tenants/{tenantId}/Namespaces/{namespaceId}/Assets/{simpleAssetId}", UriKind.Relative),
                            simpleAssetString)
                        .ConfigureAwait(false);

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new HttpRequestException();
                    }

                    // Create AssetType + Asset
                    var typeReference = new TypeReferenceDto
                    {
                        StreamReferenceId = streamReferenceId,
                        StreamReferenceName = streamReferenceName,
                        TypeId = typeId,
                    };

                    var metadataOnType = new MetadataDto
                    {
                        Id = metadataOnAssetTypeId,
                        Name = metadataOnAssetTypeName,
                        Description = "We are going to use this metadata to show inheritance",
                        Uom = uomOnAssetType,
                        SdsTypeCode = SdsTypeCode.Int64,
                    };

                    var statusMapping = new StatusMappingDto
                    {
                        Name = statusName,
                        StreamReferenceId = typeReference.StreamReferenceId,
                        StreamPropertyId = nameof(wave.Order),
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
                    };

                    Console.WriteLine("Creating AssetType");
                    var assetType = new AssetType
                    {
                        Id = assetTypeId,
                        Name = assetTypeName,
                        Description = "My first AssetType!",
                        TypeReferences = new List<TypeReferenceDto> { typeReference },
                        Metadata = new List<MetadataDto> { metadataOnType },
                        Status = statusMapping,
                    };

                    using var assetTypeString = new StringContent(JsonConvert.SerializeObject(assetType));
                    assetTypeString.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    response = await httpClient.PostAsync(
                            new Uri($"api/{apiVersion}/Tenants/{tenantId}/Namespaces/{namespaceId}/AssetTypes/{assetTypeId}", UriKind.Relative),
                            assetTypeString)
                        .ConfigureAwait(false);

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new HttpRequestException();
                    }

                    Console.WriteLine("Creating Asset with AssetType");
                    var streamReference = new StreamReferenceDto
                    {
                        Id = streamReferenceId,
                        StreamId = waveStream.Id,
                        Description = "streamReference On Asset",
                    };

                    var metadataInherited = new MetadataDto
                    {
                        Id = metadataOnAssetTypeId,
                        Description = "Metadata Name, SdsTypeCode and Uom Inherited from AssetType",
                    };

                    var metadataOnAsset = new MetadataDto
                    {
                        Id = metadataOnAssetId,
                        Name = metadataOnAssetName,
                        Description = "Simple Metadata Set on Asset",
                        Uom = uomOnAsset,
                        SdsTypeCode = SdsTypeCode.Double,
                    };

                    var asset = new Asset
                    {
                        Id = assetId,
                        Name = assetName,
                        AssetTypeId = assetTypeId,
                        StreamReferences = new List<StreamReferenceDto> { streamReference },
                        Metadata = new List<MetadataDto> { metadataOnAsset, metadataInherited },
                    };

                    using var assetString = new StringContent(JsonConvert.SerializeObject(asset));
                    assetString.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    response = await httpClient.PostAsync(
                            new Uri($"api/{apiVersion}/Tenants/{tenantId}/Namespaces/{namespaceId}/Assets/{assetId}", UriKind.Relative),
                            assetString)
                        .ConfigureAwait(false);

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new HttpRequestException();
                    }

                    // Get Asset 
                    Console.WriteLine("Getting Asset Back");
                    response = await httpClient.GetAsync(
                            new Uri($"api/{apiVersion}/Tenants/{tenantId}/Namespaces/{namespaceId}/Assets/{assetId}", UriKind.Relative))
                        .ConfigureAwait(false);

                    var returnedAsset = JsonConvert.DeserializeObject<Asset>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
                    Console.WriteLine($"Returned Asset has Id {returnedAsset.Id} and Name {returnedAsset.Name} \n");

                    // Get Resolved Asset
                    // We did not set a Description on Asset, and it should be inherited from AssetType
                    Console.WriteLine("Getting Resolved Asset Back");
                    response = await httpClient.GetAsync(
                            new Uri($"api/{apiVersion}/Tenants/{tenantId}/Namespaces/{namespaceId}/Assets/{assetId}/resolved", UriKind.Relative))
                        .ConfigureAwait(false);

                    returnedAsset = JsonConvert.DeserializeObject<Asset>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
                    Console.WriteLine($"Asset null values get overriden by its AssetType (if one exists) when resolved: Asset Description = {returnedAsset.Description} \n");

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new HttpRequestException();
                    }

                    // Update Asset
                    // Changing the Description 
                    var updatedAsset = new Asset
                    {
                        Id = assetId,
                        Name = assetName,
                        AssetTypeId = assetTypeId,
                        Description = "My First Asset with AssetType!",
                        StreamReferences = new List<StreamReferenceDto> { streamReference },
                        Metadata = new List<MetadataDto> { metadataOnAsset, metadataInherited },
                    };

                    Console.WriteLine("Updating Asset To fix Description.");
                    using var updatedAssetString = new StringContent(JsonConvert.SerializeObject(updatedAsset));
                    updatedAssetString.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    response = await httpClient.PutAsync(
                            new Uri($"api/{apiVersion}/Tenants/{tenantId}/Namespaces/{namespaceId}/Assets/{assetId}", UriKind.Relative),
                            updatedAssetString)
                        .ConfigureAwait(false);

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new HttpRequestException();
                    }

                    // Getting Asset Back and looking at Inheritance 
                    Console.WriteLine("Getting the updated asset back");
                    response = await httpClient.GetAsync(
                            new Uri($"api/{apiVersion}/Tenants/{tenantId}/Namespaces/{namespaceId}/Assets/{assetId}", UriKind.Relative))
                        .ConfigureAwait(false);
                    var updatedAssetReturned = JsonConvert.DeserializeObject<object>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new HttpRequestException();
                    }

                    Console.WriteLine(updatedAssetReturned.ToString());

                    // Actions on Asset or AssetType
                    // We can take data directly from an asset
                    Console.WriteLine("\n Getting Last data on Asset");
                    response = await httpClient.GetAsync(
                            new Uri($"api/{apiVersion}/Tenants/{tenantId}/Namespaces/{namespaceId}/Assets/{assetId}/Data/Last", UriKind.Relative))
                        .ConfigureAwait(false);

                    var data = JsonConvert.DeserializeObject<object>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
                    Console.WriteLine(data.ToString());

                    Console.WriteLine("\n Getting Last Status On Asset");

                    response = await httpClient.GetAsync(
                            new Uri($"api/{apiVersion}/Tenants/{tenantId}/Namespaces/{namespaceId}/Assets/{assetId}/Status/Last", UriKind.Relative))
                        .ConfigureAwait(false);

                    var status = JsonConvert.DeserializeObject<object>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
                    Console.WriteLine(status.ToString());

                    Console.WriteLine("\n Searching for Asset");
                    Console.WriteLine($"Searching for asset with AssetTypeId '{assetTypeId}'");
                    response = await httpClient.GetAsync(
                            new Uri($"api/{apiVersion}/Tenants/{tenantId}/Namespaces/{namespaceId}/Assets?query=AssetTypeId:{assetTypeId}", UriKind.Relative))
                        .ConfigureAwait(false);

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new HttpRequestException();
                    }

                    var foundAsset = JsonConvert.DeserializeObject<object>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
                    Console.WriteLine(foundAsset.ToString());
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    _toThrow = e;
                }
                finally
                {
                    Console.WriteLine();
                    Console.WriteLine("Cleaning up");

                    Console.WriteLine("Deleting Simple Asset");
                    RunInTryCatch(httpClient.DeleteAsync, $"api/{apiVersion}/Tenants/{tenantId}/Namespaces/{namespaceId}/Assets/{simpleAssetId}");
                    Console.WriteLine("Deleting Asset");
                    RunInTryCatch(httpClient.DeleteAsync, $"api/{apiVersion}/Tenants/{tenantId}/Namespaces/{namespaceId}/Assets/{assetId}");
                    Console.WriteLine("Deleting AssetType");
                    RunInTryCatch(httpClient.DeleteAsync, $"api/{apiVersion}/Tenants/{tenantId}/Namespaces/{namespaceId}/AssetTypes/{assetTypeId}");
                    Console.WriteLine("Deleting stream");
                    RunInTryCatch(httpClient.DeleteAsync, $"api/{apiVersion}/Tenants/{tenantId}/Namespaces/{namespaceId}/Streams/{streamId}");
                    Console.WriteLine("Deleting types");
                    RunInTryCatch(httpClient.DeleteAsync, $"api/{apiVersion}/Tenants/{tenantId}/Namespaces/{namespaceId}/Types/{typeId}");
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
            var intSdsType = new SdsType
            {
                Id = "intSdsType",
                SdsTypeCode = SdsTypeCode.Int32,
            };

            var doubleSdsType = new SdsType
            {
                Id = "doubleSdsType",
                SdsTypeCode = SdsTypeCode.Double,
            };

            var orderProperty = new SdsTypeProperty
            {
                Id = "Order",
                SdsType = intSdsType,
                IsKey = true,
            };

            var tauProperty = new SdsTypeProperty
            {
                Id = "Tau",
                SdsType = doubleSdsType,
            };

            var radiansProperty = new SdsTypeProperty
            {
                Id = "Radians",
                SdsType = doubleSdsType,
            };

            var sinProperty = new SdsTypeProperty
            {
                Id = "Sin",
                SdsType = doubleSdsType,
            };

            var cosProperty = new SdsTypeProperty
            {
                Id = "Cos",
                SdsType = doubleSdsType,
            };

            var tanProperty = new SdsTypeProperty
            {
                Id = "Tan",
                SdsType = doubleSdsType,
            };

            var sinhProperty = new SdsTypeProperty
            {
                Id = "Sinh",
                SdsType = doubleSdsType,
            };

            var coshProperty = new SdsTypeProperty
            {
                Id = "Cosh",
                SdsType = doubleSdsType,
            };

            var tanhProperty = new SdsTypeProperty
            {
                Id = "Tanh",
                SdsType = doubleSdsType,
            };

            var waveType = new SdsType
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
            var radians = order * (Math.PI / 32);

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
