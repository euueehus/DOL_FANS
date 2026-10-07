(function () {
    function send(msg) {
        if (window.chrome && window.chrome.webview) window.chrome.webview.postMessage(msg);
    }
    document.getElementById("btn-back").addEventListener("click", function () {
        send({ type: "nav", page: "start" });
    });
    document.getElementById("btn-export").addEventListener("click", function () {
        send({ type: "export" });
    });
    document.getElementById("btn-import").addEventListener("click", function () {
        send({ type: "import", text: document.getElementById("blob").value });
    });
    window.showExport = function (text) { document.getElementById("blob").value = text; };
})();