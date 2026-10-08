
using Microsoft.Web.WebView2.Core;
using System.Text.Json;
namespace DOL
//第一次用webview註解較多
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            Load += async (_, _) => await InitWebView();

        }

        private void webView21_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private string _characterId = "stelle";
        private string _playerName = "星";
        private string _servant = "遐蝶";
        private string _place = "開拓列車站";
        private string _portrait = "html_img/start.jpg";
        private string _scene = "wake";
        private List<string> _feats = new();
        private int _money = 184;
        private int _stamina = 78;
        private int _stress = 30;
        private int _injury = 0;
        private int _will = 88;
        

        private int _slot;
        private int _fatigue;
        private int _minutes = 7 * 60; 
        private int _day = 1;
        private static readonly string[] Slots = { "早晨", "上午", "午後", "傍晚", "夜晚", "深夜" };
        private bool _hasServant;
        private string _servant = "無";

        private void Spend(int slots, int stamina, int fatigue = 5)
        {
            _slot += slots;
            _stamina -= stamina;
            _fatigue += fatigue;
            if (_stamina < 0) _stamina = 0;
            if (_fatigue > 100) _fatigue = 100;
            if (_fatigue < 0) _fatigue = 0;
            if (_slot >= 6)
            {
                _slot = 0;
                _stamina = Math.Min(100, _stamina + 30);
                _fatigue = Math.Max(0, _fatigue - 20);
                _stress = Math.Max(0, _stress - 5);
            }
        }

        
        private void Pass(int minutes, int stamina = 0)
        {
            if (minutes < 0) minutes = 0;
            _minutes += minutes;
            _stamina -= stamina;
            _fatigue += minutes / 2;
            if (_stamina < 0) _stamina = 0;
            if (_fatigue > 100) _fatigue = 100;

            while (_minutes >= 24 * 60)
            {
                _minutes -= 24 * 60;
                _day++;
                _stamina = Math.Min(100, _stamina + 20);
                _fatigue = Math.Max(0, _fatigue - 30);
                _stress = Math.Max(0, _stress - 5);
            }
        }

        private string Clock()
        {
            return $"{_minutes / 60:00}:{_minutes % 60:00}";
        }

        private bool Open(int fromHour, int toHour)
        {
            int h = _minutes / 60;
            return h >= fromHour && h < toHour;
        }


        private void ApplyAction(string id)
        {
            switch (id)
            {
                case "look":
                    _scene = "mark";
                    break;
                case "ask":
                    _scene = "castorice";
                    Pass(20, 5);
                    break;
                case "station":
                    _scene = "station";
                    _place = "開拓列車站";
                    Pass(30, 8);
                    break;
                case "shop":
                    if (!Open(8, 22)) { _scene = "closed"; break; }
                    _scene = "shop";
                    _place = "便利商店";
                    _money += 40;
                    Pass(120, 15);
                    break;
                case "rest":
                    _scene = "rest";
                    _place = "居住區";
                    _stamina = Math.Min(100, _stamina + 25);
                    _fatigue = Math.Max(0, _fatigue - 20);
                    Pass(120, 0);
                    break;
                default:
                    _scene = "wake";
                    break;
            }
        }
        private void Go(string file)
        {
            var path = Path.Combine(AppContext.BaseDirectory, file);
            webView21.CoreWebView2.Navigate(new Uri(path).AbsoluteUri);

        }

        private bool _hasCharacter;

        private async void OnWebMessage(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            using var doc = JsonDocument.Parse(e.WebMessageAsJson);
            var root = doc.RootElement;
            var type = root.GetProperty("type").GetString();

            switch (type)
            {
                case "nav":
                case "cmd":
                    var key = type == "nav"
                        ? root.GetProperty("page").GetString()
                        : root.GetProperty("cmd").GetString();
                    Go(key switch
                    {
                        "create" => "create.html",
                        "gallery" => "gallery.html",
                        "feats" => "feats.html",
                        "io" or "settings" => "settings.html",
                        "title" => "start.html",
                        _ => "start.html"
                    });
                    break;

                case "create":
                    _characterId = root.TryGetProperty("characterId", out var cid)
                        ? cid.GetString() ?? "stelle" : "stelle";
                    _playerName = root.TryGetProperty("playerName", out var pn)
                        ? pn.GetString() ?? "星" : "星";
                    _hasCharacter = true;
                    ApplyCharacter(_characterId);
                    _scene = "wake";
                    Go("ui.html");
                    break;
                case "stelle":
                    _servant = "遐蝶";
                    _place = "開拓列車站";
                    _stamina = 60;
                    _will = 60;
                    break;

                case "start":
                    if (!_hasCharacter)
                    {
                        Go("create.html");
                        break;
                    }
                    _scene = "wake";
                    Go("ui.html");
                    break;

                case "feats":
                    _feats = root.GetProperty("ids").EnumerateArray()
                        .Select(x => x.GetString() ?? "")
                        .Where(x => x.Length > 0)
                        .ToList();
                    if (_hasCharacter) ApplyCharacter(_characterId);
                    Go("start.html");
                    break;
                case "export":
                    var json = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        playerName = _playerName,
                        characterId = _characterId,
                        portrait = _portrait,
                        feats = _feats
                    });
                    var escaped = json.Replace("\\", "\\\\").Replace("'", "\\'");
                    await webView21.CoreWebView2.ExecuteScriptAsync($"window.showExport('{escaped}')");
                    break;

                case "import":
                    try
                    {
                        using var imported = JsonDocument.Parse(root.GetProperty("text").GetString() ?? "{}");
                        var box = imported.RootElement;
                        if (box.TryGetProperty("playerName", out var n)) _playerName = n.GetString() ?? _playerName;
                        if (box.TryGetProperty("characterId", out var c)) _characterId = c.GetString() ?? _characterId;
                        if (box.TryGetProperty("portrait", out var p)) _portrait = p.GetString() ?? _portrait;
                        if (box.TryGetProperty("feats", out var f))
                            _feats = f.EnumerateArray().Select(x => x.GetString() ?? "").Where(x => x.Length > 0).ToList();
                        _hasCharacter = true;
                        ApplyCharacter(_characterId);
                    }
                    catch { }
                    Go("start.html");
                    break;

                case "action":
                    ApplyAction(root.GetProperty("id").GetString() ?? "");
                    await PushStateAsync();
                    break;
            }
        }
        //角色數值要增加就加if
        private void ApplyCharacter(string id)
        {
            _money = 120;
            _stamina = 60;
            _stress = 20;
            _injury = 0;
            _will = 60;
            _servant = "搭檔";
            _place = "居住區";

            switch (id)
            {
                case "stelle":
                    _hasServant = false;
                    _servant = "無";
                    _place = "居住區";
                    _stamina = 60;
                    _will = 60;
                    break;
                case "summon":
                    _scene = "summoned";
                    _hasServant = true;
                    _servant = "遐蝶";
                    _place = "刻度塔";
                    Pass(60, 20);
                    break;
                case "ask":
                    if (!_hasServant) { _scene = "noservant"; break; }
                    _scene = "castorice";
                    Pass(20, 5);
                    break;
                case "march":
                    _servant = "丹恆"; _place = "開拓列車站";
                    _stamina = 40; _will = 60; break;
                case "sensei":
                    _servant = "白子"; _place = "教室";
                    _stamina = 40; _will = 80; break;
                case "hina":
                    _servant = "星野"; _place = "社團大樓";
                    _stamina = 80; _will = 60; break;
                case "trainer":
                    _servant = "特別周"; _place = "訓練場";
                    _stamina = 60; _will = 80; break;
                case "teio":
                    _servant = "無聲鈴鹿"; _place = "賽道";
                    _stamina = 80; _will = 40; break;
                case "herta":
                    _servant = "銀狼"; _place = "刻度塔";
                    _stamina = 20; _will = 60; break;
            }

            if (_feats.Contains("rich")) _money += 200;
            if (_feats.Contains("tough")) { _stamina = 90; _injury = 0; }
            if (_feats.Contains("calm")) { _stress = 10; _will = 95; }
        }
        private async Task InitWebView()
        {
            await webView21.EnsureCoreWebView2Async(null);

            webView21.CoreWebView2.WebMessageReceived += OnWebMessage;

            webView21.CoreWebView2.NavigationCompleted += async (s, e) =>
            {
                if (!e.IsSuccess) return;
                var path = webView21.Source?.AbsolutePath ?? "";
                if (path.EndsWith("start.html") && _hasCharacter)
                {
                    var name = _playerName.Replace("'", "");
                    await webView21.CoreWebView2.ExecuteScriptAsync($"window.showPicked('{name}')");
                }
                if (path.EndsWith("ui.html"))
                    await PushStateAsync();
            };

            var path = Path.Combine(AppContext.BaseDirectory, "start.html");
            webView21.CoreWebView2.Navigate(new Uri(path).AbsoluteUri);
        }

        //推畫面
        private async Task PushStateAsync()
        {
            string passage;
            object[] actions;

            switch (_scene)
            {
                case "mark":
                    passage = "<p>三道令咒。還沒有英靈來認這三筆。</p>";
                    actions = new object[]
                    {
        new { id = "summon", text = "前往刻度塔召喚", cost = "1小時" },
                    };
                    break;
                case "castorice":
                    passage = "<p>遐蝶站在你側邊。她確認令咒還有三枚：暫時強化、脫離戰鬥、強制服從一次。</p>";
                    actions = new object[]
                    {
                new { id = "station", text = "去開拓列車站", cost = "1時段" },
                new { id = "shop", text = "先去便利商店打工", cost = "2時段" },
                    };
                    break;
                case "station":
                    passage = "<p>開拓列車站沒有列車進站。公告只寫：刻度塔異動，聖杯即將降臨。</p>";
                    actions = new object[]
                    {
                new { id = "shop", text = "去便利商店打工", cost = "2時段" },
                new { id = "rest", text = "回居住區休息", cost = "2時段" },
                    };
                    break;
                case "shop":
                    passage = "<p>你在便利商店排完一班。錢進帳，腿是沉的。</p>";
                    actions = new object[]
                    {
                new { id = "rest", text = "回居住區休息", cost = "2時段" },
                new { id = "station", text = "再去列車站", cost = "1時段" },
                    };
                    break;
                case "rest":
                    passage = "<p>居住區很安靜。體力回來一點，這一天還沒結束。</p>";
                    actions = new object[]
                    {
                new { id = "station", text = "去開拓列車站", cost = "1時段" },
                new { id = "look", text = "再看一次令咒", cost = "" },
                    };
                    break;
                case "closed":
                    passage = "<p>便利商店這時段沒開。公告寫 8:00 到 22:00。</p>";
                    actions = new object[]
                    {
                    new { id = "station", text = "去開拓列車站", cost = "30分" },
                    new { id = "rest", text = "回居住區", cost = "2小時" },
                    };
                    break;

                case "noservant":
                    passage = "<p>沒有人回應。令咒還在，英靈還沒來。</p>";
                    actions = new object[]
                    {
        new { id = "summon", text = "前往刻度塔召喚", cost = "1小時" },
                    };
                    break;
                case "summoned":
                    passage = "<p>陣中央站著遐蝶。她看了一眼你的令咒，沒有立刻靠近。</p>";
                    actions = new object[]
                    {
        new { id = "ask", text = "問她令咒能做什麼", cost = "20分" },
        new { id = "station", text = "帶她去開拓列車站", cost = "30分" },
                    };
                    break;
                default:
                    passage = "<p>刻度塔亮起。手背浮出三道令咒。身邊還沒有英靈。</p>";
                    actions = new object[]
                    {
        new { id = "look", text = "看手背上的令咒", cost = "" },
        new { id = "summon", text = "前往刻度塔召喚", cost = "1小時" },
                    };
                    break;

        
            }

            var state = new
            {
                playerName = _playerName,
                portrait = _portrait,
                stats = new Dictionary<string, object>
                {
                    ["time"] = new { value = Clock(), note = "第 " + _day + " 天", bar = 0 },
                    ["location"] = new { value = _place, note = _servant, bar = 0 },
                    ["money"] = new { value = "$" + _money, note = "", bar = 0 },
                    ["stamina"] = new { value = _stamina + " / 100", note = "", bar = _stamina },
                    ["stress"] = new { value = _stress + " / 100", note = "", bar = _stress },
                    ["injury"] = new { value = _injury + " / 100", note = "", bar = _injury },
                    ["will"] = new { value = _will + " / 100", note = "令咒還在", bar = _will },
                },
                passage,
                actions,
            };

            var json = System.Text.Json.JsonSerializer.Serialize(state);
            await webView21.CoreWebView2.ExecuteScriptAsync($"window.setState({json})");
        }
        

        private void button1_Click(object sender, EventArgs e)
        {
            list list = new list();
            this.Hide();  //要用hideeeeeeee         
            list.ShowDialog();
        }
    }
}

