using System.IO;
using RiverCommand.Sim;

namespace RiverCommand.Sim.Tests
{
    internal static class TestContent
    {
        private static ContentDatabase _db;

        public static string Root
        {
            get
            {
                var dir = new DirectoryInfo(TestContext());
                while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "Content", "Units")))
                    dir = dir.Parent;
                if (dir == null) throw new DirectoryNotFoundException("Could not find repo Content/ folder");
                return Path.Combine(dir.FullName, "Content");
            }
        }

        private static string TestContext() => NUnit.Framework.TestContext.CurrentContext.TestDirectory;

        public static ContentDatabase Db => _db ??= ContentDatabase.LoadFromDirectory(Root);

        public static SimWorld NewWorld(ulong seed = 42) => new SimWorld(Db, "rock_harbor", seed);
    }
}
