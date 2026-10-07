
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
        private string _portrait = "html_img/cyrene.jpg";
        private string _scene = "wake";
        private List<string> _feats = new();
        private int _money = 184;
        private int _stamina = 78;
        private int _stress = 30;
        private int _injury = 0;
        private int _will = 88;

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
                    var id = root.GetProperty("id").GetString();
                    _scene = id switch
                    {
                        "window" => "window",
                        "dress" => "dress",
                        "bed" => "wake",
                        "downstairs" => "dress",
                        "watch" => "watch",
                        _ => _scene
                    };
                    await PushStateAsync();
                    break;
            }
        }
        //角色數值要增加就加if
        private void ApplyCharacter(string id)
        {
            if (id == "stelle")
            {
                _money = 184;
                _stamina = 78;
                _stress = 30;
                _injury = 0;
                _will = 88;
                _portrait = "html_img/start.jpg";
                return;
            }
            if (_feats.Contains("rich")) _money += 200;
            if (_feats.Contains("tough")) { _stamina = 90; _injury = 0; }
            if (_feats.Contains("calm")) { _stress = 10; _will = 95; }

            _money = 100;
            _stamina = 70;
            _stress = 40;
            _injury = 0;
            _will = 70;
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
            var actions = Array.Empty<object>();
            //passage 下包都是劇情文字
            if (_scene == "wake")
            {
                passage = "<p>你醒在一間不算熟悉的房間。星核還在，記憶卻對不上這座城。</p>";
                var list = new List<object>
    {
        new { id = "dress", text = "先把自己整理好", cost = "15分" },
        new { id = "window", text = "看窗外", cost = "" },
    };
                if (_characterId == "stelle")
                    list.Add(new { id = "watch", text = "先觀察，不急著出門", cost = "" });
                actions = list.ToArray();
            }
            else if (_scene == "watch")
            {
                passage = "<p>你在門口站了一會。走廊安靜，沒有列車的廣播，也沒有學園的鐘。</p>";
                actions = new object[]
                {
        new { id = "dress", text = "還是去整理裝備", cost = "10分" },
        new { id = "window", text = "再看一次窗外", cost = "" },
                };
            }
            else
            {
                passage = "<p>未完成的場景。</p>";
            }

            var state = new
            {
                playerName = _playerName,
                portrait = _portrait,
                stats = new Dictionary<string, object>
                {

                    ["money"] = new { value = "$" + _money, note = "", bar = 0 },
                    ["stamina"] = new { value = _stamina + " / 100", note = "即興還撐得住", bar = _stamina },
                    ["stress"] = new { value = _stress + " / 100", note = "", bar = _stress },
                    ["injury"] = new { value = _injury + " / 100", note = "", bar = _injury },
                    ["will"] = new { value = _will + " / 100", note = "星核還在", bar = _will },
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

