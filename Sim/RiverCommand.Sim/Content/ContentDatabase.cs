using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace RiverCommand.Sim
{
    /// <summary>Loads JSON content. Unknown fields fail loudly so typos in content are caught in tests.</summary>
    public sealed class ContentDatabase
    {
        private readonly Dictionary<string, UnitDef> _units = new Dictionary<string, UnitDef>(StringComparer.Ordinal);
        private readonly Dictionary<string, MapDef> _maps = new Dictionary<string, MapDef>(StringComparer.Ordinal);

        public BarRules BarRules { get; private set; } = new BarRules();
        public IReadOnlyDictionary<string, UnitDef> Units => _units;
        public IReadOnlyDictionary<string, MapDef> Maps => _maps;

        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MissingMemberHandling = MissingMemberHandling.Error,
        };

        public static ContentDatabase LoadFromDirectory(string contentRoot)
        {
            var db = new ContentDatabase();
            foreach (var file in Files(contentRoot, "Units"))
            {
                var def = Parse<UnitDef>(file);
                if (string.IsNullOrEmpty(def.Id)) throw new InvalidDataException($"{file}: missing id");
                db._units.Add(def.Id, def);
            }
            foreach (var file in Files(contentRoot, "Maps"))
            {
                var map = Parse<MapDef>(file);
                db._maps.Add(map.Id, map);
            }
            var barFile = Path.Combine(contentRoot, "Rules", "bar.json");
            if (File.Exists(barFile)) db.BarRules = Parse<BarRules>(barFile);
            return db;
        }

        public UnitDef Unit(string id) =>
            _units.TryGetValue(id, out var d) ? d : throw new KeyNotFoundException($"Unknown unit '{id}'");

        public MapDef Map(string id) =>
            _maps.TryGetValue(id, out var m) ? m : throw new KeyNotFoundException($"Unknown map '{id}'");

        private static IEnumerable<string> Files(string root, string folder)
        {
            var dir = Path.Combine(root, folder);
            if (!Directory.Exists(dir)) return Array.Empty<string>();
            var files = Directory.GetFiles(dir, "*.json");
            Array.Sort(files, StringComparer.Ordinal); // stable load order
            return files;
        }

        private static T Parse<T>(string file)
        {
            try { return JsonConvert.DeserializeObject<T>(File.ReadAllText(file), Settings); }
            catch (JsonException e) { throw new InvalidDataException($"{file}: {e.Message}", e); }
        }
    }
}
