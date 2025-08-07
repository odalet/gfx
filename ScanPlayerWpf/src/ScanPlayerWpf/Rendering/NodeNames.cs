using ScanPlayerWpf.Models;

namespace ScanPlayerWpf.Rendering
{
    internal static class NodeNames
    {
        public static string Reference => nameof(Reference);
        public static string Platform => nameof(Platform);
        public static string HeadReferences => nameof(HeadReferences);
        public static string HeadFields => nameof(HeadFields);
        public static string Heads => nameof(Heads);
        public static string HeadPrefix => "Head#";

        public static string Jumps => nameof(Jumps);
        public static string Marks => nameof(Marks);
        public static string Points => nameof(Points);

        public static string GetHeadNodeName(IHeadDefinition head) => GetHeadNodeName(head.Id);
        public static string GetHeadNodeName(int headId) => $"{HeadPrefix}{headId}";
    }
}
