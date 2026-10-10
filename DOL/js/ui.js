(function () {
    "use strict";

    // ───────── 基本工具 ─────────
    function send(msg) {
        if (window.chrome && window.chrome.webview) window.chrome.webview.postMessage(msg);
    }

    function clamp(n) {
        n = Number(n || 0);
        if (n < 0) return 0;
        if (n > 100) return 100;
        return n;
    }

    function colourFromPercent(pct, invert) {
        if (invert) {
            if (pct <= 0) return "green";
            if (pct < 20) return "teal";
            if (pct < 40) return "lblue";
            if (pct < 60) return "blue";
            if (pct < 80) return "purple";
            if (pct < 100) return "pink";
            return "red";
        }
        if (pct <= 0) return "red";
        if (pct < 20) return "pink";
        if (pct < 40) return "purple";
        if (pct < 60) return "blue";
        if (pct < 80) return "lblue";
        if (pct < 100) return "teal";
        return "green";
    }

    function applyBars() {
        document.querySelectorAll(".stat-bar > i").forEach(function (el) {
            var n = clamp(el.getAttribute("data-value"));
            el.style.width = n + "%";
            var bar = el.parentElement;
            var stat = bar.closest(".stat");
            var invert = stat && stat.getAttribute("data-invert") === "1";
            bar.classList.remove("green", "teal", "lblue", "blue", "purple", "pink", "red", "gold");
            bar.classList.add(colourFromPercent(n, invert));
        });
    }

    function replay(el) {
        el.classList.remove("fade");
        void el.offsetWidth;
        el.classList.add("fade");
    }

    function h(tag, cls, text) {
        var e = document.createElement(tag);
        if (cls) e.className = cls;
        if (text !== undefined) e.textContent = text;
        return e;
    }

    // ───────── 畫面設定（只存在這台電腦的瀏覽器儲存空間）─────────
    var KEY = "dol.ui.v1";
    var settings = { font: 1, width: "42em", anim: true, cost: true };

    try {
        var saved = localStorage.getItem(KEY);
        if (saved) Object.assign(settings, JSON.parse(saved));
    } catch (e) { }

    function applySettings() {
        var root = document.documentElement;
        root.style.setProperty("--fs", settings.font);
        root.style.setProperty("--pw", settings.width);
        document.body.classList.toggle("no-fade", !settings.anim);
        document.body.classList.toggle("hide-cost", !settings.cost);
    }

    function saveSettings() {
        try { localStorage.setItem(KEY, JSON.stringify(settings)); } catch (e) { }
        applySettings();
    }

    // ───────── 視窗 ─────────
    var panels = {};
    var mapData = null;
    var current = "";
    var dialog = document.getElementById("dialog");
    var box = document.getElementById("dialog-box");
    var titleEl = document.getElementById("dialog-title");
    var bodyEl = document.getElementById("dialog-body");

    function openDialog(title, wide) {
        titleEl.textContent = title;
        box.classList.toggle("wide", !!wide);
        bodyEl.innerHTML = "";
        dialog.hidden = false;
        return bodyEl;
    }

    function closeDialog() {
        dialog.hidden = true;
        bodyEl.innerHTML = "";
        current = "";
    }
    window.closeDialog = closeDialog;

    var toastTimer = 0;
    window.toast = function (text) {
        var t = document.getElementById("toast");
        t.textContent = text;
        t.classList.add("show");
        clearTimeout(toastTimer);
        toastTimer = setTimeout(function () { t.classList.remove("show"); }, 1800);
    };

    function optionRow(label, items, isOn, pick) {
        var row = h("div", "optrow");
        row.appendChild(h("span", "", label));
        items.forEach(function (it) {
            var b = h("button", "btn" + (isOn(it.val) ? " on" : ""), it.text);
            b.type = "button";
            b.addEventListener("click", function () { pick(it.val); });
            row.appendChild(b);
        });
        return row;
    }

    function renderSettings() {
        var body = openDialog("設定");
        body.appendChild(optionRow("文字大小",
            [{ text: "小", val: 0.9 }, { text: "中", val: 1 }, { text: "大", val: 1.15 }, { text: "特大", val: 1.3 }],
            function (v) { return settings.font === v; },
            function (v) { settings.font = v; saveSettings(); renderSettings(); }));
        body.appendChild(optionRow("文字寬度",
            [{ text: "窄", val: "34em" }, { text: "標準", val: "42em" }, { text: "寬", val: "54em" }],
            function (v) { return settings.width === v; },
            function (v) { settings.width = v; saveSettings(); renderSettings(); }));
        body.appendChild(optionRow("換頁淡入",
            [{ text: "開", val: true }, { text: "關", val: false }],
            function (v) { return settings.anim === v; },
            function (v) { settings.anim = v; saveSettings(); renderSettings(); }));
        body.appendChild(optionRow("顯示時間花費",
            [{ text: "開", val: true }, { text: "關", val: false }],
            function (v) { return settings.cost === v; },
            function (v) { settings.cost = v; saveSettings(); renderSettings(); }));
        body.appendChild(h("p", "muted", "這些只影響畫面。快捷鍵：M 開地圖，Esc 關視窗。"));
    }

    function renderTitleConfirm() {
        var body = openDialog("回到標題？");
        body.appendChild(h("p", "", "還沒存檔的進度會消失。"));
        var row = h("div", "optrow");
        [["先存檔", function () { send({ type: "cmd", cmd: "save" }); }],
        ["直接回標題", function () { send({ type: "cmd", cmd: "title" }); }],
        ["取消", closeDialog]].forEach(function (x) {
            var b = h("button", "btn", x[0]);
            b.type = "button";
            b.addEventListener("click", x[1]);
            row.appendChild(b);
        });
        body.appendChild(row);
    }

    // ───────── 地圖 ─────────
    function renderMap() {
        var body = openDialog("地圖", true);
        if (!mapData) {
            body.appendChild(h("p", "muted", "現在還看不到地圖。"));
            return;
        }
        var NS = "http://www.w3.org/2000/svg";
        var svg = document.createElementNS(NS, "svg");
        svg.setAttribute("viewBox", "0 0 620 360");
        svg.setAttribute("class", "map-svg");

        var byId = {};
        mapData.nodes.forEach(function (n) { byId[n.id] = n; });

        mapData.edges.forEach(function (e) {
            var a = byId[e[0]], b = byId[e[1]];
            if (!a || !b) return;
            var ln = document.createElementNS(NS, "line");
            ln.setAttribute("x1", a.x); ln.setAttribute("y1", a.y);
            ln.setAttribute("x2", b.x); ln.setAttribute("y2", b.y);
            ln.setAttribute("class", "map-road");
            svg.appendChild(ln);
        });

        function text(parent, cls, y, str) {
            var t = document.createElementNS(NS, "text");
            if (cls) t.setAttribute("class", cls);
            t.setAttribute("x", 0);
            t.setAttribute("y", y);
            t.textContent = str;
            parent.appendChild(t);
        }

        mapData.nodes.forEach(function (n) {
            var g = document.createElementNS(NS, "g");
            var state = n.here ? "here" : (n.locked || !mapData.canTravel ? "locked" : "ok");
            g.setAttribute("class", "map-node " + state);
            g.setAttribute("transform", "translate(" + n.x + "," + n.y + ")");

            var r = document.createElementNS(NS, "rect");
            r.setAttribute("x", -58); r.setAttribute("y", -23);
            r.setAttribute("width", 116); r.setAttribute("height", 46);
            r.setAttribute("rx", 6);
            g.appendChild(r);

            text(g, "", -3, n.name);
            var sub = n.here ? "你在這裡" : (n.locked ? n.reason : (n.note ? n.note : n.minutes + " 分"));
            text(g, "sub", 14, sub);
            if (n.people) text(g, "who", 38, n.people);

            if (state === "ok") {
                g.addEventListener("click", function () {
                    closeDialog();
                    send({ type: "action", id: "go_" + n.id });
                });
            }
            svg.appendChild(g);
        });

        body.appendChild(svg);
        body.appendChild(h("p", "muted", mapData.canTravel
            ? "點地點出發。時間和體力依距離與天氣而定，學園和賽場的人只在白天找得到。"
            : "現在不方便離開。"));
    }

    // ───────── 存檔／讀檔 ─────────
    window.showSlots = function (data) {
        current = "slots";
        var isSave = data.mode === "save";
        var body = openDialog(isSave ? "存檔" : "讀檔");

        var input = null;
        if (isSave) {
            body.appendChild(h("span", "gold", "存檔名稱"));
            input = h("input");
            input.id = "save-name-input";
            input.type = "text";
            input.maxLength = 24;
            input.value = data.name || "";
            body.appendChild(input);
        }

        function armed(btn, label, run) {
            btn.addEventListener("click", function () {
                if (btn.getAttribute("data-armed") === "1") { run(); return; }
                btn.setAttribute("data-armed", "1");
                btn.classList.add("armed");
                btn.textContent = label;
                setTimeout(function () {
                    if (!btn.isConnected) return;
                    btn.removeAttribute("data-armed");
                    btn.classList.remove("armed");
                    btn.textContent = btn.getAttribute("data-text");
                }, 2500);
            });
        }

        function button(text) {
            var b = h("button", "btn", text);
            b.type = "button";
            b.setAttribute("data-text", text);
            return b;
        }

        data.slots.forEach(function (s) {
            var row = h("div", "slot");
            var info = h("div", "info");
            var title = h("span", "", "");
            title.appendChild(h("b", "", s.slot === 0 ? "自動" : "欄位 " + s.slot));
            title.appendChild(document.createTextNode(s.empty ? (s.slot === 0 ? "（睡覺時自動存檔）" : "（空）") : s.name));
            info.appendChild(title);
            if (!s.empty) info.appendChild(h("div", "", s.summary + "　" + s.savedAt));
            row.appendChild(info);

            if (isSave && s.slot !== 0) {
                var save = button("存檔");
                var doSave = function () { send({ type: "slotSave", slot: s.slot, name: input.value }); };
                if (s.empty) save.addEventListener("click", doSave);
                else armed(save, "確定覆蓋？", doSave);
                row.appendChild(save);
            }
            if (!isSave) {
                var load = button("讀取");
                load.disabled = s.empty;
                if (!s.empty) load.addEventListener("click", function () { send({ type: "slotLoad", slot: s.slot }); });
                row.appendChild(load);
            }
            if (!s.empty && s.slot !== 0) {
                var del = button("刪除");
                armed(del, "確定刪除？", function () { send({ type: "slotDelete", slot: s.slot, mode: data.mode }); });
                row.appendChild(del);
            }
            body.appendChild(row);
        });
    };

    function openPanel(name) {
        current = name;
        if (name === "journal" || name === "skills") {
            var body = openDialog(name === "journal" ? "日誌" : "角色");
            body.innerHTML = panels[name] || "<p class=\"muted\">（還沒有內容）</p>";
            applyBars();
        } else if (name === "map") {
            renderMap();
        } else if (name === "settings") {
            renderSettings();
        } else if (name === "title") {
            renderTitleConfirm();
        }
    }

    // ───────── 遊戲狀態 ─────────
    window.setState = function (s) {
        function setStat(id, value, note, bar) {
            var el = document.getElementById(id);
            if (!el) return;
            var valueEl = el.querySelector('[data-field="value"]');
            var noteEl = el.querySelector('[data-field="note"]');
            var barEl = el.querySelector(".stat-bar > i");
            if (value !== undefined && valueEl) valueEl.textContent = value;
            if (note !== undefined && noteEl) noteEl.textContent = note;
            if (bar !== undefined && barEl) barEl.setAttribute("data-value", bar);
        }

        if (s.portrait) {
            var img = document.getElementById("portrait");
            if (img) img.src = s.portrait;
        }
        if (s.panels) panels = s.panels;
        if (s.map) mapData = s.map;

        if (s.playerName) document.getElementById("player-name").textContent = s.playerName;
        if (s.stats) {
            Object.keys(s.stats).forEach(function (k) {
                var v = s.stats[k];
                setStat("stat-" + k, v.value, v.note, v.bar);
            });
        }
        if (s.passage) {
            var pt = document.getElementById("passage-text");
            pt.innerHTML = s.passage;
            replay(pt);
        }
        if (s.actions) {
            var ul = document.getElementById("action-list");
            ul.innerHTML = "";
            replay(ul);
            s.actions.forEach(function (a) {
                var li = document.createElement("li");
                var link = document.createElement("a");
                link.href = "#";
                link.setAttribute("data-action", a.id);
                link.textContent = a.text;
                li.appendChild(link);
                if (a.cost) {
                    var c = document.createElement("span");
                    c.className = "cost";
                    c.textContent = "（" + a.cost + "）";
                    li.appendChild(c);
                }
                ul.appendChild(li);
            });
            if (mapData && mapData.canTravel) {
                var mli = document.createElement("li");
                var mlink = document.createElement("a");
                mlink.href = "#";
                mlink.setAttribute("data-panel", "map");
                mlink.textContent = "查看地圖，到別的地方去";
                mli.appendChild(mlink);
                ul.appendChild(mli);
            }
        }
        applyBars();

        // 視窗開著時，日誌、角色、地圖跟著最新狀態更新
        if (!dialog.hidden && (current === "journal" || current === "skills" || current === "map")) openPanel(current);
    };

    // ───────── 事件 ─────────
    document.addEventListener("click", function (e) {
        var p = e.target.closest ? e.target.closest("[data-panel]") : null;
        if (!p) return;
        e.preventDefault();
        openPanel(p.getAttribute("data-panel"));
    });

    document.getElementById("ui-bar").addEventListener("click", function (e) {
        var b = e.target.closest("[data-cmd]");
        if (!b) return;
        e.preventDefault();
        send({ type: "cmd", cmd: b.getAttribute("data-cmd") });
    });

    document.getElementById("action-list").addEventListener("click", function (e) {
        var a = e.target.closest("[data-action]");
        if (!a) return;
        e.preventDefault();
        send({ type: "action", id: a.getAttribute("data-action") });
    });

    document.getElementById("ui-bar-toggle").addEventListener("click", function () {
        document.getElementById("ui-bar").classList.toggle("stowed");
    });

    document.getElementById("dialog-close").addEventListener("click", closeDialog);
    dialog.addEventListener("click", function (e) { if (e.target === dialog) closeDialog(); });
    document.addEventListener("keydown", function (e) {
        if (e.key === "Escape") { closeDialog(); return; }
        var tag = (e.target && e.target.tagName) || "";
        if ((e.key === "m" || e.key === "M") && tag !== "INPUT" && dialog.hidden) openPanel("map");
    });

    applySettings();
    applyBars();
})();
