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

        private readonly Dictionary<string, IncidentDef> _incidents = new Dictionary<string, IncidentDef>(StringComparer.Ordinal);
        private readonly List<IncidentDef> _incidentList = new List<IncidentDef>();

        public BarRules BarRules { get; private set; } = new BarRules();
        public SurvivalRules SurvivalRules { get; private set; } = new SurvivalRules();
        public SearchRules SearchRules { get; private set; } = new SearchRules();
        /// <summary>Incident templates in stable (file name) order.</summary>
        public IReadOnlyList<IncidentDef> IncidentList => _incidentList;
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
            foreach (var file in Files(contentRoot, "Incidents"))
            {
                var def = Parse<IncidentDef>(file);
                db._incidents.Add(def.Id, def);
                db._incidentList.Add(def);
            }
            db.BarRules = ParseRule(contentRoot, "bar.json", db.BarRules);
            db.SurvivalRules = ParseRule(contentRoot, "survival.json", db.SurvivalRules);
            db.SearchRules = ParseRule(contentRoot, "search.json", db.SearchRules);
            return db;
        }

        public UnitDef Unit(string id) =>
            _units.TryGetValue(id, out var d) ? d : throw new KeyNotFoundException($"Unknown unit '{id}'");

        public MapDef Map(string id) =>
            _maps.TryGetValue(id, out var m) ? m : throw new KeyNotFoundException($"Unknown map '{id}'");

        public IncidentDef Incident(string id) =>
            _incidents.TryGetValue(id, out var d) ? d : throw new KeyNotFoundException($"Unknown incident '{id}'");

        private static T ParseRule<T>(string root, string name, T fallback)
        {
            var file = Path.Combine(root, "Rules", name);
            return File.Exists(file) ? Parse<T>(file) : fallback;
        }

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
