namespace DOL
{
    public static class Story
    {
        public sealed record Beat(string Passage, object[] Actions, string Place, int PassMinutes = 0, int Stamina = 0, bool GrantSeals = false, bool Contract = false);

        public static Beat At(string scene) => scene switch
        {
            "noseal" => new(
                "<p>手背沒有印記。令咒是聖杯選中御主的證明，聖杯還沒有選你。</p>",
                new object[] { new { id = "rite1", text = "前往刻度塔", cost = "20分" } },
                "居住區", 20, 5),

            "rite1" => new(
                "<p>塔底的環是暗的。六格刻度停在零：筋力、耐久、敏捷、魔力、幸運、寶具。</p><p>你沿舊線把環描亮。環亮起的瞬間，手背燒了起來。聖杯選中了你，三道令咒浮現，一道不缺。</p>",
                new object[] { new { id = "rite2", text = "退到環外，開始詠唱", cost = "10分" } },
                "刻度塔", 10, 0, true),

            "rite2" => new(
                "<p>「以石為基，以銀為引。四方之門閉鎖，循環自此起轉。」</p><p>「聖杯在上。要來，就來一個願意應約的人。」</p>",
                new object[] { new { id = "rite3", text = "第二遍詠唱，把環封上", cost = "10分" } },
                "刻度塔", 10, 5),

            "rite3" => new(
                "<p>六格同時轉。冷氣從環外壓進來。</p><p>「我在此宣告。願你以劍立於我名之下，我以命運託付於你。」</p><p>光開始聚成人形。還不是臉。</p>",
                new object[] { new { id = "rite4", text = "維持詠唱，等輪廓", cost = "10分" } },
                "刻度塔", 10, 5),

            "rite4" => new(
                "<p>她站在環內，沒有踏線。英靈已經現身，契約還沒有成立。</p><p>「問你。你就是召喚我的Master嗎？」</p>",
                new object[] { new { id = "rite5", text = "回答：是", cost = "10分" } },
                "刻度塔", 10, 5),

            "rite5" => new(
                "<p>「Servant，Caster。應召而來。」</p><p>魔力從你流向她。契約成立。</p>",
                new object[] { new { id = "summoned", text = "確認契約", cost = "10分" } },
                "刻度塔", 10, 5, false, true),

            "summoned" => new(
                "<p>令咒三道都在。職階 Caster，真名遐蝶。</p><p>「真名不要對外說。令咒一劃是一次絕對命令，用一道就少一道。」</p>",
                new object[]
                {
                    new { id = "castorice", text = "問令咒怎麼用", cost = "20分" },
                    new { id = "station", text = "離開刻度塔", cost = "30分" },
                },
                "刻度塔", 20, 5),

            "castorice" => new(
                "<p>「一劃，一次絕對命令。可以強迫我做得到的事，也可以拿來強化、補魔。不用咒文。」</p><p>「三劃用完，你就失去命令我的資格。」</p>",
                new object[]
                {
                    new { id = "station", text = "去開拓列車站", cost = "30分" },
                    new { id = "night", text = "回居住區", cost = "30分" },
                },
                "刻度塔", 20, 0),

            "station" => new(
                "<p>開拓列車站沒有列車。公告只寫：刻度塔異動。</p><p>月台有人在拍照，手背有和你一樣的印記。</p>",
                new object[]
                {
                    new { id = "meet", text = "走過去", cost = "20分" },
                    new { id = "night", text = "不接觸，回居住區", cost = "30分" },
                },
                "開拓列車站", 30, 8),

            "meet" => new(
                "<p>「你也有？我叫三月七。這是丹恆。」</p><p>丹恆沒有問真名。三月七說印記是今天早上出現的。</p>",
                new object[]
                {
                    new { id = "ally", text = "承認剛完成召喚", cost = "20分" },
                    new { id = "decline", text = "不表明身分", cost = "10分" },
                },
                "開拓列車站", 20, 5),

            "ally" => new(
                "<p>丹恆說塔會叫滿七組。他沒有報自己的從者。</p><p>「令咒不要在街上亮。我們先回列車組。」</p><p>序章到此。聖杯戰爭已經開始。</p>",
                new object[] { new { id = "ch1", text = "進入第一章", cost = "" } },
                "開拓列車站", 20, 0),

            "decline" => new(
                "<p>三月七把相機放下。丹恆只說：令咒不要對著路人。</p><p>序章到此。你沒有表明身分。</p>",
                new object[] { new { id = "ch1", text = "進入第一章", cost = "" } },
                "開拓列車站", 10, 0),

            "ch1" => new(
                "<p>第一章。三方試探。</p><p>居住區門縫塞進兩張紙。一張蓋學園戳，一張是賽程表。都沒有署名。</p>",
                new object[]
                {
                    new { id = "ch1_school", text = "先看學園那張", cost = "10分" },
                    new { id = "ch1_track", text = "先看賽程表", cost = "10分" },
                    new { id = "ch1_neutral", text = "兩張都收起來", cost = "10分" },
                },
                "居住區", 10, 0),

            "ch1_school" => new(
                "<p>學園戳下面只有一句：校園裡不要開令咒。要談，學園祭前來社團大樓。</p><p>沒有人站在門外。</p>",
                new object[] { new { id = "ch1_end", text = "收起紙", cost = "" } },
                "居住區", 10, 0),

            "ch1_track" => new(
                "<p>賽程表圈了年度大賽的日子。空白處寫：賽道不是戰場，除非你們把它變成戰場。</p>",
                new object[] { new { id = "ch1_end", text = "收起紙", cost = "" } },
                "居住區", 10, 0),

            "ch1_neutral" => new(
                "<p>兩張都沒回。遐蝶說這樣也行，三邊都會把你記成未表態。</p>",
                new object[] { new { id = "ch1_end", text = "收起紙", cost = "" } },
                "居住區", 10, 0),

            "ch1_end" => new(
                "<p>第一場接觸到此為止。沒有開打。線索還沒有。</p>",
                new object[] { new { id = "ch2", text = "進入第二章", cost = "" } },
                "居住區"),

            "ch2" => new(
                "<p>第二章。學園祭。</p><p>廣場在辦祭典。暗處有人把刻度塔的舊記錄攤在攤位後面，不給看第二頁。</p>",
                new object[]
                {
                    new { id = "ch2_read", text = "看那一頁", cost = "30分" },
                    new { id = "ch2_skip", text = "只參加祭典", cost = "30分" },
                },
                "學園祭廣場", 30, 8),

            "ch2_read" => new(
                "<p>記錄只寫到半句：願望實現之後，城裡會有一塊區域消失。</p><p>這是線索 A。遐蝶沒有解釋消失的是哪一區。</p>",
                new object[] { new { id = "ch3", text = "進入第三章", cost = "" } },
                "學園祭廣場", 20, 0),

            "ch2_skip" => new(
                "<p>你沒有看記錄。祭典結束時，那一頁已經不在了。</p><p>線索 A 沒有拿到。</p>",
                new object[] { new { id = "ch3", text = "進入第三章", cost = "" } },
                "學園祭廣場", 20, 0),

            "ch3" => new(
                "<p>第三章。賽場決戰。</p><p>年度大賽和聖杯戰爭撞在同一天。賽道邊的刻度在轉，像在抽走上頭的速度。</p>",
                new object[]
                {
                    new { id = "ch3_race", text = "先把比賽跑完", cost = "1小時" },
                    new { id = "ch3_fight", text = "先處理賽道上的刻度", cost = "1小時" },
                },
                "賽道", 60, 15),

            "ch3_race" => new(
                "<p>比賽先結束。刻度沒有停。遐蝶說聖杯在吸力量，所以英靈才會被降格。</p><p>這是線索 B。</p>",
                new object[] { new { id = "ch4", text = "進入第四章", cost = "" } },
                "賽道", 30, 5),

            "ch3_fight" => new(
                "<p>刻度被打斷一格。比賽中止。吸力還在，只是慢了。</p><p>線索 B 一樣：降格是因為聖杯在吸收力量。</p>",
                new object[] { new { id = "ch4", text = "進入第四章", cost = "" } },
                "賽道", 30, 10),

            "ch4" => new(
                "<p>第四章。背叛之夜。</p><p>三邊對聖杯的處置分裂。沒有人要求你報真名。他們只要你站邊。</p>",
                new object[]
                {
                    new { id = "end_a", text = "站開拓：摧毀聖杯", cost = "" },
                    new { id = "end_b", text = "站學園：留下並改寫", cost = "" },
                    new { id = "end_c", text = "站賽場：用比賽決定願望", cost = "" },
                    new { id = "ch5", text = "哪邊都不站，去塔頂", cost = "" },
                },
                "刻度塔", 40, 10),

            "ch5" => new(
                "<p>第五章。刻度歸零。</p><p>塔頂的核心不是許願杯。遐蝶確認它在把願望轉成城市能源。</p><p>三邊的說法你都聽過。真結局要三邊都沒有結仇，而且你看過線索 A 和 B。</p>",
                new object[]
                {
                    new { id = "end_d", text = "改寫規則", cost = "" },
                    new { id = "end_a", text = "仍選擇摧毀", cost = "" },
                },
                "刻度塔", 30, 10),

            "end_a" => new(
                "<p>守護結局。聖杯毀掉。城市失去那台回收裝置供的能源。人還在。</p>",
                new object[] { new { id = "wake", text = "結束", cost = "" } },
                "刻度塔"),

            "end_b" => new(
                "<p>研究結局。聖杯留下，改成公開的許願系統。誰能許，還沒有寫進規則。</p>",
                new object[] { new { id = "wake", text = "結束", cost = "" } },
                "刻度塔"),

            "end_c" => new(
                "<p>奔跑結局。願望歸屬改由賽事決定。聖杯還在。</p>",
                new object[] { new { id = "wake", text = "結束", cost = "" } },
                "刻度塔"),

            "end_d" => new(
                "<p>真結局。核心被改寫。願望不再換成某一區的消失，改成要由還在城裡的人一起承擔。</p><p>這一結局沒有檢查好感度。企劃書寫要三邊達標，數值還沒接。</p>",
                new object[] { new { id = "wake", text = "結束", cost = "" } },
                "刻度塔"),

            "night" => new(
                "<p>居住區。令咒還是三道。遐蝶說今晚不要用。</p>",
                new object[]
                {
                    new { id = "station", text = "去開拓列車站", cost = "30分" },
                    new { id = "seals", text = "看令咒", cost = "" },
                },
                "居住區", 30, 5),

            "seals" => new(
                "<p>三道都在就表示還沒用過。一劃一次，用掉消失。</p>",
                new object[] { new { id = "night", text = "收回", cost = "" } },
                "居住區"),

            _ => new(
                "<p>刻度塔亮起。你是星。手背沒有印記，身邊沒有 Servant。</p>",
                new object[]
                {
                    new { id = "noseal", text = "確認手背", cost = "" },
                    new { id = "rite1", text = "前往刻度塔佈陣", cost = "20分" },
                },
                "居住區"),
        };
    }
}
