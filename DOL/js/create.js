(function () {
  function send(msg) {
    if (window.chrome && window.chrome.webview) window.chrome.webview.postMessage(msg);
  }

  document.querySelectorAll(".pick").forEach(function (card) {
    card.addEventListener("click", function () {
      document.querySelectorAll(".pick").forEach(function (c) { c.classList.remove("on"); });
      card.classList.add("on");
      if (card.getAttribute("data-id") === "stelle") {
        document.getElementById("player-name").value = "星";
      }
    });
  });

  document.getElementById("btn-back").addEventListener("click", function () {
    send({ type: "nav", page: "start" });
  });

  document.getElementById("btn-ok").addEventListener("click", function () {
    var card = document.querySelector(".pick.on");
    send({
      type: "create",
      characterId: card ? card.getAttribute("data-id") : "stelle",
      playerName: document.getElementById("player-name").value.trim() || "星"
    });
  });
})();