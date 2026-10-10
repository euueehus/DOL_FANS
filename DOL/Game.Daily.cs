using System.Text;

namespace DOL
{
    // 日常：居住區、打工、據點、聊天、隨機事件
    public static partial class Game
    {
        private static readonly Dictionary<string, string[]> Talk = new()
        {
            ["trail"] = new[]
            {
                "三月七在整理相機裡的照片，說她這幾天把刻度塔拍了十幾張。「看起來都一樣啊。可是我就是覺得哪裡怪，你說怪不怪？」",
                "丹恆在車廂外面擦他那把長柄的東西，看你過來才停手。「塔夜裡會跳一格，我數過，每次都在三點多。」說完他就繼續擦，沒有要多講的意思。",
                "三月七丟了罐汽水給你。「我剛跟姬子吵了一架，她說不准我一個人去塔那邊。我說我又不是小孩，她說我是。」她把拉環拉得喀一聲，「我真的不是。」",
                "姬子在車廂裡對著一張手畫的地圖發呆，上面標了七個點，只有三個畫了圈。「剩下四組我還沒找到。」她的語氣像在算一題不太想算的數學。",
                "丹恆難得先開口：「要是哪天我們得對上，我先說，我不會放水。」過了一下他又補一句：「你也別放。」",
            },
            ["school"] = new[]
            {
                "日奈坐在風紀委員會的辦公桌後面批文件，頭也沒抬。「有事嗎？……沒事就坐，椅子是空的。」她看起來比昨天更睏。",
                "「學園祭的攤位申請，一天收到七十幾張，」日奈揉著眉心，「我已經不在乎有沒有人想在祭典上賣炸彈麵包了。」她停了一下，「那只是比喻。應該。」",
                "日奈給你倒了杯茶，茶包還泡在裡面。「學園這邊對聖杯的看法很多，吵來吵去，結論大概是留著比砸掉省事。我不想用省事這個詞決定事情。」",
                "走廊上有人在貼學園祭的流程表，日奈順手把歪掉的那張扶正。「你知道最麻煩的是什麼嗎？不是敵人，是每個人都覺得自己是對的。」",
                "日奈把一張摺過的便條推給你。「我下午要開會，要是會上有人提到你，不用生氣，我會幫你說話。……大概。」",
            },
            ["track"] = new[]
            {
                "帝王在跑道邊拉筋，一看到你就用手指著你：「你！看起來很有潛力！要不要跟本帝王跑一圈？」說完才注意到你根本沒穿跑鞋。",
                "帝王一邊灌運動飲料一邊抱怨：「這幾天腳特別重，像有人在腳踝上掛東西。教練說是心理作用，我才不信。」",
                "「你知道賽道為什麼是圓的嗎？」帝王自問自答：「因為不管跑多遠，最後都得回到起點，再跑一次。所以我不怕輸，輸了再跑就好！」她說得很大聲，周圍的人都回頭看。",
                "帝王蹲在看台底下綁鞋帶，綁得很慢。「我跟你說個祕密，我比賽前其實會緊張，緊張到想吐。不要告訴別人喔，本帝王的形象很重要。」",
                "帝王把年度大賽的參賽名單拿給你，指著其中一格。那一欄是空的。「這裡以前有一個人的名字。你看，她不見了。」",
            },
        };

        private static readonly string[] CastTalk =
        {
            "<p>你問她在被召喚之前都在做什麼。遐蝶想了一下。「很安靜的日子，」她說，「我習慣一個人待著，沒有什麼人會靠近。」</p><p>她看了你一眼，又補一句：「所以現在這樣，我不太知道該站多近。」</p>",
            "<p>你泡了兩杯茶，她接過去的時候很小心，手指沒有碰到你的。「謝謝。」她捧著杯子看了很久，沒有喝。</p><p>「燙嗎？」你問。</p><p>「不是，」她說，「只是很久沒有人拿東西給我了。」</p>",
            "<p>遐蝶站在窗邊看刻度塔。「聖杯給了我一個願望，」她說，「我自己都沒想清楚是什麼。直到被召喚到這裡，才有一點眉目。」</p><p>「是什麼？」</p><p>她搖頭。「還不能說。等我確定了。」</p>",
            "<p>你問她會不會怕令咒。遐蝶轉頭看你，過了一會兒才答：「怕。但我更怕的是，您其實不想用，卻因為我而不得不用。」</p><p>「我不會逼你，也不會逼我自己。」你說。她輕輕點頭，肩膀放鬆了一點。</p>",
            "<p>半夜你睡不著，起來看見她坐在陽台。她回頭，說：「塔的光，今天比昨天暗了一點。」</p><p>「你看得出來？」</p><p>「我住的地方一直很暗，所以對光比較敏感。」她說得很平靜，像在講天氣。「不用擔心。至少在我身邊，您不會有事。」</p>",
            "<p>你們在陽台上坐著，誰也沒說話。遠處的塔暗了一格，遐蝶看了一眼，又把視線收回來，放在你身上。</p><p>「就這樣，也很好。」她說。</p>",
        };

        private static void ShopChoices(GameState g, List<Choice> c)
        {
            if (g.Money >= 30) c.Add(new("snack", "買個飯糰", "$30"));
            if (g.Money >= 50) c.Add(new("buy_bandage", "買繃帶", "$50"));
            if (g.ShopOpen && g.Stamina >= 6) c.Add(new("shop", "打一班", "2小時"));
            c.Add(new("home", "回居住區", TravelLabel(g, "home")));
        }

        private static bool RenderDaily(GameState g, List<Choice> c, out string p)
        {
            p = "";
            switch (g.Scene)
            {
                case "home":
                    {
                        var sb = new StringBuilder();
                        sb.Append($"<p>居住區。{g.Weekday}，{g.Clock}。{(g.Raining ? "外面在下雨。" : "")}離期限還有 {Math.Max(0, GameState.LastDay - g.Day)} 天。</p>");
                        int ch = DueChapter(g);
                        if (ch > 0) sb.Append("<p class=\"gold\">刻度塔那邊的光閃得不太對勁，有事要處理。</p>");
                        if (g.Stamina < 20) sb.Append("<p>你有點撐不住了，先歇一下比較好。</p>");
                        if (g.Injury >= 50) sb.Append("<p class=\"red\">傷得不輕，別做太激烈的事。</p>");
                        p = sb.ToString();
                        if (ch > 0) c.Add(new("ch" + ch, $"前往刻度塔（第 {ch} 章）", "30分"));
                        if (g.HasServant && !g.Has($"med:{g.Day}")) c.Add(new("meditate", "請遐蝶指點魔力", "1小時"));
                        if (g.HasServant && !g.Has($"cast:{g.Day}")) c.Add(new("cast_talk", "跟遐蝶聊聊", "30分"));
                        if (g.Items["bandage"] > 0 && g.Injury > 0) c.Add(new("bandage", "用繃帶處理傷口"));
                        if (g.Seals > 0 && g.Stamina < 40) c.Add(new("seal", "動用一道令咒補魔"));
                        c.Add(new("wait", "等一下", "1小時"));
                        c.Add(new("rest", "休息", "2小時"));
                        if (g.Hour >= 20 || g.Hour < 6) c.Add(new("sleep", "睡到早上"));
                    }
                    return true;
                case "rested":
                    p = "<p>你在沙發上躺了兩個小時，沒睡著，但腰沒那麼僵了。</p>";
                    c.Add(new("home", "站起來")); return true;
                case "slept":
                    p = $"<p>醒來是{g.Weekday}早上，{g.Clock}。{(g.Raining ? "窗外在下雨。" : "")}傷口比昨晚好一點。</p>";
                    c.Add(new("home", "起床")); return true;
                case "meditated":
                    p = "<p>遐蝶讓你坐在地板上，說魔力要走的路得先認得，才不會亂衝。她用手指在空氣裡比了一條線，要你想像那條線在身體裡的位置。</p>" +
                        "<p>一個小時下來，你只學會了不要憋氣。她說這樣就夠了。</p>";
                    c.Add(new("home", "結束")); return true;
                case "bandaged":
                    p = "<p>你把傷口清了一下，貼好。</p>";
                    c.Add(new("home", "收好")); return true;
                case "sealused":
                    p = $"<p>手背一陣熱，疲倦被什麼壓了下去。遐蝶看了一眼剩下的紋路，什麼也沒說。還剩 {g.Seals} 道。</p>";
                    c.Add(new("home", "收回")); return true;

                case "loc_shop":
                    p = "<p>便利商店的自動門每隔幾分鐘就叮咚一聲。店長在櫃檯後面對帳，抬頭看了你一眼，又低下去。</p>";
                    ShopChoices(g, c); return true;
                case "shop":
                    p = "<p>便利商店的班結束了。店長把今天的錢塞給你，順口問你要不要微波一個飯糰。</p>";
                    ShopChoices(g, c); return true;
                case "shop_s1":
                    p = "<p>店長邊補貨邊念：「塔以前每個整點會響一聲，我家那個每次聽到就說該去接小孩了。現在沒有了，他還是會在整點抬頭。」</p><p>她沒等你回話，又去排泡麵了。</p>";
                    ShopChoices(g, c); return true;
                case "shop_s2":
                    p = "<p>下班時店長塞給你一個塑膠袋，裡面是一個便當和一張摺起來的紙。「我先生以前在塔那邊做維修，這是他畫的路線。哪條巷子通後門，塔底還有個檢修口，他說鑰匙早就不見了，但門不鎖。我也看不懂，你拿著，說不定用得到。」</p>" +
                        "<p>紙是影印的，邊角已經軟了。</p>";
                    ShopChoices(g, c); return true;
                case "snack":
                    p = "<p>熱的飯糰，海苔有點軟，還是比什麼都有用。</p>";
                    c.Add(new("loc_shop", "再看看")); c.Add(new("home", "回居住區", TravelLabel(g, "home"))); return true;
                case "bought":
                    p = "<p>你把繃帶塞進外套口袋。</p>";
                    c.Add(new("loc_shop", "再看看")); c.Add(new("home", "回居住區", TravelLabel(g, "home"))); return true;
                case "closed":
                    p = "<p>鐵門拉下來了，營業時間是 8 點到 22 點。</p>";
                    c.Add(new("home", "回居住區")); return true;

                case "cast_1": case "cast_2": case "cast_3": case "cast_4": case "cast_5": case "cast_6":
                    p = CastTalk[int.Parse(g.Scene[5..]) - 1];
                    c.Add(new("home", "各自去忙")); return true;

                // ── 據點 ──
                case "loc_trail":
                case "loc_school":
                case "loc_track":
                    {
                        string k = g.Scene[4..];
                        bool here = Present(g, k);
                        p = k switch
                        {
                            "trail" => "<p>列車停在側線上，車窗都亮著燈。帕姆在月台那頭整理行李，看見你點了個頭，又低頭繼續數箱子。</p>",
                            "school" => here
                                ? "<p>社團大樓有一半的燈亮著，門口的告示欄貼滿學園祭的海報，邊角被風吹得翹起來。</p>"
                                : "<p>社團大樓的燈都關了，風紀委員會的門上貼著紙條：開放時間 8:00 到 20:00。</p>",
                            _ => !here ? "<p>跑道上沒有人，管理室的燈關著。</p>"
                                : g.Raining ? "<p>下雨，跑道封了。看台上空空的，只有幾個人躲在棚子底下滑手機。</p>"
                                : "<p>跑道上有人在熱身，鞋底刮過地面的聲音一陣一陣傳過來。</p>",
                        };
                        if (k == "trail" && !g.Has("met"))
                        {
                            p += "<p>三月七蹲在車門口對著自己的手背發呆，丹恆站在她旁邊，臉色不太好。</p>";
                            c.Add(new("meet", "走過去", "20分"));
                        }
                        else
                        {
                            if (here && !g.Has($"talk:{k}:{g.Day}"))
                                c.Add(new("talk_" + k, $"找{GameState.FactionName[k]}的人聊聊", "1小時"));
                            if (here) ArcChoices(g, k, c);
                            if (k == "trail" && g.Stamina >= 5) c.Add(new("work_trail", "幫列車組跑腿", "1小時"));
                            if (k == "school" && here)
                            {
                                if (g.Stamina >= 5) c.Add(new("work_school", "幫風紀委員會整理資料", "2小時"));
                                if (!g.Has($"study:{g.Day}") && g.Stamina >= 5) c.Add(new("study_school", "到圖書館查塔的記錄", "90分"));
                            }
                            if (k == "track" && here && !g.Raining)
                            {
                                if (g.Stamina >= 10) c.Add(new("work_track", "維護跑道、搬器材", "2小時"));
                                if (!g.Has($"train:{g.Day}") && g.Stamina >= 10 && g.Injury < 50) c.Add(new("train_track", "在跑道上訓練", "1小時"));
                            }
                        }
                        c.Add(new("home", "回居住區", TravelLabel(g, "home")));
                    }
                    return true;
                case "talk_trail":
                case "talk_school":
                case "talk_track":
                    {
                        string k = g.Scene[5..];
                        p = $"<p>{Talk[k][Math.Clamp(g.Talks[k] - 1, 0, 4)]}</p>";
                        c.Add(new("loc_" + k, "留在這裡"));
                        c.Add(new("home", "回居住區", TravelLabel(g, "home")));
                    }
                    return true;
                case "worked":
                    p = g.Last switch
                    {
                        "trail" => "<p>三月七使喚你運了兩箱貨進車廂，又拿回一堆空瓶。她硬塞了點錢過來：「這是勞務費，不是施捨喔。」</p>",
                        "school" => "<p>你替風紀委員會整理了一個下午的文件，影印機卡紙卡了三次。日奈看你一邊罵機器一邊清紙，終於笑了一下，把錢遞給你。</p>",
                        _ => "<p>搬了兩個小時的跨欄和水桶，肩膀痠得抬不起來。管理員拍拍你說：「明天還有。」你沒接話。</p>",
                    };
                    c.Add(new("loc_" + g.Last, "回到據點"));
                    c.Add(new("home", "回居住區", TravelLabel(g, "home")));
                    return true;
                case "studied":
                    p = "<p>圖書館角落那一疊舊記錄，大半是塔的維修單，字跡潦草。翻到一半有幾頁寫到六個刻度各自管什麼，邊上還有人用鉛筆加了註記，註記的人大概自己也不太確定。</p>";
                    c.Add(new("loc_school", "闔上書")); return true;
                case "trained":
                    p = "<p>你沿著跑道慢跑了一小時。前半段腿像灌了鉛，後半段呼吸才跟上，整個人鬆了下來。</p>";
                    c.Add(new("loc_track", "收操")); return true;
                case "meet":
                    p = "<p>「欸，你來得正好！」三月七一看到你就把手舉起來。她的手背上有一模一樣的紅紋，三道。「今天早上醒來就有了，我還以為是睡姿壓出來的。」</p>" +
                        "<p>丹恆沒說話，只把自己的手伸出來給你看。同樣的紋，一道不少。</p>" +
                        "<p>「你呢？」三月七湊過來，抓住你的手腕翻過去，「……噢，你也有。所以不是只有我們被盯上。」</p>";
                    c.Add(new("tell_all", "連遐蝶的名字都告訴他們", "20分"));
                    c.Add(new("tell_some", "說你召喚了人，但不說是誰", "20分"));
                    return true;
                case "tell_all":
                    p = "<p>你把早上的事從頭講了一遍，連真名都沒漏。三月七聽到一半就張大嘴，丹恆打斷她：「讓他說完。」</p>" +
                        "<p>講完之後丹恆沉默了一會兒。「真名最好不要再告訴別人。」他說，「我們這邊也是。」</p><p>「我們這邊也是什麼意思？」三月七問。</p><p>丹恆沒回答。</p>";
                    c.Add(new("loc_trail", "留在月台")); return true;
                case "tell_some":
                    p = "<p>你說你召喚了一個 Caster，其他的沒多講。三月七「欸——」了一長串，但沒有追問。</p>" +
                        "<p>「聰明。」丹恆說，聽不出是誇還是單純陳述。「令咒不要在外面亮。」</p><p>三月七補了一句：「我們也沒說我們的是誰喔，打平。」</p>";
                    c.Add(new("loc_trail", "留在月台")); return true;

                // ── 隨機事件 ──
                case "ev_found":
                    p = "<p>公車站的長椅底下壓著一個信封，裡面是別人掉的零錢。你等了一會兒，沒人回來認。</p>";
                    c.Add(new("evcont", "收下，繼續走")); return true;
                case "ev_tip":
                    p = "<p>巷口乘涼的老人叫住你，說刻度塔以前整點會響一聲，這陣子都沒聲音了。「你們年輕人有沒有注意到？」他問。你說有。他點點頭，繼續搖他的扇子。</p>";
                    c.Add(new("evcont", "道謝，繼續走")); return true;
                case "ev_cat":
                    p = "<p>一隻橘貓跟著你走了兩條街，你一停，它就坐在你腳邊，盯著你，像你欠它什麼。你蹲下來摸了一下，它忍了三秒就跑了。</p>";
                    c.Add(new("evcont", "繼續走")); return true;
                case "ev_notice":
                    p = "<p>電線桿上貼著一張尋人啟事，照片裡的人你沒見過。啟事的日期是五天前，上面蓋了塔管理處的章。</p>";
                    c.Add(new("evcont", "繼續走")); return true;
                case "ev_scuffle":
                    p = "<p>巷口有兩個人在推擠，其中一個往後退的時候撞到你肩膀。他回頭，臉很臭。</p>";
                    c.Add(new("evfight", "不讓，頂回去"));
                    c.Add(new("evavoid", "道歉，繞開", "10分")); return true;
                case "ev_win":
                    p = "<p>你站著沒退，他看了你幾秒，嘴裡罵了一句，走了。你的手心全是汗。</p>";
                    c.Add(new("evcont", "繼續走")); return true;
                case "ev_lose":
                    p = "<p>他的拳頭比看起來快，你退了兩步才站穩。他啐了一口就走了。遐蝶看著你，沒有出手，也沒有說話。</p>";
                    c.Add(new("evcont", "繼續走")); return true;
                case "ev_avoid":
                    p = "<p>你說了聲抱歉，側身讓過去。背後的罵聲一直追到巷口才斷。</p>";
                    c.Add(new("evcont", "繼續走")); return true;
            }
            return false;
        }

        private static void MaybeEvent(GameState g, string next)
        {
            if (!g.Has("summoned") || g.Rng.Next(100) >= 30) return;
            g.Next = next;
            switch (g.Rng.Next(5))
            {
                case 0: g.Earn(g.Rng.Next(15, 46)); g.Scene = "ev_found"; break;
                case 1: g.Scene = "ev_scuffle"; break;
                case 2: g.Gain("insight", 1); g.Scene = "ev_tip"; break;
                case 3: g.Calm(5); g.Scene = "ev_cat"; break;
                default: g.Gain("insight", 1); g.Scene = "ev_notice"; break;
            }
            g.Pass(10);
        }

        private static void Go(GameState g, string k, string place)
        {
            if (g.Place == place) { g.Scene = "loc_" + k; return; }
            int m = TravelMinutes(g.Place, k);
            g.Scene = "loc_" + k;
            g.Place = place;
            g.Pass(m, TravelStamina(g, m));
            MaybeEvent(g, "loc_" + k);
        }

        private static bool StepDaily(GameState g, string id)
        {
            switch (id)
            {
                case "home":
                case "go_home":
                    if (g.Place != "居住區")
                    {
                        if (g.Place is "學園祭廣場" or "賽道" or "刻度塔") g.Pass(30, g.Raining ? 2 : 1);
                        else { int m = TravelMinutes(g.Place, "home"); g.Pass(m, TravelStamina(g, m)); }
                    }
                    g.Place = "居住區"; g.Scene = "home"; return true;
                case "wait": g.Rest(60); g.Scene = "home"; return true;
                case "rest": g.Rest(120); g.Place = "居住區"; g.Scene = "rested"; return true;
                case "sleep": g.SleepToMorning(); g.Place = "居住區"; g.Scene = "slept"; return true;
                case "meditate":
                    g.Flags.Add($"med:{g.Day}");
                    g.Pass(60); g.Gain("magic", 2); g.Calm(10);
                    g.Will = Math.Min(100, g.Will + 3); g.Note("green", "意志 +3");
                    g.Scene = "meditated"; return true;
                case "cast_talk":
                    g.Flags.Add($"cast:{g.Day}");
                    g.Count["cast"]++;
                    g.Pass(30); g.AddBond();
                    g.Scene = "cast_" + Math.Min(6, g.Count["cast"]); return true;
                case "bandage":
                    if (g.Items["bandage"] > 0 && g.Injury > 0)
                    {
                        g.Items["bandage"]--; g.Injury = Math.Max(0, g.Injury - 25); g.Note("green", "傷勢 -25");
                    }
                    g.Scene = "bandaged"; return true;
                case "seal":
                    if (g.Seals > 0) { g.Seals--; g.Stamina = Math.Min(100, g.Stamina + 30); g.Note("red", "令咒 -1"); }
                    g.Scene = "sealused"; return true;
                case "shop":
                    if (!g.ShopOpen) { g.Scene = "closed"; return true; }
                    g.Place = "便利商店"; g.Pass(120, 6); g.Earn(40);
                    g.Count["shop"]++;
                    g.Scene = g.Count["shop"] switch { 3 => "shop_s1", 6 => "shop_s2", _ => "shop" };
                    if (g.Count["shop"] == 3) g.Gain("insight", 1);
                    if (g.Count["shop"] == 6) { g.Flags.Add("hatch"); g.Gain("insight", 2); }
                    if (g.Scene == "shop") MaybeEvent(g, "shop");
                    return true;
                case "snack":
                    if (g.Money >= 30) { g.Money -= 30; g.Stamina = Math.Min(100, g.Stamina + 20); g.Pass(10); g.Note("red", "金錢 -$30"); }
                    g.Scene = "snack"; return true;
                case "buy_bandage":
                    if (g.Money >= 50) { g.Money -= 50; g.Items["bandage"]++; g.Note("red", "金錢 -$50"); g.Note("green", "繃帶 +1"); }
                    g.Scene = "bought"; return true;

                case "go_trail": Go(g, "trail", "開拓列車站"); return true;
                case "go_school": Go(g, "school", "學園都市"); return true;
                case "go_track": Go(g, "track", "賽場"); return true;
                case "go_shop": Go(g, "shop", "便利商店"); return true;
                case "go_tower":
                    {
                        int n = DueChapter(g);
                        if (n > 0) return StepMain(g, "ch" + n);
                    }
                    return true;
                case "loc_trail": case "loc_school": case "loc_track": case "loc_shop": g.Scene = id; return true;
                case "talk_trail": case "talk_school": case "talk_track":
                    {
                        string k = id[5..];
                        g.Flags.Add($"talk:{k}:{g.Day}");
                        g.Talks[k]++;
                        g.Pass(60, 3); g.AddRel(k); g.Gain("social", 1);
                        g.Scene = id;
                    }
                    return true;
                case "work_trail": g.Last = "trail"; g.Pass(60, 4); g.Earn(20); g.Gain("social", 1); g.Scene = "worked"; return true;
                case "work_school": g.Last = "school"; g.Pass(120, 5); g.Earn(45); g.Gain("insight", 1); g.Scene = "worked"; return true;
                case "work_track": g.Last = "track"; g.Pass(120, 12); g.Earn(60); g.Gain("fit", 2); g.Scene = "worked"; return true;
                case "study_school":
                    g.Flags.Add($"study:{g.Day}");
                    g.Pass(90, 4); g.Gain("insight", 3); g.Gain("magic", 1); g.Scene = "studied"; return true;
                case "train_track":
                    g.Flags.Add($"train:{g.Day}");
                    g.Pass(60, 8); g.Gain("fit", 3); g.Scene = "trained"; return true;
                case "meet": g.Scene = "meet"; g.Pass(20); return true;
                case "tell_all": g.Flags.Add("met"); g.Flags.Add("name_told"); g.AddRel("trail"); g.Scene = id; g.Pass(20); return true;
                case "tell_some": g.Flags.Add("met"); g.Flags.Add("name_kept"); g.Scene = id; g.Pass(20); return true;

                case "evcont": g.Scene = g.Next; return true;
                case "evfight":
                    if (g.Rng.Next(100) < 40 + g.Skill["fit"] / 2) { g.Gain("fit", 1); g.Scene = "ev_win"; }
                    else { g.Hurt(8); g.Strain(8); g.Scene = "ev_lose"; }
                    g.Pass(10); return true;
                case "evavoid": g.Strain(3); g.Pass(10); g.Scene = "ev_avoid"; return true;
            }
            return false;
        }
    }
}
