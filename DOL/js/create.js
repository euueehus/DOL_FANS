(function () {
    function send(msg) {
        if (window.chrome && window.chrome.webview) window.chrome.webview.postMessage(msg);
    }
    function pick(selector) {
        document.querySelectorAll(selector).forEach(function (btn) {
            btn.addEventListener("click", function () {
                document.querySelectorAll(selector).forEach(function (b) { b.classList.remove("on"); });
                btn.classList.add("on");
            });
        });
    }
    pick("[data-diff]");
    pick("[data-art]");

    document.getElementById("btn-back").addEventListener("click", function () {
        send({ type: "nav", page: "start" });
    });
    document.getElementById("btn-ok").addEventListener("click", function () {
        var diff = document.querySelector("[data-diff].on");
        var art = document.querySelector("[data-art].on");
        send({
            type: "create",
            playerName: document.getElementById("player-name").value.trim() || "主角",
            difficulty: diff ? diff.getAttribute("data-diff") : "normal",
            portrait: art ? art.getAttribute("data-art") : "cyrene"
        });
    });
})();