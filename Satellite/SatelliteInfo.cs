using YamlDotNet.Serialization;

namespace SatelliteLoader {
    public class SatelliteInfo {
        [YamlMember] public string SatelliteID { get; internal set; }
        [YamlMember] public string Author { get; internal set; }
        [YamlMember] public string Version { get; internal set; }
        [YamlMember] public string Entry { get; internal set; }
        [YamlMember] public string[] RequiredSatellites { get; internal set; }
        [YamlIgnore] public string Path { get; internal set; }
        [YamlIgnore] public bool Loaded { get; internal set; }
        [YamlIgnore] public string ModName { get; set; }
    }
}