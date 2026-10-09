namespace DOL
{
    // 主線：第一章到第五章
    public static partial class Game
    {
        private static bool RenderMain(GameState g, List<Choice> c, out string p)
        {
            p = "";
            switch (g.Scene)
            {
                // ───── 第一章 ─────
                case "ch1":
                    p = "<p>第一章。</p><p>三天了，手背的紋路沒再變。這天早上，門縫裡塞進來兩張紙，一張蓋著學園的印，一張是賽程表，兩張都沒署名。</p>" +
                        "<p>遐蝶看了一眼，說：「有人想看看您會先拆哪一張。」</p>";
                    c.Add(new("ch1_school", "先看學園那張", "10分"));
                    c.Add(new("ch1_track", "先看賽程表", "10分"));
                    c.Add(new("ch1_neutral", "兩張都收起來", "10分"));
                    return true;
                case "ch1_school":
                    p = "<p>學園那張只有一行字：校園裡請不要動用令咒。若要談，學園祭之前來社團大樓找風紀委員會。</p><p>字寫得很工整，像是寫過很多次，寫到不想再改了。</p>";
                    c.Add(new("home", "把紙收起來")); return true;
                case "ch1_track":
                    p = "<p>賽程表上用紅筆圈了年度大賽的日期，旁邊有人用鉛筆寫了一行小字：賽道不是戰場。除非你們把它變成戰場。</p><p>最後那個句號壓得很重，紙都戳破了。</p>";
                    c.Add(new("home", "把紙收起來")); return true;
                case "ch1_neutral":
                    p = "<p>你把兩張紙一起折好放進抽屜，沒有回覆任何一邊。遐蝶說這樣最穩，只是三邊都會記得你什麼都沒說。</p>";
                    c.Add(new("home", "關上抽屜")); return true;

                // ───── 第二章 ─────
                case "ch2":
                    p = "<p>第二章。學園祭。</p><p>廣場擠得走不動，攤位一路排到噴水池。遐蝶站在你身後半步，手裡拿著剛買的棉花糖，一口沒吃。</p>" +
                        "<p>角落一個賣二手書的攤位後面，堆著一疊舊紙，封面寫著刻度塔維修記錄。攤主眼神不對，每次有人靠近就用手肘把第二頁壓住。</p>";
                    c.Add(new("ch2_read", "假裝買書，把第二頁抽來看", "30分"));
                    c.Add(new("ch2_skip", "不管，繼續逛", "30分"));
                    return true;
                case "ch2_read":
                    p = "<p>你挑了一本破破的食譜，趁攤主找零錢的時候把那一頁瞄完。上面寫的不多，最後一行只有半句：願望實現之後，城裡會有一塊區域消失。</p>" +
                        "<p>句尾被撕掉了。</p><p>遐蝶看了你一眼，沒問你看到什麼。</p>";
                    c.Add(new("ch2_after", "往人群外走", "10分")); return true;
                case "ch2_skip":
                    p = "<p>你跟著人潮逛到傍晚，買了一串烤魷魚，遐蝶幫你拿紙袋。再經過那個攤位時，攤主已經收攤，桌上什麼都沒有。</p>";
                    c.Add(new("ch2_after", "往人群外走", "10分")); return true;
                case "ch2_after":
                    p = "<p>人群邊緣有人叫你。是日奈，風紀委員的臂章別得很正，眼下兩圈黑。「你是開拓那邊的人吧，」她說，「來祭典可以，記得規則：不要在這裡用令咒，我不想為了這個加班。」</p>" +
                        "<p>「我只是來逛。」你說。</p><p>她沉默了兩秒。「……最好是。」</p>";
                    c.Add(new("ch2_a1", "說你是來找舊記錄的"));
                    c.Add(new("ch2_a2", "說你只是來吃東西"));
                    return true;
                case "ch2_a1":
                    p = "<p>你老實說了。日奈挑了一下眉，沒生氣。「那你應該去圖書館，」她說，「別去地攤。」</p>";
                    c.Add(new("home", "離開廣場", "30分")); return true;
                case "ch2_a2":
                    p = "<p>日奈看了眼你手上的烤魷魚，嘆了一口氣。「對，看得出來。」她轉身走了，臂章在人群裡晃了一下就不見了。</p>";
                    c.Add(new("home", "離開廣場", "30分")); return true;

                // ───── 第三章 ─────
                case "ch3":
                    p = "<p>第三章。年度大賽。</p><p>天空很藍，看台坐滿了人。賽道邊緣有幾個刻度標記在發光，頻率不一致，各跳各的。</p>" +
                        "<p>帝王剛從熱身區跑回來，看到你就大喊：「你來啦！等一下的比賽不准走開！」</p>";
                    c.Add(new("ch3_race", "先看完比賽", "1小時"));
                    c.Add(new("ch3_fight", "先去處理賽道旁的刻度", "1小時"));
                    if (g.Seals > 0) c.Add(new("ch3_seal", "令咒一劃，讓遐蝶壓住刻度"));
                    return true;
                case "ch3_race":
                    p = "<p>發令槍響，十幾個人像潮水一樣衝出去。第三個彎，所有人的速度都掉了一截，很不自然，連帝王都在咬牙。</p>" +
                        "<p>終點線後面，她喘著氣回頭看你，眼神在問你看到了沒有。你看到了。遐蝶在耳邊小聲說：「是聖杯。它在拿走他們的速度。」</p>";
                    c.Add(new("ch3_after", "等帝王過來", "20分")); return true;
                case "ch3_fight":
                    p = "<p>你擠過人群跑到彎道旁，刻度標記燙得碰不得。你用外套包著手去砸最亮的那個，砸裂了一格，彎道上的人才慢慢跑回正常速度。你的手臂被反彈的力量震得發麻。</p>" +
                        "<p>比賽重新開始，沒有人注意到剛才發生了什麼。只有遐蝶說：「聖杯在吸收力量，所以我們才會被降格。」</p>";
                    c.Add(new("ch3_after", "等帝王過來", "20分")); return true;
                case "ch3_seal":
                    p = "<p>你在心裡下令。手背一熱，遐蝶抬手朝彎道的方向壓下去，空氣顫了一下，刻度的光整個沉下去。賽道上的人從頭到尾都沒發現。</p>" +
                        "<p>「這一道很貴。」遐蝶輕聲說。「但我明白您為什麼要用。聖杯在吸收這座城的力量，我們這些被召喚出來的人，才會被降格。」</p>";
                    c.Add(new("ch3_after", "等帝王過來", "20分")); return true;
                case "ch3_after":
                    p = "<p>比賽結束，帝王衝到你面前，一臉得意：「第三名！比預期好！」然後她壓低聲音：「剛剛那個，是你做的吧？」</p>";
                    c.Add(new("ch3_t1", "承認"));
                    c.Add(new("ch3_t2", "裝傻"));
                    return true;
                case "ch3_t1":
                    p = "<p>你點頭。帝王嘴巴張得很大，過一會兒才說：「那你欠我一場比賽。」</p>";
                    c.Add(new("home", "離開賽道", "30分")); return true;
                case "ch3_t2":
                    p = "<p>「什麼剛剛？」你說。帝王瞇起眼睛看了你一陣，最後只是笑。「好啦，本帝王就當沒看到。」</p>";
                    c.Add(new("home", "離開賽道", "30分")); return true;

                // ───── 第四章 ─────
                case "ch4":
                    {
                        p = "<p>第四章。</p><p>第二十二天，刻度塔的光暗得很明顯，街上已經有人在議論。傍晚，三張紙條前後送到：列車站、社團大樓、賽場管理室，都約你今晚到塔底談。</p>" +
                            "<p>你到的時候，三邊的人已經站在石環外面，沒有人說話，彼此隔著三步遠。遐蝶走在你旁邊，沒有踏進去。</p>";
                        if (!g.Has("heard_t")) c.Add(new("ch4_t", "聽開拓怎麼說", "20分"));
                        if (!g.Has("heard_s")) c.Add(new("ch4_s", "聽學園怎麼說", "20分"));
                        if (!g.Has("heard_r")) c.Add(new("ch4_r", "聽賽場怎麼說", "20分"));
                        int sides = 0;
                        if (g.Rel["trail"] >= 3) { c.Add(new("end_a", "站開拓：把聖杯毀掉")); sides++; }
                        if (g.Rel["school"] >= 3) { c.Add(new("end_b", "站學園：留下，重寫規則")); sides++; }
                        if (g.Rel["track"] >= 3) { c.Add(new("end_c", "站賽場：用比賽決定願望")); sides++; }
                        if (sides == 0) p += "<p>你跟哪一邊都還沒熟到能站在他們那邊。</p>";
                        c.Add(new("ch5", "哪邊都不站，直接上塔頂"));
                    }
                    return true;
                case "ch4_t":
                    p = "<p>姬子先開口，聲音很平。「我們的看法很簡單：這東西在吃這座城。吃完城，下一個是人。砸掉它，能源的缺口，大家一起想辦法補。」</p>" +
                        "<p>三月七接了一句：「我們不是要欺負誰，真的。」丹恆什麼也沒說，只是看著你。</p>";
                    if (g.Has("name_told")) p += "<p>三月七偷偷看了遐蝶一眼，很快又移開視線。</p>";
                    c.Add(new("ch4", "退回原位")); return true;
                case "ch4_s":
                    p = "<p>日奈今晚沒別臂章。「學園的意見是留著，」她說，「但要有規則地留著。許願的權利，交給所有人，不是交給誰比較強。」</p>" +
                        "<p>她看了一下你的手背。「這個提議，我不是替學園講的，是我自己的想法。學園那邊還在吵。」</p>";
                    if (g.Has("name_told")) p += "<p>你注意到她提到遐蝶時，叫的是真名。你只告訴過列車組，不知道風是從哪裡漏出去的。</p>";
                    c.Add(new("ch4", "退回原位")); return true;
                case "ch4_r":
                    p = "<p>帝王站得離人群最遠，雙手插在口袋裡。「我只講一句，」她說，「願望歸誰，用跑的決定。誰跑得過我，誰說了算。我不是在開玩笑。」</p>" +
                        "<p>後面有人笑，她轉過去瞪了一眼，那個人立刻不笑了。</p>";
                    c.Add(new("ch4", "退回原位")); return true;

                // ───── 第五章 ─────
                case "ch5":
                    {
                        p = "<p>第五章。</p>";
                        if (g.Has("key_tower")) p += "<p>你拿出日奈給的鑰匙，轉了半圈，塔頂的門就開了。</p>";
                        else if (g.Has("hatch")) p += "<p>你照著店長給的那張紙，從塔底的檢修口鑽進去，爬了很久的鐵梯，才在頂端摸到一扇不上鎖的小門。</p>";
                        else p += "<p>塔頂的門被人撬過，鎖歪在一邊，你推了推就開了。</p>";
                        p += "<p>上面沒有杯子。只有一個轉得很慢的環，六個刻度一格一格暗著，環中央懸著一團光，每隔幾秒就往下沉一點。</p>" +
                             "<p>遐蝶在門口停了一下，才走進來。「這不是許願的東西，」她看了很久才說，「是在把願望換成能源。」</p>";
                        if (g.Bond >= 4) p += "<p>她在你身後輕聲說：「不管您怎麼決定，我都陪著。」</p>";
                        var miss = Missing(g);
                        if (miss.Count == 0)
                        {
                            p += "<p>三邊的說法你都聽過了，該知道的也都湊齊了。你看得懂這顆核心，現在有一條路可以走。</p>";
                            c.Add(new("end_d", "改寫核心"));
                        }
                        else
                        {
                            p += "<p>你想動它，但還差一些：</p><p class=\"red\">" + string.Join("<br>", miss) + "</p>";
                        }
                        c.Add(new("end_a", "直接毀掉它"));
                    }
                    return true;
            }
            return false;
        }

        private static bool StepMain(GameState g, string id)
        {
            switch (id)
            {
                case "ch1": g.Scene = "ch1"; g.Place = "居住區"; g.Pass(10); return true;
                case "ch1_school": g.Flags.Add("ch1"); g.AddRel("school"); g.Scene = id; g.Pass(10); return true;
                case "ch1_track": g.Flags.Add("ch1"); g.AddRel("track"); g.Scene = id; g.Pass(10); return true;
                case "ch1_neutral": g.Flags.Add("ch1"); g.Scene = id; g.Pass(10); return true;

                case "ch2": g.Scene = "ch2"; g.Place = "學園祭廣場"; g.Pass(30, 3); return true;
                case "ch2_read": g.Flags.Add("ch2"); g.Flags.Add("clueA"); g.Gain("insight", 2); g.Scene = id; g.Pass(30); return true;
                case "ch2_skip": g.Flags.Add("ch2"); g.Scene = id; g.Pass(30); return true;
                case "ch2_after": g.Scene = id; g.Pass(10); return true;
                case "ch2_a1": g.AddRel("school"); g.Scene = id; g.Pass(10); return true;
                case "ch2_a2": g.Gain("social", 1); g.Scene = id; g.Pass(10); return true;

                case "ch3": g.Scene = "ch3"; g.Place = "賽道"; g.Pass(60, 4); return true;
                case "ch3_race": g.Flags.Add("ch3"); g.Flags.Add("clueB"); g.AddRel("track"); g.Scene = id; g.Pass(30, 2); return true;
                case "ch3_fight":
                    g.Flags.Add("ch3"); g.Flags.Add("clueB"); g.AddRel("trail");
                    g.Hurt(g.Skill["fit"] >= 20 ? 5 : 10); g.Scene = id; g.Pass(30, 5); return true;
                case "ch3_seal":
                    if (g.Seals > 0) g.Seals--;
                    g.Note("red", "令咒 -1");
                    g.Flags.Add("ch3"); g.Flags.Add("clueB"); g.AddRel("trail"); g.AddRel("track");
                    g.Scene = id; g.Pass(30, 2); return true;
                case "ch3_after": g.Scene = id; g.Pass(20); return true;
                case "ch3_t1": g.AddRel("track"); g.Scene = id; g.Pass(10); return true;
                case "ch3_t2": g.Gain("social", 1); g.Scene = id; g.Pass(10); return true;

                case "ch4":
                    {
                        bool first = !g.Has("ch4_in");
                        g.Flags.Add("ch4"); g.Flags.Add("ch4_in");
                        g.Scene = "ch4"; g.Place = "刻度塔";
                        if (first) g.Pass(40, 3);
                    }
                    return true;
                case "ch4_t": g.Flags.Add("heard_t"); g.Scene = id; g.Pass(20); return true;
                case "ch4_s":
                    g.Flags.Add("heard_s"); g.Scene = id; g.Pass(20);
                    if (g.Has("name_told")) g.Strain(5);
                    return true;
                case "ch4_r": g.Flags.Add("heard_r"); g.Scene = id; g.Pass(20); return true;

                case "ch5": g.Scene = "ch5"; g.Place = "刻度塔"; g.Pass(30, 3); return true;
            }
            return false;
        }
    }
}
