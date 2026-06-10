(function(){
  "use strict";

  /* Loaded globally from App.razor, but only the landing page (Login.razor)
     renders the .landing wrapper — bail out everywhere else. */
  if (!document.querySelector(".landing") || !document.getElementById("nav")) return;

  var reduceMotion = window.matchMedia("(prefers-reduced-motion: reduce)").matches;

  /* ---------- nav scrolled state ---------- */
  var nav = document.getElementById("nav");
  function onScroll(){
    nav.classList.toggle("scrolled", window.scrollY > 24);
  }
  window.addEventListener("scroll", onScroll, { passive: true });
  onScroll();

  /* ---------- reveal on view + bar fills ---------- */
  var revealEls = document.querySelectorAll(".js-reveal");
  if ("IntersectionObserver" in window) {
    var io = new IntersectionObserver(function(entries){
      entries.forEach(function(entry){
        if (!entry.isIntersecting) return;
        var el = entry.target;
        el.classList.add("in");
        el.querySelectorAll(".js-bar[data-w]").forEach(function(bar){
          bar.style.width = bar.dataset.w + "%";
        });
        io.unobserve(el);
      });
    }, { rootMargin: "0px 0px -8% 0px", threshold: 0.08 });
    revealEls.forEach(function(el){ io.observe(el); });
  } else {
    revealEls.forEach(function(el){
      el.classList.add("in");
      el.querySelectorAll(".js-bar[data-w]").forEach(function(bar){
        bar.style.width = bar.dataset.w + "%";
      });
    });
  }

  /* ---------- hero card stack tilt ---------- */
  var fineHover = window.matchMedia("(hover: hover) and (pointer: fine)").matches;
  var stack = document.getElementById("stack");
  if (stack && fineHover && !reduceMotion) {
    var hero = stack.closest(".hero-visual");
    hero.addEventListener("pointermove", function(e){
      var r = hero.getBoundingClientRect();
      var nx = ((e.clientX - r.left) / r.width) * 2 - 1;
      var ny = ((e.clientY - r.top) / r.height) * 2 - 1;
      stack.style.transform =
        "perspective(1100px) rotateX(" + (ny * -2.4).toFixed(2) + "deg)" +
        " rotateY(" + (nx * 3.2).toFixed(2) + "deg)";
    });
    hero.addEventListener("pointerleave", function(){
      stack.style.transform = "perspective(1100px) rotateX(0deg) rotateY(0deg)";
    });
  }

  /* ---------- the engine (ported from WSIST.Engine/PriorityCalculator.cs) ---------- */

  var VOLUME_PTS        = [2, 4, 6, 8, 10, 12];
  var UNDERSTANDING_PTS = [12, 10, 8, 6, 4, 2];
  var LEVEL_LABELS = ["Very low", "Low", "Medium", "Average", "High", "Very high"];

  function urgencyScore(days){
    if (days <= 1)  return 10;
    if (days <= 2)  return 8;
    if (days <= 7)  return 6;
    if (days <= 14) return 4;
    if (days <= 30) return 2;
    return 0;
  }

  /* NOTE: the C# engine currently returns 0 for an average below 3
     (the `_ => 0` branch). That contradicts the product's own rule —
     "struggling subjects get a push" — so this demo uses the intended
     mapping: anything under 4 earns the full +6. One-line fix in
     PriorityCalculator.CalculateGradeScore: change `_ => 0` to `_ => 6`. */
  function gradeScore(avg){
    if (avg >= 5) return 2;
    if (avg >= 4) return 4;
    return 6;
  }

  /* ---------- playground wiring ---------- */

  var inDays = document.getElementById("in-days");
  var inVol  = document.getElementById("in-vol");
  var inUnd  = document.getElementById("in-und");
  var inAvg  = document.getElementById("in-avg");

  var outDays = document.getElementById("out-days");
  var outVol  = document.getElementById("out-vol");
  var outUnd  = document.getElementById("out-und");
  var outAvg  = document.getElementById("out-avg");

  var scoreNum = document.getElementById("score-num");
  var verdict  = document.getElementById("verdict");
  var segU = document.getElementById("seg-u");
  var segV = document.getElementById("seg-v");
  var segN = document.getElementById("seg-n");
  var segG = document.getElementById("seg-g");
  var chipU = document.getElementById("chip-u");
  var chipV = document.getElementById("chip-v");
  var chipN = document.getElementById("chip-n");
  var chipG = document.getElementById("chip-g");

  function trackFill(input){
    var min = parseFloat(input.min), max = parseFloat(input.max);
    var p = ((parseFloat(input.value) - min) / (max - min)) * 100;
    input.style.setProperty("--p", p + "%");
  }

  function dayLabel(d){
    if (d === 0) return "today";
    if (d === 1) return "tomorrow";
    return "in " + d + " days";
  }

  function verdictFor(total){
    if (total >= 30) return "Drop everything — this is tonight.";
    if (total >= 22) return "High on the list. Start here.";
    if (total >= 12) return "On the radar — schedule it this week.";
    return "Breathe. You've got time.";
  }

  var lastVerdict = "";
  var lastScore = -1;

  function render(){
    var days = parseInt(inDays.value, 10);
    var vol  = parseInt(inVol.value, 10);
    var und  = parseInt(inUnd.value, 10);
    var avg  = parseFloat(inAvg.value);

    var u = urgencyScore(days);
    var v = VOLUME_PTS[vol];
    var n = UNDERSTANDING_PTS[und];
    var g = gradeScore(avg);
    var total = u + v + n + g;

    outDays.textContent = dayLabel(days);
    outVol.textContent  = LEVEL_LABELS[vol];
    outUnd.textContent  = LEVEL_LABELS[und];
    outAvg.textContent  = "Ø " + avg.toFixed(1);

    [inDays, inVol, inUnd, inAvg].forEach(trackFill);

    scoreNum.textContent = total;
    if (total !== lastScore && lastScore !== -1 && !reduceMotion) {
      scoreNum.classList.remove("pulse");
      void scoreNum.offsetWidth; /* restart animation */
      scoreNum.classList.add("pulse");
    }
    lastScore = total;

    segU.style.width = (u / 40 * 100) + "%";
    segV.style.width = (v / 40 * 100) + "%";
    segN.style.width = (n / 40 * 100) + "%";
    segG.style.width = (g / 40 * 100) + "%";

    chipU.textContent = "+" + u;
    chipV.textContent = "+" + v;
    chipN.textContent = "+" + n;
    chipG.textContent = "+" + g;

    var vText = verdictFor(total);
    if (vText !== lastVerdict) {
      lastVerdict = vText;
      if (reduceMotion) {
        verdict.textContent = vText;
      } else {
        verdict.classList.add("swap");
        setTimeout(function(){
          verdict.textContent = vText;
          verdict.classList.remove("swap");
        }, 160);
      }
    }
  }

  [inDays, inVol, inUnd, inAvg].forEach(function(input){
    input.addEventListener("input", render);
  });

  render();
})();
