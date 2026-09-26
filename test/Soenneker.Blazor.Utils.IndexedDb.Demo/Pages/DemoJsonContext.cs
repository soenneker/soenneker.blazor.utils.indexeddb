using System.Text.Json;
using System.Text.Json.Serialization;

namespace Soenneker.Blazor.Utils.IndexedDb.Demo.Pages;

[JsonSourceGenerationOptions(JsonSerializerDefaults.Web, WriteIndented = true)]
[JsonSerializable(typeof(DashboardPreference))]
internal partial class DemoJsonContext : JsonSerializerContext;
