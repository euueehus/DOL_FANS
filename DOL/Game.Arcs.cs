namespace DOL
{
    // 三邊各一條三段式支線。聊天累積到一定次數才會解鎖下一段。
    public static partial class Game
    {
        private static readonly int[] ArcNeed = { 1, 3, 5 };

        private static void ArcChoices(GameState g, string k, List<Choice> c)
        {
            string[] label = k switch
            {
                "trail" => new[] { "丹恆說今晚想去塔那邊看看", "三月七有東西要給你看", "姬子把你們叫去車廂" },
                "school" => new[] { "日奈說夜裡有件事要處理", "去文書股，找那份舊記錄", "日奈約你上天台" },
                _ => new[] { "帝王有件事想拜託你", "跟帝王跑一圈", "名單的事有結果了" },
            };
            string[] cost = k == "track" ? new[] { "2小時", "1小時", "1小時" } : new[] { "2小時", "1小時", "40分" };
            for (int n = 1; n <= 3; n++)
            {
                if (g.Has($"arc_{k}_{n}")) continue;
                bool rainBlock = k == "track" && n == 2 && g.Raining;
                if (g.Talks[k] >= ArcNeed[n - 1] && g.Stamina >= 4 && !rainBlock)
                    c.Add(new($"arc_{k}_{n}", label[n - 1], cost[n - 1]));
                break;
            }
        }

        private static bool RenderArcs(GameState g, List<Choice> c, out string p)
        {
            p = "";
            switch (g.Scene)
            {
                // ───── 開拓：列車組 ─────
                case "arc_trail_1":
                    p = "<p>丹恆說今晚想去塔那邊看看，問你要不要一起。你們在塔外的鐵梯底下一直等到半夜，風很大，他把外套領子豎起來，沒怎麼說話。</p>" +
                        "<p>三點十分，塔身最上面一格亮了一下，下面那格同時暗了一下，很快。丹恆在筆記本上畫了一筆，旁邊已經有五個一樣的記號。</p>" +
                        "<p>「被抽走的，」他說，「不是壞掉。有東西把光拿去用。」</p>";
                    c.Add(new("arc_trail_1a", "問他怎麼確定", "30分"));
                    c.Add(new("arc_trail_1b", "不問，陪他坐到天亮", "1小時"));
                    return true;
                case "arc_trail_1a":
                    p = "<p>「看太多次了。」他只說了這句，沒有解釋。你想起他不是會隨便說自己的事的人，就沒再追問。</p>";
                    c.Add(new("loc_trail", "回到月台")); return true;
                case "arc_trail_1b":
                    p = "<p>你們坐在鐵梯下面，看天色從黑轉成灰。快五點的時候，丹恆說：「謝謝你來。」說完就回列車上睡了，連頭都沒回。</p>";
                    c.Add(new("loc_trail", "回到月台")); return true;
                case "arc_trail_2":
                    p = "<p>三月七把一疊洗好的照片鋪在車廂地板上，六張一排，是她這幾天從同一個角度拍的刻度塔。</p>" +
                        "<p>「你看，」她指給你，「第一天六格一樣亮。這張，最底下那格比較暗。這張，連著兩格。」她抬頭，眼睛很亮，「它在變暗，一格一格的，有順序。」</p>" +
                        "<p>「我本來不想管的，」她說，「可是要是這座塔最後熄了，我們是不是都得離開？」</p>";
                    c.Add(new("arc_trail_2a", "把遐蝶叫出來一起看", "30分"));
                    c.Add(new("arc_trail_2b", "自己照時間對一遍", "30分"));
                    return true;
                case "arc_trail_2a":
                    p = "<p>你把遐蝶叫出來，她看了很久，最後伸手指向其中一張照片的角落。「這裡有個很淡的圈。」她說，「不在塔上，在塔底石環的外面。有人在外面又畫了一個。」</p>" +
                        "<p>三月七倒抽一口氣。「所以不只一個人在動手腳。」</p>";
                    c.Add(new("loc_trail", "回到月台")); return true;
                case "arc_trail_2b":
                    p = "<p>你把照片按拍攝時間排好，從頭看到尾，發現暗掉的順序跟你描環時的順序剛好相反。第六格先暗，然後是第五格。</p>" +
                        "<p>三月七聽完呆了一下。「所以是從寶具那格開始……」她自己說完，沒再往下講。</p>";
                    c.Add(new("loc_trail", "回到月台")); return true;
                case "arc_trail_3":
                    p = "<p>姬子把你們三個叫進車廂，桌上攤著那張七個點的地圖。</p>" +
                        "<p>「我只說一次。」她把菸按熄。「如果最後得選，我會選毀掉聖杯。這是我的立場，不是命令。你們三個想怎麼做，我都不攔。」</p>" +
                        "<p>她看向你：「但如果你要動它，不管是砸還是改，先來跟我說一聲。我不想從別人嘴裡才知道。」</p>";
                    c.Add(new("arc_trail_3a", "答應", "")); 
                    c.Add(new("arc_trail_3b", "現在答應不了", ""));
                    return true;
                case "arc_trail_3a":
                    p = "<p>「好。」你說。姬子點點頭，什麼都沒再加。</p><p>三月七偷偷對你比了個大拇指，被丹恆看見，兩人互看一眼，誰也沒笑。</p>";
                    c.Add(new("loc_trail", "走出車廂")); return true;
                case "arc_trail_3b":
                    p = "<p>「我現在答應不了。」你說。姬子盯著你看了幾秒，然後笑了一下。「很好。至少你沒騙我。」</p><p>三月七有點急，想開口，被丹恆按住肩膀。</p>";
                    c.Add(new("loc_trail", "走出車廂")); return true;

                // ───── 學園：風紀委員會 ─────
                case "arc_school_1":
                    p = "<p>日奈說社團大樓三樓盡頭有一間辦公室，連著三天夜裡燈都沒關，守衛敲了門沒人應。她不想為了這種事動用風紀委員，問你能不能陪她去一趟。</p>" +
                        "<p>門從裡面反鎖了。日奈敲了兩下：「風紀委員會，開個門，我不會記名字。」</p>" +
                        "<p>過了很久，門開了一條縫。裡面的人縮在椅子上，眼睛下面一圈黑，手背上也有紅紋。他說他不想打，只想有個地方躲。</p>";
                    c.Add(new("arc_school_1a", "請日奈安排他回宿舍", "30分"));
                    c.Add(new("arc_school_1b", "帶他去列車站避一避", "1小時"));
                    return true;
                case "arc_school_1a":
                    p = "<p>日奈把他的名字寫在自己的筆記本上，說會有人送他回宿舍，這件事不會讓別人知道。那個人一直低著頭道謝，聲音啞啞的。</p>";
                    c.Add(new("loc_school", "回到社團大樓")); return true;
                case "arc_school_1b":
                    p = "<p>你帶他走到列車站，三月七一看到就把毯子拿出來。「睡我的床！我去睡丹恆那邊。」丹恆在後面說了一句：「為什麼是我那邊。」沒有人理他。</p>";
                    c.Add(new("loc_school", "回到社團大樓")); return true;
                case "arc_school_2":
                    p = "<p>學園祭那天被藏起來的記錄第二頁，日奈知道在誰手上，是文書股的一個學生，一直不肯交出來。</p>" +
                        "<p>你在文書股辦公室找到她。桌上堆著整理到一半的檔案，她看見日奈跟你一起進來，臉先沉了一下。</p>";
                    c.Add(new("arc_school_2a", "好好跟她談（社交 5 以上比較有把握）", "30分"));
                    c.Add(new("arc_school_2b", "趁她離開時看一下桌面（觀察 5 以上比較有把握）", "30分"));
                    return true;
                case "arc_school_2a":
                    p = "<p>你沒提任何要求，只問她為什麼不想交。她盯著桌面看了很久，說：「我怕交出去，之後就不是我們說了算。」你說你不會自己決定什麼，她才把那頁影本從資料夾最底下抽出來。</p>" + SchoolClue(g);
                    c.Add(new("loc_school", "道謝離開")); return true;
                case "arc_school_2b":
                    p = "<p>她去倒水的時候你掃了一眼桌面，影本就夾在一疊報表中間。你沒有碰，等她回來，直接問她那頁是不是在左邊第三個資料夾。她臉色變了，沒再否認。</p>" + SchoolClue(g);
                    c.Add(new("loc_school", "道謝離開")); return true;
                case "arc_school_2ax":
                    p = "<p>你講了一堆，她只是搖頭。日奈在旁邊嘆了口氣，說今天先這樣。你覺得自己該多跟人說說話。</p>";
                    c.Add(new("loc_school", "離開")); return true;
                case "arc_school_2bx":
                    p = "<p>你把桌面翻了一遍，什麼也沒找到。她回來時看見你站在桌邊，表情冷下來。日奈替你打了圓場，說先走。</p>";
                    c.Add(new("loc_school", "離開")); return true;
                case "arc_school_3":
                    p = "<p>日奈約你在天台。風很大，她把文件夾夾在腋下，從口袋裡掏出一把鑰匙。</p>" +
                        "<p>「塔頂那扇門，只有風紀委員長有鑰匙。」她說。「學園要是決定留著聖杯，我得拿這把去開門。我想先讓你知道，我不是要騙你上去。」</p>" +
                        "<p>她把鑰匙放在欄杆上，退開一步。「你決定要不要拿。」</p>";
                    c.Add(new("arc_school_3a", "收下鑰匙", ""));
                    c.Add(new("arc_school_3b", "還給她，說你不想被當成學園的人", ""));
                    return true;
                case "arc_school_3a":
                    p = "<p>你把鑰匙收進口袋。日奈點點頭，沒再說什麼，轉身去推天台的門，像在確認沒人偷聽。</p>";
                    c.Add(new("loc_school", "下樓")); return true;
                case "arc_school_3b":
                    p = "<p>「我不想被當成學園的人。」你說。日奈愣了一下，把鑰匙收回去。「……好，」她說，「那就是兩邊都不欠。」她笑了一下，笑得很累。</p>";
                    c.Add(new("loc_school", "下樓")); return true;

                // ───── 賽場：帝王 ─────
                case "arc_track_1":
                    p = "<p>帝王把你拉到看台後面，從外套裡掏出一張皺巴巴的名單。「年度大賽的出賽表，」她壓低聲音，「有一個人的名字被換掉了。換的人用的是跟原本一樣的筆跡，做得很細，不是外行人。」</p>" +
                        "<p>「我不想直接去問，」帝王說，「你幫我看看，是誰在動。」</p>";
                    c.Add(new("arc_track_1a", "答應，去守管理室", "2小時"));
                    return true;
                case "arc_track_1a":
                    p = "<p>你在管理室外面的走廊守了兩個晚上。第二天夜裡，有人拿著鑰匙進去，在櫃子前面站了很久，什麼也沒動，只是把名單抽出來對著燈看。你認得那個背影，是賽場的後勤主管。</p>" +
                        "<p>你沒有出聲，只把這件事記下來。</p>";
                    c.Add(new("loc_track", "回到賽場")); return true;
                case "arc_track_2":
                    p = "<p>帝王把你拉到跑道上。「不跑一圈，我怎麼知道你值不值得信？」她說得理所當然，連跑鞋都借了你一雙，大了兩號。</p>";
                    c.Add(new("arc_track_2a", "跑", "1小時"));
                    return true;
                case "arc_track_2a":
                    p = g.Skill["fit"] >= 10
                        ? "<p>你咬著牙跟到了最後一個彎，帝王回頭看了一眼，咧嘴笑起來：「不錯嘛！」</p>"
                        : "<p>你在第二個彎就喘不過氣，慢了下來。帝王停在前面等你，沒笑你，只說：「沒關係，本帝王等得起。」</p>";
                    p += "<p>跑到第三個彎，兩個人的腿同時沉了一下，像踩進泥裡。帝王低頭看著自己的腳：「就是這個。」</p>";
                    c.Add(new("loc_track", "收操")); return true;
                case "arc_track_3":
                    p = "<p>你和帝王在管理室外面等到那個人出現。是賽場的後勤主管，手裡抱著一個舊保溫袋，裡面是一雙磨壞的跑鞋。</p>" +
                        "<p>她看到你們，沒有跑，只是把袋子抱緊了一點。「那是我女兒的鞋。」她說。「她跑到一半，腿出了事。我想要的願望很小，只是想讓她再跑一次。」</p>" +
                        "<p>帝王張了張嘴，什麼都沒說出來。</p>";
                    c.Add(new("arc_track_3a", "替她保密，勸帝王別追究", ""));
                    c.Add(new("arc_track_3b", "要她自己去跟大會說清楚", ""));
                    return true;
                case "arc_track_3a":
                    p = "<p>你對帝王說，這件事先放在這裡。帝王抓了抓頭髮，看著那雙鞋，過了很久才點頭。「……好。但名單要改回去。」她對那個人說。那個人鞠了一個很深的躬。</p>";
                    c.Add(new("loc_track", "離開管理室")); return true;
                case "arc_track_3b":
                    p = "<p>「該怎麼處理，要她自己去說。」你說。後勤主管閉上眼睛，點了點頭。帝王沒有反對，只是把名單抽走，說明天一起去找大會。</p>";
                    c.Add(new("loc_track", "離開管理室")); return true;
            }
            return false;
        }

        private static string SchoolClue(GameState g) =>
            g.Has("arc_school_2_extra")
                ? "<p>影本上有一段你上次沒看到的備註：消失的範圍，是從塔往外算的。</p>"
                : "<p>影本上只有半句：願望實現之後，城裡會有一塊區域消失。句尾被撕掉了。</p>";

        private static bool StepArcs(GameState g, string id)
        {
            switch (id)
            {
                // 開拓
                case "arc_trail_1": g.Scene = id; g.Pass(60, 2); return true;
                case "arc_trail_1a": g.Flags.Add("arc_trail_1"); g.Gain("insight", 1); g.Scene = id; g.Pass(30); return true;
                case "arc_trail_1b": g.Flags.Add("arc_trail_1"); g.Calm(5); g.AddRel("trail"); g.Scene = id; g.Pass(60); return true;
                case "arc_trail_2": g.Scene = id; g.Pass(20, 1); return true;
                case "arc_trail_2a": g.Flags.Add("arc_trail_2"); g.AddBond(); g.Gain("insight", 1); g.Scene = id; g.Pass(30); return true;
                case "arc_trail_2b": g.Flags.Add("arc_trail_2"); g.Gain("insight", 2); g.Scene = id; g.Pass(30); return true;
                case "arc_trail_3": g.Scene = id; g.Pass(10); return true;
                case "arc_trail_3a": g.Flags.Add("arc_trail_3"); g.Flags.Add("promise_trail"); g.AddRel("trail"); g.Scene = id; g.Pass(30); return true;
                case "arc_trail_3b": g.Flags.Add("arc_trail_3"); g.Flags.Add("hold_trail"); g.AddRel("trail"); g.Scene = id; g.Pass(30); return true;

                // 學園
                case "arc_school_1": g.Scene = id; g.Pass(60, 2); return true;
                case "arc_school_1a": g.Flags.Add("arc_school_1"); g.AddRel("school"); g.Scene = id; g.Pass(30); return true;
                case "arc_school_1b": g.Flags.Add("arc_school_1"); g.AddRel("trail"); g.AddRel("school"); g.Scene = id; g.Pass(60, 1); return true;
                case "arc_school_2": g.Scene = id; g.Pass(20, 1); return true;
                case "arc_school_2a":
                case "arc_school_2b":
                    {
                        bool ok = id.EndsWith("a") ? g.Skill["social"] >= 5 : g.Skill["insight"] >= 5;
                        if (ok)
                        {
                            g.Flags.Add("arc_school_2");
                            if (g.Has("clueA")) { g.Flags.Add("arc_school_2_extra"); g.Gain("insight", 1); }
                            else g.Flags.Add("clueA");
                            g.AddRel("school");
                            g.Scene = id;
                        }
                        else
                        {
                            if (id.EndsWith("b")) g.Strain(3);
                            g.Scene = id + "x";
                        }
                        g.Pass(30);
                    }
                    return true;
                case "arc_school_3": g.Scene = id; g.Pass(10); return true;
                case "arc_school_3a": g.Flags.Add("arc_school_3"); g.Flags.Add("key_tower"); g.AddRel("school"); g.Scene = id; g.Pass(30); return true;
                case "arc_school_3b": g.Flags.Add("arc_school_3"); g.Flags.Add("no_key"); g.AddRel("school"); g.Scene = id; g.Pass(30); return true;

                // 賽場
                case "arc_track_1": g.Scene = id; g.Pass(20, 1); return true;
                case "arc_track_1a": g.Flags.Add("arc_track_1"); g.Gain("insight", 1); g.Scene = id; g.Pass(120, 3); return true;
                case "arc_track_2": g.Scene = id; g.Pass(10); return true;
                case "arc_track_2a": g.Flags.Add("arc_track_2"); g.Gain("fit", 2); g.AddRel("track"); g.Scene = id; g.Pass(60, 8); return true;
                case "arc_track_3": g.Scene = id; g.Pass(30); return true;
                case "arc_track_3a": g.Flags.Add("arc_track_3"); g.Flags.Add("secret_track"); g.AddRel("track"); g.Scene = id; g.Pass(30); return true;
                case "arc_track_3b": g.Flags.Add("arc_track_3"); g.Flags.Add("expose_track"); g.AddRel("track"); g.Scene = id; g.Pass(30); return true;
            }
            return false;
        }
    }
}
