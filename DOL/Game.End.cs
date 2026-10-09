namespace DOL
{
    // 結局：每個結局兩頁，第二頁會依你做過的事補幾句後話
    public static partial class Game
    {
        private static bool RenderEnd(GameState g, List<Choice> c, out string p)
        {
            p = "";
            switch (g.Scene)
            {
                case "end_a":
                    p = "<p class=\"gold\">結局　熄燈</p>" +
                        "<p>你下令的時候，遐蝶沒有回頭。她把手貼在核心上，光一格一格退下去，最後一格熄滅時，整座塔暗了。</p>" +
                        "<p>城裡的燈閃了一下，然後全部滅掉。人們從窗戶探出頭，有人罵，有人笑，也有人只是站在陽台上，看著那座第一次完全黑下來的塔。</p>" +
                        "<p>天亮之後，電力局宣布臨時供電。三天後，路燈亮回一半。</p>";
                    c.Add(new("end_a2", "之後", "")); return true;
                case "end_a2":
                    p = "<p>你在列車站待了幾天，三月七把她拍的那些照片全貼在牆上，一張一張標日期。丹恆站在月台上，姿勢比以前直了一點。姬子泡了茶，沒有人提那座塔。</p>";
                    if (g.Has("promise_trail")) p += "<p>姬子把茶推到你面前。「謝謝你先來說。」她說。</p>";
                    else if (g.Has("hold_trail")) p += "<p>姬子看著你，笑得很淡。「你到最後也沒答應我，做的卻跟我想的一樣。」</p>";
                    if (g.Bond >= 3) p += "<p>遐蝶站在月台邊，看第一班從臨時線路開出去的列車。「我的願望，」她說，「好像不用許了。」</p>";
                    c.Add(new("restart", "重新開始")); return true;

                case "end_b":
                    p = "<p class=\"gold\">結局　共同書寫</p>" +
                        "<p>你站在學園這邊。日奈說，新規則總得有一個外人在場才算數，把筆遞給了你。</p>" +
                        "<p>接下來的一個月，會議從早開到晚，每個人都在吵誰有資格許願、許什麼、誰來審核。聖杯沒有毀，卻第一次被拿來當成一件普通的事討論。</p>";
                    if (g.Has("key_tower")) p += "<p>你把那把鑰匙放回日奈桌上，她看了一眼，沒說話。</p>";
                    c.Add(new("end_b2", "之後", "")); return true;
                case "end_b2":
                    p = "<p>新規則簽下去的那天，日奈在最後一頁簽完名，把筆丟進筆筒。「這是我這輩子最長的一次加班。」她說。</p>";
                    if (g.Bond >= 3) p += "<p>遐蝶旁聽了所有會議，一次也沒發言。最後一天，她只說了一句：「請把規則寫得簡單一點。」所有人都笑了。</p>";
                    c.Add(new("restart", "重新開始")); return true;

                case "end_c":
                    p = "<p class=\"gold\">結局　跑道上的願望</p>" +
                        "<p>你站在賽場這邊。年度大賽延期，規則重寫：許願權不給最強的，給願意把名字寫在賽道上的人。</p>" +
                        "<p>帝王在新規則底下第一個報名，說第一個跑的當然是她。</p>";
                    c.Add(new("end_c2", "之後", "")); return true;
                case "end_c2":
                    p = "<p>重賽那天下午放晴。起跑線上排了一整排人，帝王回頭對你比了個大拇指。</p>";
                    if (g.Has("secret_track")) p += "<p>後勤主管那雙舊跑鞋，被擺在開幕式的獎盃旁邊，沒有人問為什麼。</p>";
                    else if (g.Has("expose_track")) p += "<p>後勤主管在大會上把一切都說了，全場安靜了很久。最後是帝王第一個站起來鼓掌。</p>";
                    if (g.Bond >= 3) p += "<p>遐蝶坐在看台最角落，從頭看到尾。有人問她看得懂嗎，她說：「不懂，可是很好看。」</p>";
                    c.Add(new("restart", "重新開始")); return true;

                case "end_d":
                    p = "<p class=\"gold\">結局　重描一遍</p>" +
                        "<p>你把手放在核心上，遐蝶的手疊在你手背上。令咒一道都沒用，只是兩個人一起，把環上的刻度從零重新描了一遍，就像你第一天在塔底做的那樣。</p>" +
                        "<p>刻度亮起來之後，核心不再往下沉。光回到塔身，亮的是整座塔，不是某一側。</p>";
                    if (g.Bond >= 5) p += "<p>遐蝶的指尖微微發抖。「原來是這個，」她小聲說，「我要的，是有人願意跟我一起把一件事做完。」</p>";
                    c.Add(new("end_d2", "之後", "")); return true;
                case "end_d2":
                    p = "<p>三方都沒有拿到原本想要的。三月七說這樣大概才叫公平，說完自己先笑場。</p>";
                    if (g.Has("promise_trail")) p += "<p>你事先去找過姬子，說你要改。姬子聽完，只說：「謝謝你先來告訴我。」</p>";
                    else if (g.Has("hold_trail")) p += "<p>你到列車站才說這件事。姬子沉默了一下：「你至少有來。」</p>";
                    if (g.Has("key_tower")) p += "<p>日奈把鑰匙收回去，說塔頂的鎖該換新的了。</p>";
                    if (g.Has("secret_track") || g.Has("expose_track"))
                        p += "<p>賽場那邊，後勤主管帶著女兒來看了第一場比賽。帝王把她們安排在最前排。</p>";
                    c.Add(new("restart", "重新開始")); return true;

                case "end_late":
                    p = "<p class=\"gold\">結局　逾期</p>" +
                        "<p>第三十天過去，什麼都沒有發生。刻度塔的光慢慢暗下去，最後一格熄了。</p><p>三方各自做了決定，沒有一個跟你有關。</p>";
                    c.Add(new("restart", "重新開始")); return true;
            }
            return false;
        }

        private static bool StepEnd(GameState g, string id)
        {
            switch (id)
            {
                case "end_d": g.Scene = TrueEnding(g) ? "end_d" : "ch5"; return true;
                case "end_a":
                case "end_b":
                case "end_c":
                case "end_a2":
                case "end_b2":
                case "end_c2":
                case "end_d2":
                    g.Scene = id; return true;
            }
            return false;
        }
    }
}
