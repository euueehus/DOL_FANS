using System.Text;

namespace DOL
{
    // 核心：畫面分派、數值側欄、日誌視窗。劇情文字在 Game.Intro / Daily / Arcs / Main / End。
    public static partial class Game
    {
        private static readonly int[] ChapterDay = { 3, 8, 15, 22 };

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

        private static int ArcDone(GameState g, string k) =>
            Enumerable.Range(1, 3).Count(i => g.Has($"arc_{k}_{i}"));

        private static bool AllArcs(GameState g) => GameState.FactionName.Keys.All(k => ArcDone(g, k) == 3);

        // 真結局：兩條線索、三條支線都走完、觀察 ≥10、意志 ≥40、遐蝶信賴 ≥3
        private static List<string> Missing(GameState g)
        {
            var m = new List<string>();
            if (!g.Has("clueA")) m.Add("學園祭那份舊記錄的內容");
            if (!g.Has("clueB")) m.Add("賽道刻度的真相");
            foreach (var kv in GameState.FactionName)
                if (ArcDone(g, kv.Key) < 3) m.Add($"{kv.Value}那邊的事還沒了結");
            if (g.Skill["insight"] < 10) m.Add("觀察 10 以上（不然看不懂核心）");
            if (g.Will < 40) m.Add("意志 40 以上（撐不住改寫）");
            if (g.Bond < 3) m.Add("遐蝶的信賴 3 以上");
            return m;
        }
        private static bool TrueEnding(GameState g) => Missing(g).Count == 0;

        // ───────── 側欄數值（DoL 式狀態文字）─────────
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
            ["stamina"] = new { value = g.Stamina + " / 100", note = Word(g.Stamina, (20, "快撐不住"), (45, "有點累"), (75, "還行"), (101, "精神很好")), bar = g.Stamina },
            ["fatigue"] = new { value = g.Fatigue + " / 100", note = Word(g.Fatigue, (25, "清醒"), (55, "有點睏"), (85, "很睏"), (101, "累垮了")), bar = g.Fatigue },
            ["stress"] = new { value = g.Stress + " / 100", note = Word(g.Stress, (20, "平靜"), (50, "緊繃"), (80, "焦躁"), (101, "快崩潰")), bar = g.Stress },
            ["injury"] = new { value = g.Injury + " / 100", note = Word(g.Injury, (1, "無傷"), (30, "輕傷"), (60, "重傷"), (101, "危急")), bar = g.Injury },
            ["will"] = new { value = g.Will + " / 100", note = (g.Seals > 0 ? "令咒 " + g.Seals + "　" : "") + Word(g.Will, (40, "動搖"), (70, "穩"), (101, "堅定")), bar = g.Will },
        };

        // ───────── 日誌／角色視窗 ─────────
        public static object Panels(GameState g)
        {
            var j = new StringBuilder();
            j.Append($"<p class=\"muted\">難度：{(g.Difficulty == "easy" ? "簡單" : g.Difficulty == "hard" ? "困難" : "普通")}</p>");
            j.Append("<h3 class=\"gold\">目標</h3>");
            int ch = DueChapter(g);
            j.Append(ch > 0
                ? $"<p>刻度塔那邊有事要處理（第 {ch} 章）。</p>"
                : $"<p>期限前把能做的事做完。剩 {Math.Max(0, GameState.LastDay - g.Day)} 天。</p>");
            j.Append("<h3 class=\"gold\">線索</h3>");
            j.Append($"<p>{Mark(g.Has("clueA"))} 舊記錄　{Mark(g.Has("clueB"))} 賽道刻度</p>");
            j.Append("<h3 class=\"gold\">各方的事</h3><p>");
            j.Append(string.Join("<br>", GameState.FactionName.Select(kv =>
                $"{kv.Value}　好感 {g.Rel[kv.Key]}　進度 {ArcDone(g, kv.Key)}/3")));
            j.Append("</p>");
            if (g.HasServant) j.Append($"<h3 class=\"gold\">遐蝶</h3><p>信賴 {g.Bond} / 5</p>");
            j.Append("<h3 class=\"gold\">想走到最好的結局，需要</h3><p>");
            j.Append($"{Mark(g.Has("clueA") && g.Has("clueB"))} 兩條線索<br>");
            j.Append($"{Mark(AllArcs(g))} 三邊的事都走完<br>");
            j.Append($"{Mark(g.Skill["insight"] >= 10)} 觀察 10 以上<br>");
            j.Append($"{Mark(g.Will >= 40)} 意志 40 以上<br>");
            j.Append($"{Mark(g.Bond >= 3)} 遐蝶的信賴 3 以上</p>");

            var s = new StringBuilder();
            s.Append("<h3 class=\"gold\">能力</h3>");
            foreach (var kv in g.Skill)
                s.Append($"<div class=\"skill\"><span>{GameState.SkillName[kv.Key]}　<span class=\"muted\">{Level(kv.Value)}</span></span><span>{kv.Value}</span>" +
                         $"<div class=\"stat-bar teal\"><i data-value=\"{kv.Value}\"></i></div></div>");
            s.Append("<h3 class=\"gold\">持有</h3>");
            s.Append($"<p>金錢 ${g.Money}　令咒 {g.Seals}　繃帶 {g.Items["bandage"]}</p>");
            return new { journal = j.ToString(), skills = s.ToString() };
        }

        // ───────── 分派 ─────────
        public static SceneView Render(GameState g)
        {
            var c = new List<Choice>();
            string p;
            if (!(RenderIntro(g, c, out p) || RenderDaily(g, c, out p) || RenderArcs(g, c, out p)
                  || RenderMain(g, c, out p) || RenderEnd(g, c, out p)))
            {
                p = "<p>（找不到這個場景：" + g.Scene + "）</p>";
                c.Add(new("home", "回居住區"));
            }
            if (g.Notes.Count > 0)
                p += "<p class=\"notes\">" + string.Join("　", g.Notes.Select(n => $"<span class=\"{n.Cls}\">{n.Text}</span>")) + "</p>";
            return new SceneView(p, c);
        }

        public static void Step(GameState g, string id)
        {
            g.Notes.Clear();
            if (!(StepIntro(g, id) || StepDaily(g, id) || StepArcs(g, id) || StepMain(g, id) || StepEnd(g, id)))
                g.Scene = "home";

            if (g.Day > GameState.LastDay && !g.Scene.StartsWith("end"))
                g.Scene = "end_late";
        }
    }
}
