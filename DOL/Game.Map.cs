namespace DOL
{
    // 地圖：地點座標、移動時間、營業與在場時間。UI 端用這些資料畫 DoL 式的可點擊地圖。
    public static partial class Game
    {
        private sealed record MapNode(string Id, string Name, int X, int Y);

        private static readonly MapNode[] Nodes =
        {
            new("home", "居住區", 300, 215),
            new("shop", "便利商店", 388, 292),
            new("trail", "開拓列車站", 105, 125),
            new("school", "學園都市", 300, 55),
            new("track", "賽場", 505, 135),
            new("tower", "刻度塔", 520, 300),
        };

        private static readonly (string A, string B)[] Roads =
        {
            ("home", "shop"), ("home", "trail"), ("home", "school"), ("home", "track"),
            ("trail", "school"), ("school", "track"), ("track", "tower"), ("shop", "tower"),
        };

        private static string PlaceId(string place) => place switch
        {
            "居住區" => "home",
            "便利商店" => "shop",
            "開拓列車站" => "trail",
            "學園都市" or "學園祭廣場" => "school",
            "賽場" or "賽道" => "track",
            "刻度塔" => "tower",
            _ => "home",
        };

        private static MapNode NodeOf(string id) => Nodes.First(n => n.Id == id);

        /// <summary>依地圖上的距離算出分鐘數（每 5 分鐘一格，最少 10 分）。</summary>
        public static int TravelMinutes(string fromPlace, string toId)
        {
            var a = NodeOf(PlaceId(fromPlace));
            var b = NodeOf(toId);
            double dist = Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));
            return Math.Max(10, (int)Math.Round(dist / 7.0 / 5.0) * 5);
        }

        private static int TravelStamina(GameState g, int minutes) => Math.Max(1, minutes / 15) + (g.Raining ? 2 : 0);

        private static string TravelLabel(GameState g, string toId) => T(TravelMinutes(g.Place, toId));

        // 學園的人白天才在，賽場的人早一點到、也早一點收；列車組住在車上，隨時找得到
        private static bool Present(GameState g, string k) => k switch
        {
            "school" => g.Hour >= 8 && g.Hour < 20,
            "track" => g.Hour >= 6 && g.Hour < 20,
            _ => true,
        };

        // 只有在日常畫面才能離開，劇情進行到一半不能直接跑去別的地方
        public static bool CanTravel(GameState g) =>
            g.Has("summoned") && g.Scene is "home" or "loc_trail" or "loc_school" or "loc_track" or "loc_shop";

        public static object MapData(GameState g)
        {
            string here = PlaceId(g.Place);
            bool can = CanTravel(g);
            int due = DueChapter(g);

            var nodes = Nodes.Select(n =>
            {
                bool isHere = n.Id == here;
                string people = n.Id switch
                {
                    "home" => g.HasServant ? "遐蝶" : "",
                    "shop" => "店長",
                    "trail" => "三月七・丹恆・姬子",
                    "school" => Present(g, "school") ? "日奈" : "日奈（不在）",
                    "track" => Present(g, "track") ? "帝王" : "帝王（不在）",
                    _ => "",
                };
                string reason = "";
                if (!isHere)
                {
                    if (n.Id == "shop" && !g.ShopOpen) reason = "已打烊";
                    else if (n.Id == "tower" && due == 0) reason = "沒事可做";
                    else if (n.Id != "home" && g.Stamina < 3) reason = "太累了";
                }
                return (object)new
                {
                    id = n.Id,
                    name = n.Name,
                    x = n.X,
                    y = n.Y,
                    people,
                    here = isHere,
                    minutes = isHere ? 0 : TravelMinutes(g.Place, n.Id),
                    locked = reason.Length > 0,
                    reason,
                    note = n.Id == "tower" && due > 0 ? $"第 {due} 章" : "",
                };
            }).ToList();

            return new
            {
                here,
                canTravel = can,
                nodes,
                edges = Roads.Select(r => new[] { r.A, r.B }).ToList(),
            };
        }
    }
}
