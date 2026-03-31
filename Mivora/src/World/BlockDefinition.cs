using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mivora.World;

public class BlockPlacement
{
    [JsonPropertyName("condition")]    public string Condition       { get; set; } = "";
    [JsonPropertyName("depth")]        public int    Depth           { get; set; } = 3;
    [JsonPropertyName("minY")]         public int    MinY            { get; set; } = 0;
    [JsonPropertyName("maxY")]         public int    MaxY            { get; set; } = 64;
    [JsonPropertyName("excludeNearWater")] public bool ExcludeNearWater { get; set; } = false;
    [JsonPropertyName("noiseThreshold")]   public float NoiseThreshold  { get; set; } = 0.75f;
    [JsonPropertyName("noiseScale")]       public float NoiseScale       { get; set; } = 0.1f;
}

public class BlockDefinition
{
    [JsonPropertyName("id")]          public int            Id        { get; set; }
    [JsonPropertyName("name")]        public string         Name      { get; set; } = "";
    [JsonPropertyName("texturePath")] public string         TexturePath { get; set; } = "";
    [JsonPropertyName("objPath")]     public string         ObjPath   { get; set; } = "";
    [JsonPropertyName("placement")]   public BlockPlacement Placement { get; set; } = new();
}

public class BlockRegistry
{
    [JsonPropertyName("blocks")]
    public List<BlockDefinition> Blocks { get; set; } = new();
}
