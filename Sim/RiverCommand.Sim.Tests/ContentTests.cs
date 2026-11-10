using System.IO;
using NUnit.Framework;
using Newtonsoft.Json.Linq;

namespace RiverCommand.Sim.Tests
{
    public class ContentTests
    {
        [Test]
        public void AllContentLoads()
        {
            var db = TestContent.Db;
            Assert.That(db.Units.ContainsKey("mlb47"));
            Assert.That(db.Units.ContainsKey("rbs29"));
            Assert.That(db.Maps.ContainsKey("rock_harbor"));
        }

        [Test]
        public void EveryDefinitionHasSchemaVersion()
        {
            foreach (var folder in new[] { "Units", "Maps", "Incidents" })
            foreach (var file in Directory.GetFiles(Path.Combine(TestContent.Root, folder), "*.json"))
            {
                var json = JObject.Parse(File.ReadAllText(file));
                Assert.That(json["schemaVersion"]?.Value<int>(), Is.GreaterThan(0), file);
                Assert.That(json["id"]?.Value<string>(), Is.EqualTo(Path.GetFileNameWithoutExtension(file)), file);
            }
        }

        [Test]
        public void SurfBoatHandlesMoreSurfThanSmallBoat()
        {
            var db = TestContent.Db;
            Assert.That(db.Unit("mlb47").Limits.MaxBarBreakerFt, Is.GreaterThan(db.Unit("rbs29").Limits.MaxBarBreakerFt));
        }
    }
}
