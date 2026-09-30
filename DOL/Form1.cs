
using Microsoft.Web.WebView2.Core;
namespace DOL
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
        private async Task InitWebView()
        {
            // Evergreen 傳 null 即可
            await webView21.EnsureCoreWebView2Async(null);

            webView21.NavigateToString("<html><body><h1>Hello</h1></body></html>");

            webView21.CoreWebView2.NavigationCompleted += (s, e) =>
            {
                if (e.IsSuccess)
                    Text = webView21.CoreWebView2.DocumentTitle;
            };
        }
    }
}

