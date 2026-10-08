using Microsoft.Web.WebView2.Core;
using System.Text.Json;

namespace DOL
{
    public partial class Form1 : Form
    {
        private string _characterId = "stelle";
        private string _playerName = "星";
        private string _portrait = "html_img/start.jpg";
        private string _scene = "wake";
        private string _servant = "無";
        private string _place = "居住區";
        private bool _hasCharacter;
        private bool _hasServant;
        private bool _hasSeals;
        private List<string> _feats = new();
        private int _money = 120;
        private int _stamina = 60;
        private int _stress = 20;
        private int _injury = 0;
        private int _will = 60;
        private int _fatigue;
        private int _minutes = 7 * 60;
        private int _day = 1;
        private int _seals;

        public Form1()
        {
            InitializeComponent();
            Load += async (_, _) => await InitWebView();
        }

        private void Go(string file)
        {
            var path = Path.Combine(AppContext.BaseDirectory, file);
            webView21.CoreWebView2.Navigate(new Uri(path).AbsoluteUri);
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
            var start = Path.Combine(AppContext.BaseDirectory, "start.html");
            webView21.CoreWebView2.Navigate(new Uri(start).AbsoluteUri);
        }

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
                case "start":
                    if (!_hasCharacter) { Go("create.html"); break; }
                    _scene = "wake";
                    Go("ui.html");
                    break;
                case "feats":
                    _feats = root.GetProperty("ids").EnumerateArray()
                        .Select(x => x.GetString() ?? "").Where(x => x.Length > 0).ToList();
                    if (_hasCharacter) ApplyCharacter(_characterId);
                    Go("start.html");
                    break;
                case "export":
                    var json = JsonSerializer.Serialize(new
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

        private void ApplyCharacter(string id)
        {
            _money = 120;
            _stamina = 60;
            _stress = 20;
            _injury = 0;
            _will = 60;
            _hasServant = false;
            _hasSeals = false;
            _seals = 0;
            _servant = "無";
            _place = "居住區";
            _scene = "wake";
            _minutes = 7 * 60;
            _day = 1;
            switch (id)
            {
                case "stelle": _stamina = 60; _will = 60; break;
                case "march": _stamina = 40; _will = 60; break;
                case "sensei": _stamina = 40; _will = 80; break;
                case "hina": _stamina = 80; _will = 60; break;
                case "trainer": _stamina = 60; _will = 80; break;
                case "teio": _stamina = 80; _will = 40; break;
                case "herta": _stamina = 20; _will = 60; break;
            }
            if (_feats.Contains("rich")) _money += 200;
            if (_feats.Contains("tough")) { _stamina = 90; _injury = 0; }
            if (_feats.Contains("calm")) { _stress = 10; _will = 95; }
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

        private string Clock() => $"{_minutes / 60:00}:{_minutes % 60:00}";

        private void ApplyAction(string id)
        {
            switch (id)
            {
                case "noseal": _scene = "noseal"; break;
                case "seals": _scene = "seals"; break;
                case "tower":
                    _scene = "rite1";
                    _place = "刻度塔";
                    _hasSeals = true;
                    _seals = 3;
                    Pass(20, 5);
                    break;
                case "rite2": _scene = "rite2"; Pass(10, 0); break;
                case "rite3": _scene = "rite3"; Pass(10, 5); break;
                case "rite4": _scene = "rite4"; Pass(10, 5); break;
                case "rite5": _scene = "rite5"; Pass(10, 5); break;
                case "rite6":
                    _scene = "summoned";
                    _hasServant = true;
                    _servant = "遐蝶";
                    Pass(10, 5);
                    break;
                case "ask":
                    _scene = _hasServant ? "castorice" : "noseal";
                    if (_hasServant) Pass(20, 5);
                    break;
                case "station":
                    _scene = "station";
                    _place = "開拓列車站";
                    Pass(30, 8);
                    break;
                case "meet": _scene = "meet"; Pass(20, 5); break;
                case "ally": _scene = "ally"; Pass(20, 0); break;
                case "decline": _scene = "decline"; Pass(10, 0); break;
                case "shop":
                    if (_minutes / 60 < 8 || _minutes / 60 >= 22) { _scene = "closed"; break; }
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
                case "back":
                    _scene = "night";
                    _place = "居住區";
                    Pass(30, 5);
                    break;
                default:
                    _scene = "wake";
                    break;
            }
        }

        private async Task PushStateAsync()
        {
            string passage;
            object[] actions;
            switch (_scene)
            {
                case "noseal":
                    passage = "<p>手背沒有印記。令咒是聖杯選中御主的證明，聖杯還沒有選你。</p>";
                    actions = new object[] { new { id = "tower", text = "前往刻度塔", cost = "20分" } };
                    break;
                case "rite1":
                    passage = "<p>塔底的環是暗的。六格刻度停在零：筋力、耐久、敏捷、魔力、幸運、寶具。</p><p>你沿舊線把環描亮，用的是自己的魔力。</p><p>環亮起的瞬間，手背燒了起來。聖杯選中了你，三道令咒浮現，一道不缺。</p>";
                    actions = new object[] { new { id = "rite2", text = "退到環外，開始詠唱", cost = "10分" } };
                    break;
                case "rite2":
                    passage = "<p>環心是空的。召喚要先把邊界說清楚。</p><p>「以石為基，以銀為引。四方之門閉鎖，循環自此起轉。」</p><p>「聖杯在上。要來，就來一個願意應約的人。」</p>";
                    actions = new object[] { new { id = "rite3", text = "第二遍詠唱，把環封上", cost = "10分" } };
                    break;
                case "rite3":
                    passage = "<p>六格同時轉。冷氣從環外壓進來，像門後有重量，還沒有形狀。</p><p>「我在此宣告。願你以劍立於我名之下，我以命運託付於你。」</p><p>光開始聚成人形。還不是臉。</p>";
                    actions = new object[] { new { id = "rite4", text = "維持詠唱，等輪廓", cost = "10分" } };
                    break;
                case "rite4":
                    passage = "<p>裙襬先從光裡落下，然後是手，然後是眼睛。她站在環內，沒有踏線。</p><p>刻度停住。英靈已經現身，契約還沒有成立。</p><p>她看向你：「問你。你就是召喚我的Master嗎？」</p>";
                    actions = new object[] { new { id = "rite5", text = "回答：是", cost = "10分" } };
                    break;
                case "rite5":
                    passage = "<p>她看了一眼你手背的令咒，像在確認聖杯的選擇沒有錯。</p><p>「Servant，Caster。應召而來。」</p><p>魔力沿著看不見的線從你流向她。契約成立。</p>";
                    actions = new object[] { new { id = "rite6", text = "確認契約", cost = "10分" } };
                    break;
                case "summoned":
                    passage = "<p>令咒沒有變化，三道都在。你看得見她的狀態面板：職階 Caster，真名 遐蝶。</p><p>「真名不要對外說。」遐蝶說。「令咒是對我的強制，用一道就少一道。離開環吧，塔還在讀這份契約。」</p>";
                    actions = new object[]
                    {
                        new { id = "ask", text = "離開環，問令咒的用法", cost = "20分" },
                        new { id = "station", text = "帶她離開刻度塔", cost = "30分" },
                    };
                    break;
                case "seals":
                    passage = _hasSeals
                        ? $"<p>令咒 {_seals} 道還在。一劃是一次絕對命令，用掉就少一劃。</p>"
                        : "<p>手背沒有印記。</p>";
                    actions = new object[]
                    {
                        new { id = "ask", text = "問遐蝶", cost = "20分" },
                        new { id = "station", text = "去開拓列車站", cost = "30分" },
                    };
                    break;
                case "castorice":
                    passage = "<p>遐蝶看著你手背的三道。</p><p>「不是三種法術。一劃，一次絕對命令。可以強迫我做得到的事，也可以拿來強化、補魔。不用咒文。你下令，它就燒一劃。」</p><p>「三劃都用完，你就不再是能命令我的御主。所以不要試。」</p>";
                    actions = new object[]
                    {
                        new { id = "back", text = "先回居住區", cost = "30分" },
                        new { id = "station", text = "去開拓列車站", cost = "30分" },
                    };
                    break;
                case "station":
                    passage = "<p>開拓列車站沒有列車進站。公告只寫：刻度塔異動。</p><p>月台邊有人在拍照。她手背上的印記，跟你的是同一種。</p>";
                    actions = new object[]
                    {
                        new { id = "meet", text = "走過去", cost = "20分" },
                        new { id = "shop", text = "不接觸，去便利商店打工", cost = "2小時" },
                    };
                    break;
                case "meet":
                    passage = "<p>拍照的人先開口：「你也有？我叫三月七。這是丹恆。」</p><p>丹恆站在她側後方。他看了一眼遐蝶，沒有問真名。</p><p>三月七舉起手背：「今天早上才出現的。我們也不知道塔為什麼亮。」</p>";
                    actions = new object[]
                    {
                        new { id = "ally", text = "說你們也是剛完成召喚", cost = "20分" },
                        new { id = "decline", text = "只聽，不表明身分", cost = "10分" },
                    };
                    break;
                case "ally":
                    passage = "<p>遐蝶沒有報職階。三月七也沒有逼。</p><p>丹恆說：「公告之外還有一條。七組。塔會繼續叫人，叫滿為止。」</p><p>「令咒不要在街上亮。我們先回列車組。」</p>";
                    actions = new object[]
                    {
                        new { id = "back", text = "回居住區", cost = "30分" },
                        new { id = "shop", text = "去便利商店打工", cost = "2小時" },
                    };
                    break;
                case "decline":
                    passage = "<p>三月七把相機放下。「行。那我們當沒見過。」</p><p>丹恆經過時只留一句：「令咒不要對著路人。」</p>";
                    actions = new object[]
                    {
                        new { id = "shop", text = "去便利商店打工", cost = "2小時" },
                        new { id = "back", text = "回居住區", cost = "30分" },
                    };
                    break;
                case "shop":
                    passage = "<p>便利商店一班結束。錢進帳。</p>";
                    actions = new object[]
                    {
                        new { id = "rest", text = "回居住區休息", cost = "2小時" },
                        new { id = "station", text = "再去列車站", cost = "30分" },
                    };
                    break;
                case "rest":
                    passage = "<p>居住區很安靜。體力回來一點。</p>";
                    actions = new object[]
                    {
                        new { id = "station", text = "去開拓列車站", cost = "30分" },
                        new { id = "seals", text = "再看一次令咒", cost = "" },
                    };
                    break;
                case "night":
                    passage = "<p>居住區沒有第二個人跟著進來。遐蝶站在門內側。</p><p>令咒還是三道。她說今晚不要用。</p>";
                    actions = new object[]
                    {
                        new { id = "rest", text = "休息", cost = "2小時" },
                        new { id = "seals", text = "再確認手背", cost = "" },
                    };
                    break;
                case "closed":
                    passage = "<p>便利商店這時段沒開。8:00 到 22:00。</p>";
                    actions = new object[]
                    {
                        new { id = "station", text = "去開拓列車站", cost = "30分" },
                        new { id = "rest", text = "回居住區", cost = "2小時" },
                    };
                    break;
                default:
                    passage = "<p>刻度塔亮起。你是星。手背沒有印記，聖杯還沒有選中你，身邊也沒有Servant。</p><p>去塔底，等聖杯回應。</p>";
                    actions = new object[]
                    {
                        new { id = "noseal", text = "確認手背", cost = "" },
                        new { id = "tower", text = "前往刻度塔佈陣", cost = "20分" },
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
                    ["will"] = new { value = _will + " / 100", note = _hasSeals ? "令咒 " + _seals : "", bar = _will },
                },
                passage,
                actions,
            };
            var json = JsonSerializer.Serialize(state);
            await webView21.CoreWebView2.ExecuteScriptAsync($"window.setState({json})");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var list = new list();
            Hide();
            list.ShowDialog();
        }
    }
}