using System.Text.Encodings.Web;
using System.Text.Json;

namespace DOL
{
    public sealed class SaveData
    {
        public int Version { get; set; } = 1;
        public string Name { get; set; } = "";
        public string SavedAt { get; set; } = "";
        public string CharacterId { get; set; } = "stelle";
        public string PlayerName { get; set; } = "星";
        public string Portrait { get; set; } = "html_img/start.jpg";
        public string Mode { get; set; } = "basic";
        public List<string> Feats { get; set; } = new();
        public GameState State { get; set; } = new();
    }

    /// <summary>存檔放在 %AppData%\DOL\saves。0 號是睡覺時的自動存檔，1~8 是手動欄位。</summary>
    public static class SaveStore
    {
        public const int Slots = 8;

        private static readonly JsonSerializerOptions Opt = new()
        {
            IncludeFields = true,
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        };

        private static string Dir
        {
            get
            {
                var d = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DOL", "saves");
                Directory.CreateDirectory(d);
                return d;
            }
        }

        private static string PathOf(int slot) => Path.Combine(Dir, slot == 0 ? "auto.json" : $"slot{slot}.json");

        public static void Write(int slot, SaveData d)
        {
            d.SavedAt = DateTime.Now.ToString("MM-dd HH:mm");
            var tmp = PathOf(slot) + ".tmp";
            File.WriteAllText(tmp, JsonSerializer.Serialize(d, Opt));
            File.Move(tmp, PathOf(slot), true);
        }

        public static SaveData? Read(int slot)
        {
            try
            {
                var p = PathOf(slot);
                if (!File.Exists(p)) return null;
                return JsonSerializer.Deserialize<SaveData>(File.ReadAllText(p), Opt);
            }
            catch { return null; }
        }

        public static void Delete(int slot)
        {
            try { File.Delete(PathOf(slot)); } catch { }
        }

        public static List<object> List()
        {
            var r = new List<object>();
            for (int i = 0; i <= Slots; i++)
            {
                var d = Read(i);
                if (d == null)
                {
                    r.Add(new { slot = i, empty = true, name = i == 0 ? "自動存檔" : "", summary = "", savedAt = "" });
                    continue;
                }
                var s = d.State;
                r.Add(new
                {
                    slot = i,
                    empty = false,
                    name = string.IsNullOrWhiteSpace(d.Name) ? "未命名" : d.Name,
                    summary = $"第 {s.Day} / {GameState.LastDay} 天　{s.Weekday} {s.Clock}　{s.Place}",
                    savedAt = d.SavedAt,
                });
            }
            return r;
        }
    }
}
