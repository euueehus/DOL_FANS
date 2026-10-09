namespace DOL
{
    // ───────── 遊戲狀態 ─────────
    public sealed class GameState
    {
        public const int LastDay = 30;

        public static readonly Dictionary<string, string> FactionName =
            new() { ["trail"] = "開拓", ["school"] = "學園", ["track"] = "賽場" };
        public static readonly Dictionary<string, string> SkillName =
            new() { ["fit"] = "體能", ["social"] = "社交", ["magic"] = "魔術", ["insight"] = "觀察" };
        private static readonly string[] Weekdays = { "週一", "週二", "週三", "週四", "週五", "週六", "週日" };
        private static readonly string[] WeatherTable = { "晴", "晴", "陰", "雨", "晴", "陰", "雨" };

        public int Day = 1, Minutes = 7 * 60, Money = 120, Stamina = 85, Fatigue, Stress = 20, Injury, Will = 60, Seals, Bond;
        public string Scene = "wake", Place = "居住區", Servant = "無", Next = "home", Last = "";
        public bool HasServant;
        public HashSet<string> Flags = new();
        // 好感（0~5）：開拓是星自己人，起點就是 2
        public Dictionary<string, int> Rel = new() { ["trail"] = 2, ["school"] = 0, ["track"] = 0 };
        public Dictionary<string, int> Talks = new() { ["trail"] = 0, ["school"] = 0, ["track"] = 0 };
        public Dictionary<string, int> Skill = new() { ["fit"] = 0, ["social"] = 0, ["magic"] = 0, ["insight"] = 0 };
        public Dictionary<string, int> Items = new() { ["bandage"] = 0 };
        public Dictionary<string, int> Count = new() { ["shop"] = 0, ["cast"] = 0 };
        public List<(string Cls, string Text)> Notes = new();
        public Random Rng = new();

        public string Clock => $"{Minutes / 60:00}:{Minutes % 60:00}";
        public int Hour => Minutes / 60;
        public bool ShopOpen => Hour >= 8 && Hour < 22;
        public string Weekday => Weekdays[(Day - 1) % 7];
        public string Weather => WeatherTable[(Day * 3 + 1) % 7];
        public bool Raining => Weather == "雨";
        public bool Has(string f) => Flags.Contains(f);

        public void Note(string cls, string text) => Notes.Add((cls, text));
        public void AddRel(string k, int n = 1)
        {
            if (Rel[k] >= 5) return;
            Rel[k] = Math.Min(5, Rel[k] + n);
            Note("teal", $"{FactionName[k]}好感 +{n}");
        }
        public void AddBond(int n = 1)
        {
            if (Bond >= 5) return;
            Bond = Math.Min(5, Bond + n);
            Note("teal", $"遐蝶的信賴 +{n}");
        }
        public void Gain(string s, int n)
        {
            Skill[s] = Math.Min(100, Skill[s] + n);
            Note("green", $"{SkillName[s]} +{n}");
        }
        public void Earn(int m) { Money += m; Note("green", $"金錢 +${m}"); }
        public void Hurt(int n) { Injury = Math.Min(100, Injury + n); Note("red", $"傷勢 +{n}"); }
        public void Strain(int n) { Stress = Math.Min(100, Stress + n); Note("red", $"壓力 +{n}"); }
        public void Calm(int n) { Stress = Math.Max(0, Stress - n); Note("green", $"壓力 -{n}"); }

        private void Advance(int minutes)
        {
            Minutes += minutes;
            while (Minutes >= 24 * 60)
            {
                Minutes -= 24 * 60;
                Day++;
                Stamina = Math.Min(100, Stamina + 30);
                Fatigue = Math.Max(0, Fatigue - 30);
                Stress = Math.Max(0, Stress - 5);
                Will = Stress < 40 ? Math.Min(100, Will + 2) : Math.Max(0, Will - 2);
            }
        }

        /// <summary>做事：花時間、花體力。時間本身會慢慢回一點體力，疲勞增加得很慢。</summary>
        public void Pass(int minutes, int stamina = 0)
        {
            minutes = Math.Max(0, minutes);
            Stamina = Math.Max(0, Stamina - stamina);
            Stamina = Math.Min(100, Stamina + minutes / 30);
            Fatigue = Math.Min(100, Fatigue + minutes / 8);
            Advance(minutes);
        }

        public void Rest(int minutes)
        {
            int h = minutes / 60;
            Stamina = Math.Min(100, Stamina + h * 12);
            Fatigue = Math.Max(0, Fatigue - h * 10);
            Stress = Math.Max(0, Stress - h * 2);
            Advance(minutes);
        }

        public void SleepToMorning()
        {
            Rest((7 * 60 - Minutes + 24 * 60) % (24 * 60));
            Injury = Math.Max(0, Injury - 5);
        }

        public void Reset(string characterId, IEnumerable<string> feats)
        {
            var f = feats.ToList();
            Day = 1; Minutes = 7 * 60; Money = 120; Fatigue = 0; Stress = 20;
            Injury = 0; Seals = 0; Bond = 0;
            Scene = "wake"; Place = "居住區"; Servant = "無"; Next = "home"; Last = "";
            HasServant = false;
            Flags.Clear(); Notes.Clear();
            foreach (var k in Rel.Keys.ToList()) Rel[k] = k == "trail" ? 2 : 0;
            foreach (var k in Talks.Keys.ToList()) Talks[k] = 0;
            foreach (var k in Skill.Keys.ToList()) Skill[k] = 0;
            foreach (var k in Items.Keys.ToList()) Items[k] = 0;
            foreach (var k in Count.Keys.ToList()) Count[k] = 0;
            int st = 60, will = 60;
            switch (characterId)
            {
                case "stelle": st = 60; will = 60; break;
                case "march": st = 40; will = 60; break;
                case "sensei": st = 40; will = 80; break;
                case "hina": st = 80; will = 60; break;
                case "trainer": st = 60; will = 80; break;
                case "teio": st = 80; will = 40; break;
                case "herta": st = 20; will = 60; break;
            }
            Stamina = Math.Min(100, st + 25);
            Will = will;
            if (f.Contains("rich")) Money += 200;
            if (f.Contains("tough")) { Stamina = 100; Injury = 0; }
            if (f.Contains("calm")) { Stress = 10; Will = 95; }
        }
    }

    public sealed record Choice(string Id, string Text, string Cost = "");
    public sealed record SceneView(string Passage, List<Choice> Choices);
}
