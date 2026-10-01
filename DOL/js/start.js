(function () {
  var help = {
    basic: '<span class="gold">基礎開局：</span>你可以對遊戲難度、xp預設以及 NPC 性別進行大範圍調整。所有 NPC 都將根據這些設置隨機生成。<span class="teal">推薦初學者選擇。</span>',
    custom: '<span class="gold">自定義開局：</span>之後再接詳細設定。現在點了只是換說明。',
    random: '<span class="gold">隨機開局：</span>之後再接隨機規則。現在點了只是換說明。'
  };
  function send(msg) {
    if (window.chrome && window.chrome.webview) window.chrome.webview.postMessage(msg);
  }
  document.querySelectorAll(".mode").forEach(function (btn) {
    btn.addEventListener("click", function () {
      document.querySelectorAll(".mode").forEach(function (b) { b.classList.remove("on"); });
      btn.classList.add("on");
      document.getElementById("mode-help").innerHTML = help[btn.getAttribute("data-mode")] || "";
    });
  });
  document.querySelectorAll("[data-cmd]").forEach(function (btn) {
    btn.addEventListener("click", function () {
      send({ type: "cmd", cmd: btn.getAttribute("data-cmd") });
    });
  });
  document.getElementById("btn-start").addEventListener("click", function () {
    var name = document.getElementById("save-name").value.trim();
    var modeBtn = document.querySelector(".mode.on");
    send({
      type: "start",
      saveName: name,
      mode: modeBtn ? modeBtn.getAttribute("data-mode") : "basic"
    });
  });
})();