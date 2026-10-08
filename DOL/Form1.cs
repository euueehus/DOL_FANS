using Microsoft.Web.WebView2.Core;
using System.Text.Json;

namespace DOL
{
    public partial class Form1 : Form
    {
        private string _characterId = "stelle";
        private string _playerName = "星";
        private string _portrait = "html_img/start.jpg";
        private bool _hasCharacter;
        private List<string> _feats = new();
        private readonly GameState _g = new();

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
                    Go("ui.html");
                    break;
                case "start":
                    if (!_hasCharacter) { Go("create.html"); break; }
                    ApplyCharacter(_characterId);
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
                    var actionId = root.GetProperty("id").GetString() ?? "";
                    if (actionId == "restart") ApplyCharacter(_characterId);
                    else Game.Step(_g, actionId);
                    await PushStateAsync();
                    break;
            }
        }

        private void ApplyCharacter(string id) => _g.Reset(id, _feats);

        private async Task PushStateAsync()
        {
            var v = Game.Render(_g);
            var state = new
            {
                playerName = _playerName,
                portrait = _portrait,
                stats = Game.Hud(_g),
                panels = Game.Panels(_g),
                passage = v.Passage,
                actions = v.Choices.Select(c => new { id = c.Id, text = c.Text, cost = c.Cost }).ToArray(),
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