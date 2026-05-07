(() => {
  const canvas = document.getElementById("game");
  const ctx = canvas.getContext("2d");
  const resultEl = document.getElementById("result");
  const scoreEl = document.getElementById("score");

  const state = {
    hits: 0,
    outs: 0,
    strikes: 0,
    message: "Spaceでスイング",
    ball: null,
    swingTimer: 999,
    swingDuration: 0.22,
    nextPitchTimer: 0.9,
  };

  const world = {
    cameraZ: -5.5,
    home: { x: 0, y: 0, z: 0 },
    pitcher: { x: 0, y: 1.7, z: 18 },
    gravity: -17,
  };

  function rand(min, max) {
    return min + (max - min) * Math.random();
  }

  function resetBall() {
    const targetX = rand(-0.6, 0.6);
    const targetY = rand(0.8, 1.6);
    const targetZ = 0.45;
    const dx = targetX - world.pitcher.x;
    const dy = targetY - world.pitcher.y;
    const dz = targetZ - world.pitcher.z;
    const len = Math.hypot(dx, dy, dz);
    const speed = 19.5;

    state.ball = {
      x: world.pitcher.x,
      y: world.pitcher.y,
      z: world.pitcher.z,
      vx: (dx / len) * speed,
      vy: (dy / len) * speed,
      vz: (dz / len) * speed,
      hit: false,
      judged: false,
    };
    state.message = "来球中...";
  }

  function scheduleNextPitch() {
    state.nextPitchTimer = 1.0;
  }

  function updateScoreText() {
    resultEl.textContent = state.message;
    scoreEl.textContent = `HIT: ${state.hits} / OUT: ${state.outs} / STRIKE: ${state.strikes}`;
  }

  function project(p) {
    const depth = p.z - world.cameraZ;
    if (depth <= 0.1) {
      return null;
    }
    const f = 560 / depth;
    return {
      x: canvas.width * 0.5 + p.x * f,
      y: canvas.height * 0.72 - p.y * f,
      s: f,
    };
  }

  function drawField() {
    const w = canvas.width;
    const h = canvas.height;
    ctx.clearRect(0, 0, w, h);

    const sky = ctx.createLinearGradient(0, 0, 0, h);
    sky.addColorStop(0, "#8ec3ff");
    sky.addColorStop(1, "#4f90e8");
    ctx.fillStyle = sky;
    ctx.fillRect(0, 0, w, h);

    ctx.fillStyle = "#3e9f49";
    ctx.fillRect(0, h * 0.58, w, h * 0.42);

    const home = project(world.home);
    if (home) {
      ctx.fillStyle = "#f6f6f6";
      const size = 12;
      ctx.beginPath();
      ctx.moveTo(home.x, home.y - size * 0.45);
      ctx.lineTo(home.x + size * 0.45, home.y);
      ctx.lineTo(home.x, home.y + size * 0.45);
      ctx.lineTo(home.x - size * 0.45, home.y);
      ctx.closePath();
      ctx.fill();
    }

    const plateCenter = project({ x: 0, y: 1.2, z: 0.45 });
    if (plateCenter) {
      const sw = plateCenter.s * 1.35;
      const sh = plateCenter.s * 1.75;
      ctx.strokeStyle = "rgba(255,255,255,0.55)";
      ctx.lineWidth = 2;
      ctx.strokeRect(plateCenter.x - sw / 2, plateCenter.y - sh / 2, sw, sh);
    }
  }

  function drawBatter() {
    const body = project({ x: 0, y: 1.0, z: -0.2 });
    if (!body) {
      return;
    }

    ctx.fillStyle = "#24364f";
    ctx.beginPath();
    ctx.arc(body.x, body.y, body.s * 0.37, 0, Math.PI * 2);
    ctx.fill();

    const swingT = Math.min(state.swingTimer / state.swingDuration, 1);
    const active = swingT >= 0 && swingT <= 1;
    const start = -1.2;
    const end = 0.6;
    const angle = active ? start + (end - start) * swingT : start;
    const batLen = body.s * 1.7;

    const shoulderX = body.x + body.s * 0.2;
    const shoulderY = body.y - body.s * 0.1;
    const batEndX = shoulderX + Math.cos(angle) * batLen;
    const batEndY = shoulderY + Math.sin(angle) * batLen;

    ctx.strokeStyle = "#8e6a3d";
    ctx.lineWidth = Math.max(3, body.s * 0.16);
    ctx.lineCap = "round";
    ctx.beginPath();
    ctx.moveTo(shoulderX, shoulderY);
    ctx.lineTo(batEndX, batEndY);
    ctx.stroke();
  }

  function drawBall() {
    if (!state.ball) {
      return;
    }
    const p = project(state.ball);
    if (!p) {
      return;
    }
    const radius = Math.max(3, p.s * 0.18);
    ctx.fillStyle = "#ffffff";
    ctx.beginPath();
    ctx.arc(p.x, p.y, radius, 0, Math.PI * 2);
    ctx.fill();

    ctx.strokeStyle = "#d33";
    ctx.lineWidth = 1.4;
    ctx.beginPath();
    ctx.arc(p.x, p.y, radius * 0.7, -0.8, 0.9);
    ctx.stroke();
  }

  function inHitWindow() {
    const t = state.swingTimer / state.swingDuration;
    return t >= 0.28 && t <= 0.75;
  }

  function swingPower01() {
    const t = state.swingTimer / state.swingDuration;
    const center = 0.52;
    const width = 0.24;
    return 1 - Math.min(1, Math.abs(t - center) / width);
  }

  function tryHit() {
    const b = state.ball;
    if (!b || b.hit || !inHitWindow()) {
      return;
    }
    const inZone = Math.abs(b.x) <= 0.85 && b.y >= 0.55 && b.y <= 1.9 && Math.abs(b.z) <= 0.95;
    if (!inZone) {
      return;
    }

    const power = swingPower01();
    const speed = 13 + 16 * power;
    const side = rand(-0.3, 0.3);
    const up = 0.32 + 0.3 * power;
    const dz = 1;
    const len = Math.hypot(side, up, dz);
    b.vx = (side / len) * speed;
    b.vy = (up / len) * speed;
    b.vz = (dz / len) * speed;
    b.hit = true;
    b.judged = false;
    state.message = "打球！";
    updateScoreText();
  }

  function judgeHitOrOut() {
    const b = state.ball;
    if (!b || !b.hit || b.judged) {
      return;
    }
    if (b.y > 0.2) {
      return;
    }
    const distance = Math.hypot(b.x - world.home.x, b.z - world.home.z);
    b.judged = true;
    if (distance >= 14.5) {
      state.hits += 1;
      state.message = `ヒット！ (${distance.toFixed(1)}m)`;
    } else {
      state.outs += 1;
      state.message = `アウト (${distance.toFixed(1)}m)`;
    }
    updateScoreText();
    scheduleNextPitch();
  }

  function update(dt) {
    state.swingTimer += dt;
    if (state.nextPitchTimer > 0) {
      state.nextPitchTimer -= dt;
      if (state.nextPitchTimer <= 0) {
        resetBall();
        updateScoreText();
      }
    }

    const b = state.ball;
    if (!b) {
      return;
    }

    b.vy += world.gravity * dt;
    b.x += b.vx * dt;
    b.y += b.vy * dt;
    b.z += b.vz * dt;

    if (b.y < 0.18) {
      b.y = 0.18;
      b.vx *= 0.9;
      b.vz *= 0.9;
      b.vy *= -0.28;
    }

    tryHit();

    if (!b.hit && b.z < -0.85) {
      state.strikes += 1;
      state.message = "ストライク！";
      state.ball = null;
      updateScoreText();
      scheduleNextPitch();
      return;
    }

    judgeHitOrOut();

    if (b.hit && Math.hypot(b.vx, b.vy, b.vz) < 1.1 && b.y <= 0.2 && !b.judged) {
      judgeHitOrOut();
    }
  }

  let last = performance.now();
  function loop(now) {
    const dt = Math.min(0.033, (now - last) / 1000);
    last = now;
    update(dt);
    drawField();
    drawBatter();
    drawBall();
    requestAnimationFrame(loop);
  }

  window.addEventListener("keydown", (e) => {
    if (e.code === "Space") {
      e.preventDefault();
      if (state.swingTimer > state.swingDuration + 0.11) {
        state.swingTimer = 0;
      }
    } else if (e.key.toLowerCase() === "r") {
      state.hits = 0;
      state.outs = 0;
      state.strikes = 0;
      state.message = "リセットしました";
      state.ball = null;
      scheduleNextPitch();
      updateScoreText();
    }
  });

  updateScoreText();
  scheduleNextPitch();
  requestAnimationFrame(loop);
})();
