
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

        private async void OnWebMessage(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            using var doc = JsonDocument.Parse(e.WebMessageAsJson);
            var root = doc.RootElement;
            if (root.GetProperty("type").GetString() != "action") return;

            var id = root.GetProperty("id").GetString();
            _scene = id switch
            {
                "window" => "window",
                "dress" => "dress",
                "bed" => "wake",
                "downstairs" => "dress",
                _ => _scene
            };
            await PushStateAsync();
        }
        private async Task InitWebView()
        {
            await webView21.EnsureCoreWebView2Async(null);

            webView21.CoreWebView2.WebMessageReceived += OnWebMessage;

            webView21.CoreWebView2.NavigationCompleted += async (s, e) =>
            {
                if (!e.IsSuccess) return;
                await PushStateAsync();
            };

            var path = Path.Combine(AppContext.BaseDirectory, "ui.html");
            webView21.CoreWebView2.Navigate(new Uri(path).AbsoluteUri);
        }
        private string _scene = "wake";
        //推畫面
        private async Task PushStateAsync()
        {
            string passage;
            var actions = Array.Empty<object>();
            //passage 下包都是劇情文字
            if (_scene == "wake")
            {
                passage = "<p>鬧鐘響了第二次。你還沒決定要不要起床。</p>";
                actions = new object[]
                {
            new { id = "dress",  text = "換上制服下樓", cost = "15分" },
            new { id = "window", text = "看窗外",       cost = "" },
                };
            }
            else if (_scene == "window")
            {
                passage = "<p>窗外是還沒亮完的天。樓下已經有人在走了。</p>";
                actions = new object[]
                {
            new { id = "dress", text = "還是去換衣服", cost = "10分" },
            new { id = "bed",   text = "再躺五分鐘",   cost = "" },
                };
            }
            else if (_scene == "dress")
            {
                passage = "<p>你把制服從椅背上拿起來。</p>";
                actions = new object[]
                {
            new { id = "downstairs", text = "下樓", cost = "5分" },
                };
            }
            else
            {
                passage = "<p>未完成的場景。</p>";
            }

            var state = new
            {
                playerName = "主角",
                stats = new Dictionary<string, object>
                {
                    ["time"] = new { value = "週五 07:12", note = "天氣：陰", bar = 30 },
                    ["location"] = new { value = "宿舍房間", note = "", bar = 0 },
                    ["money"] = new { value = "$184", note = "", bar = 0 },
                    ["stamina"] = new { value = "72 / 100", note = "", bar = 72 },
                    ["fatigue"] = new { value = "38 / 100", note = "", bar = 38 },
                    ["stress"] = new { value = "45 / 100", note = "", bar = 45 },
                    ["injury"] = new { value = "5 / 100", note = "", bar = 5 },
                    ["will"] = new { value = "80 / 100", note = "", bar = 80 },
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

