using System.Text;

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

        public int Day = 1, Minutes = 7 * 60, Money = 120, Stamina = 60, Fatigue, Stress = 20, Injury, Will = 60, Seals;
        public string Scene = "wake", Place = "居住區", Servant = "無", Next = "home", Last = "";
        public bool HasServant;
        public HashSet<string> Flags = new();
        public Dictionary<string, int> Rel = new() { ["trail"] = 0, ["school"] = 0, ["track"] = 0 };
        public Dictionary<string, int> Skill = new() { ["fit"] = 0, ["social"] = 0, ["magic"] = 0, ["insight"] = 0 };
        public Dictionary<string, int> Items = new() { ["bandage"] = 0 };
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
        public void AddRel(string k, int n = 1) { Rel[k] += n; Note("teal", $"{FactionName[k]}好感 +{n}"); }
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
                Stamina = Math.Min(100, Stamina + 20);
                Fatigue = Math.Max(0, Fatigue - 30);
                Stress = Math.Max(0, Stress - 5);
                Will = Stress < 40 ? Math.Min(100, Will + 2) : Math.Max(0, Will - 3);
            }
        }

        public void Pass(int minutes, int stamina = 0)
        {
            minutes = Math.Max(0, minutes);
            Stamina = Math.Max(0, Stamina - stamina);
            Fatigue = Math.Min(100, Fatigue + minutes / 2);
            Advance(minutes);
        }

        public void Rest(int minutes)
        {
            int h = minutes / 60;
            Stamina = Math.Min(100, Stamina + h * 8);
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
            Day = 1; Minutes = 7 * 60; Money = 120; Stamina = 60; Fatigue = 0; Stress = 20;
            Injury = 0; Will = 60; Seals = 0;
            Scene = "wake"; Place = "居住區"; Servant = "無"; Next = "home"; Last = "";
            HasServant = false;
            Flags.Clear(); Notes.Clear();
            foreach (var k in Rel.Keys.ToList()) Rel[k] = 0;
            foreach (var k in Skill.Keys.ToList()) Skill[k] = 0;
            foreach (var k in Items.Keys.ToList()) Items[k] = 0;
            switch (characterId)
            {
                case "stelle": Stamina = 60; Will = 60; break;
                case "march": Stamina = 40; Will = 60; break;
                case "sensei": Stamina = 40; Will = 80; break;
                case "hina": Stamina = 80; Will = 60; break;
                case "trainer": Stamina = 60; Will = 80; break;
                case "teio": Stamina = 80; Will = 40; break;
                case "herta": Stamina = 20; Will = 60; break;
            }
            if (f.Contains("rich")) Money += 200;
            if (f.Contains("tough")) { Stamina = 90; Injury = 0; }
            if (f.Contains("calm")) { Stress = 10; Will = 95; }
        }
    }

    public sealed record Choice(string Id, string Text, string Cost = "");
    public sealed record SceneView(string Passage, List<Choice> Choices);

    public static class Game
    {
        private static readonly int[] ChapterDay = { 3, 8, 15, 22 };

        private static readonly Dictionary<string, string[]> Talk = new()
        {
            ["trail"] = new[]
            {
                "三月七遞給你一罐汽水，說列車組還在查刻度塔的記錄，暫時沒有結論。",
                "丹恆願意多說一點：塔的刻度比他們來之前更亂，像是有人在動。",
                "三月七直接把列車組的調查筆記攤給你。「你不算外人了。」",
            },
            ["school"] = new[]
            {
                "學園的人只問你有沒有在校內亮過令咒。你說沒有，她點頭記下。",
                "日奈肯多聊兩句：學園祭是少數可以讓三邊同時出現的場合。",
                "日奈把社團大樓的鑰匙借給你。「要談事情，這裡比較安靜。」",
            },
            ["track"] = new[]
            {
                "賽道邊的人只看你一眼，說跑道今天不對外開放。",
                "帝王邀你看一場練習。她說比賽之外的事她不插手，但不討厭誠實的人。",
                "帝王把年度大賽的出賽名單給你看。「這份名單，有人想改。」",
            },
        };

        private static string T(int m) => m >= 60 && m % 60 == 0 ? $"{m / 60}小時" : $"{m}分";
        private static string Level(int v) => new[] { "入門", "普通", "熟練", "精通", "頂尖" }[Math.Min(4, v / 20)];
        private static string Mark(bool ok) => ok ? "<span class=\"green\">✔</span>" : "<span class=\"red\">✘</span>";

        private static int DueChapter(GameState g)
        {
            for (int n = 1; n <= 4; n++)
            {
                if (g.Has("ch" + n)) continue;
                return g.Day >= ChapterDay[n - 1] ? n : 0;
            }
            return 0;
        }

        // 真結局條件：三邊好感 ≥1、兩條線索、觀察 ≥10、意志 ≥40
        private static List<string> Missing(GameState g)
        {
            var m = new List<string>();
            if (!g.Has("clueA")) m.Add("線索 A（學園祭的舊記錄）");
            if (!g.Has("clueB")) m.Add("線索 B（賽場的刻度）");
            foreach (var kv in g.Rel) if (kv.Value < 1) m.Add($"{GameState.FactionName[kv.Key]}的信任");
            if (g.Skill["insight"] < 10) m.Add("觀察 10 以上（才讀得懂核心）");
            if (g.Will < 40) m.Add("意志 40 以上（撐得住改寫）");
            return m;
        }
        private static bool TrueEnding(GameState g) => Missing(g).Count == 0;

        // ───────── 側欄數值（含 DoL 式狀態文字）─────────
        private static string Word(int v, params (int Below, string Text)[] steps)
        {
            foreach (var (b, t) in steps) if (v < b) return t;
            return steps[^1].Text;
        }

        public static Dictionary<string, object> Hud(GameState g) => new()
        {
            ["time"] = new { value = g.Clock, note = $"{g.Weekday}　第 {g.Day} / {GameState.LastDay} 天　{g.Weather}", bar = 0 },
            ["location"] = new { value = g.Place, note = g.HasServant ? "Servant：" + g.Servant : "", bar = 0 },
            ["money"] = new { value = "$" + g.Money, note = "", bar = 0 },
            ["stamina"] = new { value = g.Stamina + " / 100", note = Word(g.Stamina, (25, "快撐不住"), (50, "疲憊"), (80, "尚可"), (101, "充沛")), bar = g.Stamina },
            ["fatigue"] = new { value = g.Fatigue + " / 100", note = Word(g.Fatigue, (20, "清醒"), (50, "有點累"), (80, "睏"), (101, "累垮了")), bar = g.Fatigue },
            ["stress"] = new { value = g.Stress + " / 100", note = Word(g.Stress, (20, "平靜"), (50, "緊繃"), (80, "焦躁"), (101, "快崩潰")), bar = g.Stress },
            ["injury"] = new { value = g.Injury + " / 100", note = Word(g.Injury, (1, "無傷"), (30, "輕傷"), (60, "重傷"), (101, "危急")), bar = g.Injury },
            ["will"] = new { value = g.Will + " / 100", note = (g.Seals > 0 ? "令咒 " + g.Seals + "　" : "") + Word(g.Will, (40, "動搖"), (70, "穩"), (101, "堅定")), bar = g.Will },
        };

        // ───────── 日誌／角色視窗 ─────────
        public static object Panels(GameState g)
        {
            var j = new StringBuilder();
            j.Append("<h3 class=\"gold\">目標</h3>");
            int ch = DueChapter(g);
            j.Append(ch > 0
                ? $"<p>刻度塔有事要處理（第 {ch} 章）。</p>"
                : $"<p>在期限前累積信任、線索與能力。剩 {Math.Max(0, GameState.LastDay - g.Day)} 天。</p>");
            j.Append("<h3 class=\"gold\">線索</h3>");
            j.Append($"<p>{Mark(g.Has("clueA"))} 線索 A　{Mark(g.Has("clueB"))} 線索 B</p>");
            j.Append("<h3 class=\"gold\">關係</h3>");
            j.Append("<p>" + string.Join("　", g.Rel.Select(kv => $"{GameState.FactionName[kv.Key]} {kv.Value}")) + "</p>");
            j.Append("<h3 class=\"gold\">真結局條件</h3><p>");
            j.Append($"{Mark(g.Has("clueA") && g.Has("clueB"))} 兩條線索<br>");
            j.Append($"{Mark(g.Rel.Values.All(v => v >= 1))} 三邊好感都至少 1<br>");
            j.Append($"{Mark(g.Skill["insight"] >= 10)} 觀察 10 以上<br>");
            j.Append($"{Mark(g.Will >= 40)} 意志 40 以上</p>");

            var s = new StringBuilder();
            s.Append("<h3 class=\"gold\">能力</h3>");
            foreach (var kv in g.Skill)
                s.Append($"<div class=\"skill\"><span>{GameState.SkillName[kv.Key]}　<span class=\"muted\">{Level(kv.Value)}</span></span><span>{kv.Value}</span>" +
                         $"<div class=\"stat-bar teal\"><i data-value=\"{kv.Value}\"></i></div></div>");
            s.Append("<h3 class=\"gold\">持有</h3>");
            s.Append($"<p>金錢 ${g.Money}　令咒 {g.Seals}　繃帶 {g.Items["bandage"]}</p>");
            return new { journal = j.ToString(), skills = s.ToString() };
        }

        // ───────── 畫面 ─────────
        public static SceneView Render(GameState g)
        {
            var c = new List<Choice>();
            string p;
            switch (g.Scene)
            {
                // ── 序章 ──
                case "wake":
                    p = "<p>刻度塔亮起。你是星。手背沒有印記，聖杯還沒有選中你，身邊也沒有 Servant。</p><p>去塔底，等聖杯回應。</p>";
                    c.Add(new("noseal", "確認手背"));
                    c.Add(new("tower", "前往刻度塔佈陣", "20分"));
                    break;
                case "noseal":
                    p = "<p>手背沒有印記。令咒是聖杯選中御主的證明，聖杯還沒有選你。</p>";
                    c.Add(new("tower", "前往刻度塔", "20分"));
                    break;
                case "rite1":
                    p = "<p>塔底的環是暗的。六格刻度停在零：筋力、耐久、敏捷、魔力、幸運、寶具。</p><p>你沿舊線把環描亮，用的是自己的魔力。環亮起的瞬間，手背燒了起來。聖杯選中了你，三道令咒浮現，一道不缺。</p>";
                    c.Add(new("rite2", "退到環外，開始詠唱", "10分"));
                    break;
                case "rite2":
                    p = "<p>環心是空的。召喚要先把邊界說清楚。</p><p>「以石為基，以銀為引。四方之門閉鎖，循環自此起轉。」</p><p>「聖杯在上。要來，就來一個願意應約的人。」</p>";
                    c.Add(new("rite3", "第二遍詠唱，把環封上", "10分"));
                    break;
                case "rite3":
                    p = "<p>六格同時轉。冷氣從環外壓進來，像門後有重量，還沒有形狀。</p><p>「我在此宣告。願你以劍立於我名之下，我以命運託付於你。」</p><p>光開始聚成人形。還不是臉。</p>";
                    c.Add(new("rite4", "維持詠唱，等輪廓", "10分"));
                    break;
                case "rite4":
                    p = "<p>裙襬先從光裡落下，然後是手，然後是眼睛。她站在環內，沒有踏線。</p><p>刻度停住。英靈已經現身，契約還沒有成立。</p><p>她看向你：「問你。你就是召喚我的 Master 嗎？」</p>";
                    c.Add(new("rite5", "回答：是", "10分"));
                    break;
                case "rite5":
                    p = "<p>她看了一眼你手背的令咒，像在確認聖杯的選擇沒有錯。</p><p>「Servant，Caster。應召而來。」</p><p>魔力沿著看不見的線從你流向她。契約成立。</p>";
                    c.Add(new("rite6", "確認契約", "10分"));
                    break;
                case "summoned":
                    p = "<p>令咒沒有變化，三道都在。職階 Caster，真名遐蝶。</p><p>「真名不要對外說。」遐蝶說。「令咒是對我的強制，用一道就少一道。離開環吧，塔還在讀這份契約。」</p>";
                    c.Add(new("castorice", "離開環，問令咒的用法", "20分"));
                    c.Add(new("home", "回居住區", "30分"));
                    break;
                case "castorice":
                    p = "<p>遐蝶看著你手背的三道。</p><p>「一劃，一次絕對命令。可以強迫我做得到的事，也可以拿來強化、補魔。不用咒文。你下令，它就燒一劃。」</p><p>「三劃都用完，你就不再是能命令我的御主。所以不要試。」</p>";
                    c.Add(new("home", "回居住區", "30分"));
                    break;

                // ── 日常中樞 ──
                case "home":
                    {
                        var sb = new StringBuilder();
                        sb.Append($"<p>居住區。{g.Weekday}，{g.Clock}，{g.Weather}。距離期限還有 {Math.Max(0, GameState.LastDay - g.Day)} 天。</p>");
                        int ch = DueChapter(g);
                        if (ch > 0) sb.Append("<p class=\"gold\">刻度塔的環在發亮，有事要處理。</p>");
                        if (g.Stamina < 15) sb.Append("<p>你累得走不動，先休息吧。</p>");
                        if (g.Injury >= 50) sb.Append("<p class=\"red\">傷得不輕，不適合劇烈活動。</p>");
                        p = sb.ToString();
                        if (ch > 0) c.Add(new("ch" + ch, $"前往刻度塔（第 {ch} 章）", "30分"));
                        if (g.Stamina >= 5)
                        {
                            c.Add(new("go_trail", "去開拓列車站", "30分"));
                            c.Add(new("go_school", "去學園都市", "30分"));
                            c.Add(new("go_track", "去賽場", "30分"));
                        }
                        if (g.ShopOpen && g.Stamina >= 15) c.Add(new("shop", "去便利商店打工", "2小時"));
                        if (g.HasServant && g.Stamina >= 5 && !g.Has($"med:{g.Day}")) c.Add(new("meditate", "請遐蝶指點魔力", "1小時"));
                        if (g.Items["bandage"] > 0 && g.Injury > 0) c.Add(new("bandage", "用繃帶處理傷口"));
                        if (g.Seals > 0 && g.Stamina < 40) c.Add(new("seal", "動用一道令咒補魔"));
                        c.Add(new("wait", "等一下", "1小時"));
                        c.Add(new("rest", "休息", "2小時"));
                        if (g.Hour >= 20 || g.Hour < 6) c.Add(new("sleep", "睡到早上"));
                    }
                    break;
                case "rested": p = "<p>居住區很安靜。體力回來一點。</p>"; c.Add(new("home", "站起來")); break;
                case "slept": p = $"<p>醒來。第 {g.Day} 天，{g.Clock}，{g.Weather}。傷勢退了一點。</p>"; c.Add(new("home", "起床")); break;
                case "meditated":
                    p = "<p>遐蝶坐在對面，用指尖在桌面上畫出一條線。「魔力不是用力，是順。」</p><p>你照做了一小時。呼吸慢下來。</p>";
                    c.Add(new("home", "結束")); break;
                case "bandaged": p = "<p>你把傷口包好。比剛才好多了。</p>"; c.Add(new("home", "收好")); break;
                case "sealused":
                    p = $"<p>手背一熱，魔力被補回來。遐蝶說這種用法不值得常用。剩 {g.Seals} 道令咒。</p>";
                    c.Add(new("home", "收回")); break;
                case "shop":
                    p = "<p>便利商店一班結束。錢進帳。</p>";
                    if (g.Money >= 30) c.Add(new("snack", "買點吃的", "$30"));
                    if (g.Money >= 50) c.Add(new("buy_bandage", "買繃帶", "$50"));
                    if (g.ShopOpen && g.Stamina >= 15) c.Add(new("shop", "再打一班", "2小時"));
                    c.Add(new("home", "回居住區", "30分"));
                    break;
                case "snack": p = "<p>熱食下肚，體力回來一些。</p>"; c.Add(new("shop", "再看看")); c.Add(new("home", "回居住區", "30分")); break;
                case "bought": p = "<p>繃帶放進包裡。</p>"; c.Add(new("shop", "再看看")); c.Add(new("home", "回居住區", "30分")); break;
                case "closed": p = "<p>便利商店這時段沒開。營業時間 8:00 到 22:00。</p>"; c.Add(new("home", "回居住區")); break;

                // ── 據點 ──
                case "loc_trail":
                case "loc_school":
                case "loc_track":
                    {
                        string k = g.Scene[4..];
                        bool metWait = k == "trail" && !g.Has("met");
                        p = k switch
                        {
                            "trail" => "<p>開拓列車站沒有列車進站。公告只寫：刻度塔異動。</p>",
                            "school" => "<p>學園都市。社團大樓的燈亮著，門口站著人。</p>",
                            _ => g.Raining ? "<p>下雨，跑道封閉，看台上沒有人。</p>" : "<p>賽場。跑道上有人在做熱身，看台空著。</p>",
                        };
                        if (metWait)
                        {
                            p += "<p>月台邊有人在拍照。她手背上的印記，跟你的是同一種。</p>";
                            c.Add(new("meet", "走過去", "20分"));
                        }
                        else
                        {
                            if (!g.Has($"talk:{k}:{g.Day}") && g.Stamina >= 10)
                                c.Add(new("talk_" + k, $"找{GameState.FactionName[k]}的人聊聊", "1小時"));
                            if (k == "trail" && g.Stamina >= 10) c.Add(new("work_trail", "幫列車組跑腿", "1小時"));
                            if (k == "school")
                            {
                                if (g.Stamina >= 12) c.Add(new("work_school", "幫社團整理資料", "2小時"));
                                if (!g.Has($"study:{g.Day}") && g.Stamina >= 12) c.Add(new("study_school", "到圖書館查塔的記錄", "90分"));
                            }
                            if (k == "track" && !g.Raining)
                            {
                                if (g.Stamina >= 25) c.Add(new("work_track", "維護跑道、搬器材", "2小時"));
                                if (!g.Has($"train:{g.Day}") && g.Stamina >= 25 && g.Injury < 50) c.Add(new("train_track", "在跑道上訓練", "1小時"));
                            }
                        }
                        c.Add(new("home", "回居住區", "30分"));
                    }
                    break;
                case "talk_trail":
                case "talk_school":
                case "talk_track":
                    {
                        string k = g.Scene[5..];
                        p = $"<p>{Talk[k][Math.Clamp(g.Rel[k], 1, 3) - 1]}</p>";
                        c.Add(new("loc_" + k, "留在這裡"));
                        c.Add(new("home", "回居住區", "30分"));
                    }
                    break;
                case "worked":
                    p = g.Last switch
                    {
                        "trail" => "<p>你替列車組跑了幾趟腿，三月七塞給你一點零用錢。</p>",
                        "school" => "<p>整理了一下午資料，社團的人記下你的名字。</p>",
                        _ => "<p>搬完器材，肩膀痠，但錢到手。</p>",
                    };
                    c.Add(new("loc_" + g.Last, "回到據點"));
                    c.Add(new("home", "回居住區", "30分"));
                    break;
                case "studied":
                    p = "<p>圖書館角落有一疊舊記錄。大半是塔的維修單，但有幾頁寫到六格刻度各自在抽什麼。</p>";
                    c.Add(new("loc_school", "闔上書"));
                    break;
                case "trained":
                    p = "<p>你沿著跑道練了一小時。心跳慢慢和步伐對上。</p>";
                    c.Add(new("loc_track", "收操"));
                    break;
                case "meet":
                    p = "<p>拍照的人先開口：「你也有？我叫三月七。這是丹恆。」</p><p>丹恆站在她側後方。他看了一眼遐蝶，沒有問真名。</p><p>三月七舉起手背：「今天早上才出現的。我們也不知道塔為什麼亮。」</p>";
                    c.Add(new("ally", "說你們也是剛完成召喚", "20分"));
                    c.Add(new("decline", "只聽，不表明身分", "10分"));
                    break;
                case "ally":
                    p = "<p>遐蝶沒有報職階。三月七也沒有逼。</p><p>丹恆說：「公告之外還有一條。七組。塔會繼續叫人，叫滿為止。」</p><p>「令咒不要在街上亮。」</p>";
                    c.Add(new("loc_trail", "留在月台"));
                    break;
                case "decline":
                    p = "<p>三月七把相機放下。「行。那我們當沒見過。」</p><p>丹恆經過時只留一句：「令咒不要對著路人。」</p>";
                    c.Add(new("loc_trail", "留在月台"));
                    break;

                // ── 隨機事件 ──
                case "ev_found":
                    p = "<p>路邊的長椅下壓著一個信封，裡面是別人掉的零錢。附近沒有人認領。</p>";
                    c.Add(new("evcont", "收下，繼續走")); break;
                case "ev_tip":
                    p = "<p>一個路過的老人叫住你，隨口說起刻度塔以前每逢整點會響一次，這陣子卻沒有了。</p>";
                    c.Add(new("evcont", "道謝，繼續走")); break;
                case "ev_scuffle":
                    p = "<p>巷口有兩個人在推擠，其中一個撞到了你。對方回頭，一臉不爽。</p>";
                    c.Add(new("evfight", "不退讓，頂回去"));
                    c.Add(new("evavoid", "道歉，繞開", "10分")); break;
                case "ev_win":
                    p = "<p>你沒退。對方掂了掂你的分量，罵了一句就走了。</p>";
                    c.Add(new("evcont", "繼續走")); break;
                case "ev_lose":
                    p = "<p>你吃了一拳，對方趁機跑掉。遐蝶沒有出手，只是看著你。</p>";
                    c.Add(new("evcont", "繼續走")); break;
                case "ev_avoid":
                    p = "<p>你點頭道歉，從旁邊繞開。背後的罵聲很快遠了。</p>";
                    c.Add(new("evcont", "繼續走")); break;

                // ── 主線章節 ──
                case "ch1":
                    p = "<p>第一章。三方試探。</p><p>居住區門縫塞進兩張紙。一張蓋學園戳，一張是賽程表。都沒有署名。</p>";
                    c.Add(new("ch1_school", "先看學園那張", "10分"));
                    c.Add(new("ch1_track", "先看賽程表", "10分"));
                    c.Add(new("ch1_neutral", "兩張都收起來", "10分"));
                    break;
                case "ch1_school": p = "<p>學園戳下面只有一句：校園裡不要開令咒。要談，學園祭前來社團大樓。</p>"; c.Add(new("home", "收起紙")); break;
                case "ch1_track": p = "<p>賽程表圈了年度大賽的日子。空白處寫：賽道不是戰場，除非你們把它變成戰場。</p>"; c.Add(new("home", "收起紙")); break;
                case "ch1_neutral": p = "<p>兩張都沒回。遐蝶說這樣也行，三邊都會把你記成未表態。</p>"; c.Add(new("home", "收起紙")); break;
                case "ch2":
                    p = "<p>第二章。學園祭。</p><p>廣場在辦祭典。暗處有人把刻度塔的舊記錄攤在攤位後面，不給看第二頁。</p>";
                    c.Add(new("ch2_read", "看那一頁", "30分"));
                    c.Add(new("ch2_skip", "只參加祭典", "30分"));
                    break;
                case "ch2_read": p = "<p>記錄只寫到半句：願望實現之後，城裡會有一塊區域消失。</p><p>這是線索 A。遐蝶沒有解釋消失的是哪一區。</p>"; c.Add(new("home", "離開廣場", "30分")); break;
                case "ch2_skip": p = "<p>你沒有看記錄。祭典結束時，那一頁已經不在了。</p><p>線索 A 沒有拿到。</p>"; c.Add(new("home", "離開廣場", "30分")); break;
                case "ch3":
                    p = "<p>第三章。賽場決戰。</p><p>年度大賽和聖杯戰爭撞在同一天。賽道邊的刻度在轉，像在抽走上頭的速度。</p>";
                    c.Add(new("ch3_race", "先把比賽跑完", "1小時"));
                    c.Add(new("ch3_fight", "先處理賽道上的刻度", "1小時"));
                    if (g.Seals > 0) c.Add(new("ch3_seal", "令咒一劃，讓遐蝶壓住刻度"));
                    break;
                case "ch3_race": p = "<p>比賽先結束。刻度沒有停。遐蝶說聖杯在吸力量，所以英靈才會被降格。</p><p>這是線索 B。</p>"; c.Add(new("home", "離開賽道", "30分")); break;
                case "ch3_fight": p = "<p>刻度被打斷一格。比賽中止，你挨了一下。吸力還在，只是慢了。</p><p>線索 B 一樣：降格是因為聖杯在吸收力量。</p>"; c.Add(new("home", "離開賽道", "30分")); break;
                case "ch3_seal": p = "<p>你對遐蝶下令。刻度被壓住，賽道上沒有人受傷。</p><p>代價是手背少了一道。線索 B 一樣：降格是因為聖杯在吸收力量。</p>"; c.Add(new("home", "離開賽道", "30分")); break;
                case "ch4":
                    p = "<p>第四章。背叛之夜。</p><p>三邊對聖杯的處置分裂。沒有人要求你報真名。他們只要你站邊。</p>";
                    if (g.Rel["trail"] >= 2) c.Add(new("end_a", "站開拓：摧毀聖杯"));
                    if (g.Rel["school"] >= 2) c.Add(new("end_b", "站學園：留下並改寫"));
                    if (g.Rel["track"] >= 2) c.Add(new("end_c", "站賽場：用比賽決定願望"));
                    if (c.Count == 0) p += "<p>你和哪一邊都還不夠熟，沒有人把你當自己人。</p>";
                    c.Add(new("ch5", "哪邊都不站，去塔頂"));
                    break;
                case "ch5":
                    {
                        p = "<p>第五章。刻度歸零。</p><p>塔頂的核心不是許願杯。遐蝶確認它在把願望轉成城市能源。</p>";
                        var miss = Missing(g);
                        if (miss.Count == 0)
                        {
                            p += "<p>三邊的說法你都聽過，線索湊齊，你也讀得懂這顆核心。有一條路可以走。</p>";
                            c.Add(new("end_d", "改寫規則"));
                        }
                        else
                        {
                            p += "<p>你想改寫它，但還差：</p><p class=\"red\">" + string.Join("<br>", miss) + "</p>";
                        }
                        c.Add(new("end_a", "仍選擇摧毀"));
                    }
                    break;

                // ── 結局 ──
                case "end_a": p = "<p>守護結局。聖杯毀掉。城市失去那台回收裝置供的能源。人還在。</p>"; c.Add(new("restart", "重新開始")); break;
                case "end_b": p = "<p>研究結局。聖杯留下，改成公開的許願系統。誰能許，還沒有寫進規則。</p>"; c.Add(new("restart", "重新開始")); break;
                case "end_c": p = "<p>奔跑結局。願望歸屬改由賽事決定。聖杯還在。</p>"; c.Add(new("restart", "重新開始")); break;
                case "end_d": p = "<p>真結局。核心被改寫。願望不再換成某一區的消失，改成要由還在城裡的人一起承擔。</p>"; c.Add(new("restart", "重新開始")); break;
                case "end_late": p = "<p>期限已過。刻度塔沒有等你，其他人替聖杯做了決定。</p>"; c.Add(new("restart", "重新開始")); break;

                default:
                    p = "<p>（找不到這個場景：" + g.Scene + "）</p>";
                    c.Add(new("home", "回居住區"));
                    break;
            }

            if (g.Notes.Count > 0)
                p += "<p class=\"notes\">" + string.Join("　", g.Notes.Select(n => $"<span class=\"{n.Cls}\">{n.Text}</span>")) + "</p>";
            return new SceneView(p, c);
        }

        // ───────── 狀態變化 ─────────
        private static void MaybeEvent(GameState g, string next)
        {
            if (!g.Has("summoned") || g.Rng.Next(100) >= 30) return;
            g.Next = next;
            switch (g.Rng.Next(3))
            {
                case 0: g.Earn(g.Rng.Next(15, 46)); g.Scene = "ev_found"; break;
                case 1: g.Scene = "ev_scuffle"; break;
                default: g.Gain("insight", 1); g.Scene = "ev_tip"; break;
            }
            g.Pass(10);
        }

        private static void Go(GameState g, string k, string place)
        {
            g.Scene = "loc_" + k;
            g.Place = place;
            g.Pass(30, g.Raining ? 8 : 5);
            MaybeEvent(g, "loc_" + k);
        }

        public static void Step(GameState g, string id)
        {
            g.Notes.Clear();
            switch (id)
            {
                // 序章
                case "noseal": g.Scene = "noseal"; break;
                case "tower": g.Scene = "rite1"; g.Place = "刻度塔"; g.Seals = 3; g.Pass(20, 5); break;
                case "rite2": g.Scene = "rite2"; g.Pass(10); break;
                case "rite3": g.Scene = "rite3"; g.Pass(10, 5); break;
                case "rite4": g.Scene = "rite4"; g.Pass(10, 5); break;
                case "rite5": g.Scene = "rite5"; g.Pass(10, 5); break;
                case "rite6":
                    g.Scene = "summoned"; g.HasServant = true; g.Servant = "遐蝶"; g.Flags.Add("summoned");
                    g.Pass(10, 5); break;
                case "castorice": g.Scene = "castorice"; g.Pass(20, 5); break;

                // 日常
                case "home":
                    if (g.Place != "居住區") g.Pass(30, g.Raining ? 5 : 3);
                    g.Place = "居住區"; g.Scene = "home"; break;
                case "wait": g.Rest(60); g.Scene = "home"; break;
                case "rest": g.Rest(120); g.Place = "居住區"; g.Scene = "rested"; break;
                case "sleep": g.SleepToMorning(); g.Place = "居住區"; g.Scene = "slept"; break;
                case "meditate":
                    g.Flags.Add($"med:{g.Day}");
                    g.Pass(60, 5); g.Gain("magic", 2); g.Calm(10);
                    g.Will = Math.Min(100, g.Will + 3); g.Note("green", "意志 +3");
                    g.Scene = "meditated"; break;
                case "bandage":
                    if (g.Items["bandage"] > 0 && g.Injury > 0)
                    {
                        g.Items["bandage"]--; g.Injury = Math.Max(0, g.Injury - 25); g.Note("green", "傷勢 -25");
                    }
                    g.Scene = "bandaged"; break;
                case "seal":
                    if (g.Seals > 0) { g.Seals--; g.Stamina = Math.Min(100, g.Stamina + 30); g.Note("red", "令咒 -1"); }
                    g.Scene = "sealused"; break;
                case "shop":
                    if (!g.ShopOpen) { g.Scene = "closed"; break; }
                    g.Scene = "shop"; g.Place = "便利商店"; g.Pass(120, 15); g.Earn(40);
                    MaybeEvent(g, "shop"); break;
                case "snack":
                    if (g.Money >= 30) { g.Money -= 30; g.Stamina = Math.Min(100, g.Stamina + 15); g.Pass(10); g.Note("red", "金錢 -$30"); }
                    g.Scene = "snack"; break;
                case "buy_bandage":
                    if (g.Money >= 50) { g.Money -= 50; g.Items["bandage"]++; g.Note("red", "金錢 -$50"); g.Note("green", "繃帶 +1"); }
                    g.Scene = "bought"; break;

                // 據點
                case "go_trail": Go(g, "trail", "開拓列車站"); break;
                case "go_school": Go(g, "school", "學園都市"); break;
                case "go_track": Go(g, "track", "賽場"); break;
                case "loc_trail": case "loc_school": case "loc_track": g.Scene = id; break;
                case "talk_trail":
                case "talk_school":
                case "talk_track":
                    {
                        string k = id[5..];
                        g.Flags.Add($"talk:{k}:{g.Day}");
                        g.Pass(60, 10); g.AddRel(k); g.Gain("social", 1);
                        g.Scene = id;
                    }
                    break;
                case "work_trail": g.Last = "trail"; g.Pass(60, 10); g.Earn(20); g.Gain("social", 1); g.Scene = "worked"; break;
                case "work_school": g.Last = "school"; g.Pass(120, 12); g.Earn(45); g.Gain("insight", 1); g.Scene = "worked"; break;
                case "work_track": g.Last = "track"; g.Pass(120, 25); g.Earn(60); g.Gain("fit", 2); g.Scene = "worked"; break;
                case "study_school":
                    g.Flags.Add($"study:{g.Day}");
                    g.Pass(90, 12); g.Gain("insight", 3); g.Gain("magic", 1); g.Scene = "studied"; break;
                case "train_track":
                    g.Flags.Add($"train:{g.Day}");
                    g.Pass(60, 20); g.Gain("fit", 3); g.Scene = "trained"; break;
                case "meet": g.Scene = "meet"; g.Pass(20, 5); break;
                case "ally": g.Flags.Add("met"); g.AddRel("trail"); g.Scene = "ally"; g.Pass(20); break;
                case "decline": g.Flags.Add("met"); g.Scene = "decline"; g.Pass(10); break;

                // 隨機事件
                case "evcont": g.Scene = g.Next; break;
                case "evfight":
                    if (g.Rng.Next(100) < 40 + g.Skill["fit"] / 2) { g.Gain("fit", 1); g.Scene = "ev_win"; }
                    else { g.Hurt(8); g.Strain(8); g.Scene = "ev_lose"; }
                    g.Pass(10); break;
                case "evavoid": g.Strain(3); g.Pass(10); g.Scene = "ev_avoid"; break;

                // 主線
                case "ch1": g.Scene = "ch1"; g.Place = "居住區"; g.Pass(10); break;
                case "ch1_school": g.Flags.Add("ch1"); g.AddRel("school"); g.Scene = id; g.Pass(10); break;
                case "ch1_track": g.Flags.Add("ch1"); g.AddRel("track"); g.Scene = id; g.Pass(10); break;
                case "ch1_neutral": g.Flags.Add("ch1"); g.Scene = id; g.Pass(10); break;
                case "ch2": g.Scene = "ch2"; g.Place = "學園祭廣場"; g.Pass(30, 8); break;
                case "ch2_read": g.Flags.Add("ch2"); g.Flags.Add("clueA"); g.Gain("insight", 2); g.Scene = id; g.Pass(30); break;
                case "ch2_skip": g.Flags.Add("ch2"); g.Scene = id; g.Pass(30); break;
                case "ch3": g.Scene = "ch3"; g.Place = "賽道"; g.Pass(60, 15); break;
                case "ch3_race": g.Flags.Add("ch3"); g.Flags.Add("clueB"); g.AddRel("track"); g.Scene = id; g.Pass(30, 5); break;
                case "ch3_fight":
                    g.Flags.Add("ch3"); g.Flags.Add("clueB"); g.AddRel("trail");
                    g.Hurt(g.Skill["fit"] >= 20 ? 5 : 10); g.Scene = id; g.Pass(30, 10); break;
                case "ch3_seal":
                    if (g.Seals > 0) g.Seals--;
                    g.Note("red", "令咒 -1");
                    g.Flags.Add("ch3"); g.Flags.Add("clueB"); g.AddRel("trail"); g.AddRel("track");
                    g.Scene = id; g.Pass(30, 5); break;
                case "ch4": g.Flags.Add("ch4"); g.Scene = "ch4"; g.Place = "刻度塔"; g.Pass(40, 10); break;
                case "ch5": g.Scene = "ch5"; g.Place = "刻度塔"; g.Pass(30, 10); break;

                // 結局
                case "end_d": g.Scene = TrueEnding(g) ? "end_d" : "ch5"; break;
                case "end_a": case "end_b": case "end_c": g.Scene = id; break;

                default: g.Scene = "home"; break;
            }

            if (g.Day > GameState.LastDay && !g.Scene.StartsWith("end"))
                g.Scene = "end_late";
        }
    }
}