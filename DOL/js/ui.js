(function () {
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

    window.setState = function (s) {
        function setStat(id, value, note, bar) {
            if (s.portrait) {
                var img = document.getElementById("portrait");
                if (img) img.src = s.portrait;
                if (s.portrait) {
                    var img = document.getElementById("portrait");
                    if (img) img.src = s.portrait;
                }
            }
            var el = document.getElementById(id);

            if (!el) return;
            var valueEl = el.querySelector('[data-field="value"]');
            var noteEl = el.querySelector('[data-field="note"]');
            var barEl = el.querySelector(".stat-bar > i");
            if (value !== undefined && valueEl) valueEl.textContent = value;
            if (note !== undefined && noteEl) noteEl.textContent = note;
            if (bar !== undefined && barEl) barEl.setAttribute("data-value", bar);
        }

        if (s.playerName) {
            document.getElementById("player-name").textContent = s.playerName;
        }
        if (s.stats) {
            Object.keys(s.stats).forEach(function (k) {
                var v = s.stats[k];
                setStat("stat-" + k, v.value, v.note, v.bar);
            });
        }
        if (s.passage) {
            document.getElementById("passage-text").innerHTML = s.passage;
        }
        if (s.actions) {
            var ul = document.getElementById("action-list");
            ul.innerHTML = "";
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
        }
        applyBars();
    };

    function send(msg) {
        if (window.chrome && window.chrome.webview) {
            window.chrome.webview.postMessage(msg);
        }
    }

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

    applyBars();
})();