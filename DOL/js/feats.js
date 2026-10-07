(function () {
    function send(msg) {
        if (window.chrome && window.chrome.webview) window.chrome.webview.postMessage(msg);
    }
    document.getElementById("btn-back").addEventListener("click", function () {
        send({ type: "nav", page: "start" });
    });
    document.getElementById("btn-ok").addEventListener("click", function () {
        var ids = [];
        document.querySelectorAll(".feat input:checked").forEach(function (el) { ids.push(el.value); });
        send({ type: "feats", ids: ids });
    });
})();