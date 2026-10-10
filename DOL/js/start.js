(function () {
  var help = {
    basic: '<span class="gold">基礎開局：</span>標準難度，直接開始。<span class="teal">推薦初學者選擇。</span>',
    custom: '<span class="gold">自定義開局：</span>可以自己選難度。成就加成請到上面的「成就加成」勾選。',
    random: '<span class="gold">隨機開局：</span>每次開局隨機抽難度與成就加成，重新開始時也會重抽。'
  };
  function send(msg) {
    if (window.chrome && window.chrome.webview) window.chrome.webview.postMessage(msg);
  }
  function currentMode() {
    var b = document.querySelector(".mode.on");
    return b ? b.getAttribute("data-mode") : "basic";
  }
  document.querySelectorAll(".mode").forEach(function (btn) {
    btn.addEventListener("click", function () {
      document.querySelectorAll(".mode").forEach(function (b) { b.classList.remove("on"); });
      btn.classList.add("on");
      var mode = btn.getAttribute("data-mode");
      document.getElementById("mode-help").innerHTML = help[mode] || "";
      document.getElementById("custom-box").hidden = mode !== "custom";
    });
  });
  document.querySelectorAll("[data-cmd]").forEach(function (btn) {
    btn.addEventListener("click", function () {
      send({ type: "cmd", cmd: btn.getAttribute("data-cmd") });
    });
  });
  document.getElementById("btn-start").addEventListener("click", function () {
    var mode = currentMode();
    send({
      type: "start",
      saveName: document.getElementById("save-name").value.trim(),
      mode: mode,
      difficulty: mode === "custom" ? document.getElementById("difficulty").value : "normal"
    });
  });
  // C# 在回到標題頁時呼叫，顯示目前選好的角色
  window.showPicked = function (name) {
    document.getElementById("picked").textContent = "目前角色：" + name;
  };
})();
