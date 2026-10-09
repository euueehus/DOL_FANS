namespace DOL
{
    // 序章：刻度塔底下的召喚
    public static partial class Game
    {
        private static bool RenderIntro(GameState g, List<Choice> c, out string p)
        {
            p = "";
            switch (g.Scene)
            {
                case "wake":
                    p = "<p>天還沒全亮，刻度塔先亮了。白光從塔頂一圈圈往下爬，你盯著看了快一分鐘，它沒有要滅的意思。</p><p>手背乾乾淨淨，什麼都沒有。</p>";
                    c.Add(new("noseal", "看看手背"));
                    c.Add(new("tower", "去刻度塔底下看看", "20分"));
                    return true;
                case "noseal":
                    p = "<p>你把手翻過來又翻回去。聖杯選御主會留下令咒，你這邊什麼都沒有，要嘛還沒輪到，要嘛不會輪到。</p><p>塔底離這裡走路二十分鐘。</p>";
                    c.Add(new("tower", "去刻度塔底下看看", "20分"));
                    return true;
                case "rite1":
                    p = "<p>塔底的石環是暗的。六個刻度全停在零，旁邊刻著筋力、耐久、敏捷、魔力、幸運、寶具。環上有人用粉筆畫過線，被鞋底踩得只剩淡淡一層。</p>" +
                        "<p>你蹲下來照著舊線描，一格一格。描到第六格，手背像被燙了一下。</p><p>三道紅紋浮上來，一道都不缺。</p>";
                    c.Add(new("rite2", "退到環外，唸封環咒", "10分"));
                    return true;
                case "rite2":
                    p = "<p>咒文你背得不太順，中間卡了兩次，重唸才接上。</p><p>「以石為基，以銀為引。四方之門閉鎖，循環自此起轉。」</p><p>「聖杯在上。要來，就來一個願意應約的人。」</p><p>環裡起了風，環外一點感覺也沒有。</p>";
                    c.Add(new("rite3", "再唸一遍，把環封上", "10分"));
                    return true;
                case "rite3":
                    p = "<p>六格刻度同時開始轉，轉得很慢。冷氣從環心往上冒，你的袖口被吹得貼住手腕。</p><p>「我在此宣告。願你以劍立於我名之下，我以命運託付於你。」</p><p>環中央的光聚成一個人的高度，還看不出臉。</p>";
                    c.Add(new("rite4", "穩住，繼續唸", "10分"));
                    return true;
                case "rite4":
                    p = "<p>先出現的是裙擺，接著是手，最後才是臉。她站在環內，腳尖停在線前一寸，沒有踩上去。</p><p>刻度停了。人到了，契約還沒立。</p><p>「請問，」她看著你，「召喚我的是您嗎？」</p>";
                    c.Add(new("rite5", "是我", "10分"));
                    return true;
                case "rite5":
                    p = "<p>她低頭看了一眼你的手背，確認紋路之後，才輕輕點頭。</p><p>「Caster，應召而來。」</p><p>有什麼東西從胸口被抽走，沿著看不見的線流向她，不痛，只是有點空。契約成立了。</p>";
                    c.Add(new("rite6", "確認契約", "10分"));
                    return true;
                case "summoned":
                    p = "<p>三道令咒都還在。她的職階是 Caster，真名叫遐蝶。</p><p>「真名請不要對外說，」遐蝶說，聲音很輕。「令咒的事，離開這裡之後我再跟您講。環還在讀契約，現在不適合久留。」</p>";
                    c.Add(new("castorice", "離開環，聽她說令咒", "20分"));
                    c.Add(new("home", "回居住區", "30分"));
                    return true;
                case "castorice":
                    p = "<p>走出塔底，天已經全亮了。遐蝶走在你旁邊，隔著一步的距離，一路都沒有縮短。</p>" +
                        "<p>「令咒一劃，是一次絕對命令。我做得到的事，您都可以逼我做，也可以拿來替您補魔力。不需要咒文，您心裡下令就行。」</p>" +
                        "<p>「三劃用完，您就不再是我的御主了。所以……請留著。」</p>";
                    c.Add(new("home", "回居住區", "30分"));
                    return true;
            }
            return false;
        }

        private static bool StepIntro(GameState g, string id)
        {
            switch (id)
            {
                case "noseal": g.Scene = "noseal"; return true;
                case "tower": g.Scene = "rite1"; g.Place = "刻度塔"; g.Seals = 3; g.Pass(20, 1); return true;
                case "rite2": g.Scene = "rite2"; g.Pass(10); return true;
                case "rite3": g.Scene = "rite3"; g.Pass(10, 1); return true;
                case "rite4": g.Scene = "rite4"; g.Pass(10, 1); return true;
                case "rite5": g.Scene = "rite5"; g.Pass(10, 1); return true;
                case "rite6":
                    g.Scene = "summoned"; g.HasServant = true; g.Servant = "遐蝶"; g.Flags.Add("summoned");
                    g.Pass(10, 2); return true;
                case "castorice": g.Scene = "castorice"; g.Pass(20); return true;
            }
            return false;
        }
    }
}
