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
        private string _saveName = "";
        private string _mode = "basic";
        private string _difficulty = "normal";
        private GameState _g = new();
        private static readonly Random Dice = new();

        public Form1()
        {
            InitializeComponent();
            Load += async (_, _) => await InitWebAsync();
        }

        private async Task InitWebAsync()
        {
            await webView21.EnsureCoreWebView2Async(null);
            webView21.CoreWebView2.WebMessageReceived += OnWebMessage;
            webView21.CoreWebView2.NavigationCompleted += async (_, args) =>
            {
                if (!args.IsSuccess) return;
                var path = webView21.Source?.AbsolutePath ?? "";
                if (path.EndsWith("start.html") && _hasCharacter)
                    await Exec($"window.showPicked({JsonSerializer.Serialize(_playerName)})");
                else if (path.EndsWith("feats.html"))
                    await Exec($"window.setFeats({JsonSerializer.Serialize(_feats)})");
                else if (path.EndsWith("load.html"))
                    await PushSlotsAsync("load");
                else if (path.EndsWith("ui.html"))
                    await PushStateAsync();
            };
            Go("start.html");
        }

        private void Go(string file) =>
            webView21.CoreWebView2.Navigate(new Uri(Path.Combine(AppContext.BaseDirectory, file)).AbsoluteUri);

        private bool IsPage(string file) => (webView21.Source?.AbsolutePath ?? "").EndsWith(file);

        private Task<string> Exec(string script) => webView21.CoreWebView2.ExecuteScriptAsync(script);

        private Task Toast(string text) => Exec($"window.toast && window.toast({JsonSerializer.Serialize(text)})");

        private static string Str(JsonElement root, string name, string fallback) =>
            root.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() ?? fallback : fallback;

        private async void OnWebMessage(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                using var doc = JsonDocument.Parse(e.WebMessageAsJson);
                var root = doc.RootElement;
                var type = root.GetProperty("type").GetString();
                switch (type)
                {
                    case "nav":
                    case "cmd":
                        {
                            var key = type == "nav" ? Str(root, "page", "start") : Str(root, "cmd", "start");
                            bool inGame = IsPage("ui.html");
                            if (inGame && key is "save" or "load")
                            {
                                await PushSlotsAsync(key);
                                break;
                            }
                            Go(key switch
                            {
                                "create" => "create.html",
                                "gallery" => "gallery.html",
                                "feats" => "feats.html",
                                "io" or "settings" => "settings.html",
                                "load" => "load.html",
                                _ => "start.html",
                            });
                        }
                        break;

                    case "create":
                        _characterId = Str(root, "characterId", "stelle");
                        _playerName = Str(root, "playerName", "星");
                        _hasCharacter = true;
                        ApplyCharacter(_characterId);
                        Go("ui.html");
                        break;

                    case "start":
                        _mode = Str(root, "mode", "basic");
                        _difficulty = Str(root, "difficulty", "normal");
                        _saveName = Str(root, "saveName", "");
                        if (!_hasCharacter) { Go("create.html"); break; }
                        ApplyCharacter(_characterId);
                        Go("ui.html");
                        break;

                    case "feats":
                        _feats = root.GetProperty("ids").EnumerateArray()
                            .Select(x => x.GetString() ?? "").Where(x => x.Length > 0).ToList();
                        Go("start.html");
                        break;

                    case "export":
                        var json = JsonSerializer.Serialize(new
                        {
                            playerName = _playerName,
                            characterId = _characterId,
                            portrait = _portrait,
                            feats = _feats,
                        });
                        await Exec($"window.showExport({JsonSerializer.Serialize(json)})");
                        break;

                    case "import":
                        try
                        {
                            using var imported = JsonDocument.Parse(Str(root, "text", "{}"));
                            var box = imported.RootElement;
                            _playerName = Str(box, "playerName", _playerName);
                            _characterId = Str(box, "characterId", _characterId);
                            _portrait = Str(box, "portrait", _portrait);
                            if (box.TryGetProperty("feats", out var f) && f.ValueKind == JsonValueKind.Array)
                                _feats = f.EnumerateArray().Select(x => x.GetString() ?? "").Where(x => x.Length > 0).ToList();
                            _hasCharacter = true;
                        }
                        catch { }
                        Go("start.html");
                        break;

                    case "action":
                        {
                            var actionId = Str(root, "id", "");
                            if (actionId == "restart") ApplyCharacter(_characterId);
                            else Game.Step(_g, actionId);
                            if (actionId == "sleep")
                            {
                                try { SaveStore.Write(0, MakeSave("自動存檔")); } catch { }
                            }
                            await PushStateAsync();
                        }
                        break;

                    case "slotSave":
                        {
                            int slot = root.GetProperty("slot").GetInt32();
                            if (slot < 1 || slot > SaveStore.Slots) break;
                            var name = Str(root, "name", "").Trim();
                            if (name.Length == 0) name = $"{_playerName}・第 {_g.Day} 天";
                            _saveName = name;
                            SaveStore.Write(slot, MakeSave(name));
                            await Toast("已存檔");
                            await PushSlotsAsync("save");
                        }
                        break;

                    case "slotLoad":
                        {
                            int slot = root.GetProperty("slot").GetInt32();
                            var d = SaveStore.Read(slot);
                            if (d == null) { await Toast("這個欄位沒有存檔"); break; }
                            d.State.Normalize();
                            _g = d.State;
                            _characterId = d.CharacterId;
                            _playerName = d.PlayerName;
                            _portrait = d.Portrait;
                            _mode = d.Mode;
                            _feats = d.Feats;
                            _difficulty = d.State.Difficulty;
                            _saveName = d.Name;
                            _hasCharacter = true;
                            if (IsPage("ui.html"))
                            {
                                await Exec("window.closeDialog && window.closeDialog()");
                                await PushStateAsync();
                                await Toast("已讀檔");
                            }
                            else Go("ui.html");
                        }
                        break;

                    case "slotDelete":
                        {
                            int slot = root.GetProperty("slot").GetInt32();
                            if (slot >= 0 && slot <= SaveStore.Slots) SaveStore.Delete(slot);
                            await PushSlotsAsync(Str(root, "mode", "load"));
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                try { await Toast("出了點問題：" + ex.Message); } catch { }
            }
        }

        private SaveData MakeSave(string name) => new()
        {
            Name = name,
            CharacterId = _characterId,
            PlayerName = _playerName,
            Portrait = _portrait,
            Mode = _mode,
            Feats = _feats.ToList(),
            State = _g,
        };

        // 「隨機」模式每次開局都重新抽難度與成就加成
        private void ApplyCharacter(string id)
        {
            var diff = _difficulty;
            var feats = _feats;
            if (_mode == "random")
            {
                diff = new[] { "easy", "normal", "hard" }[Dice.Next(3)];
                feats = new[] { "rich", "tough", "calm" }.Where(_ => Dice.Next(2) == 0).ToList();
            }
            _g.Reset(id, feats, diff);
        }

        private async Task PushStateAsync()
        {
            var v = Game.Render(_g);
            var state = new
            {
                playerName = _playerName,
                portrait = _portrait,
                stats = Game.Hud(_g),
                panels = Game.Panels(_g),
                map = Game.MapData(_g),
                passage = v.Passage,
                actions = v.Choices.Select(c => new { id = c.Id, text = c.Text, cost = c.Cost }).ToArray(),
            };
            await Exec($"window.setState({JsonSerializer.Serialize(state)})");
        }

        private async Task PushSlotsAsync(string mode)
        {
            var data = new
            {
                mode,
                slots = SaveStore.List(),
                name = string.IsNullOrWhiteSpace(_saveName) ? $"{_playerName}・第 {_g.Day} 天" : _saveName,
            };
            await Exec($"window.showSlots({JsonSerializer.Serialize(data)})");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var list = new list();
            Hide();
            list.ShowDialog();
        }
    }
}
