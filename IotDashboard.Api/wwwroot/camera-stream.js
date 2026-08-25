function $(id) {
  return document.getElementById(id);
}

function readForm() {
  const api = ($("apiUrl")?.value.trim() || location.origin)
    .replace(/\/$/, "")
    .replace(/\/hubs\/camera-stream$/, "");
  return {
    api,
    deviceId: Number($("deviceId").value),
    cameraIndex: Number($("cameraIndex").value || 0)
  };
}

function iceServers() {
  const servers = [{ urls: "stun:stun.l.google.com:19302" }];
  const url = $("turnUrl")?.value.trim();
  if (url) {
    servers.push({
      urls: url,
      username: $("turnUser")?.value.trim(),
      credential: $("turnPass")?.value.trim()
    });
  }
  return servers;
}

function log(message) {
  const el = $("log");
  if (!el) return;
  el.textContent += message + "\n";
  el.scrollTop = el.scrollHeight;
}

async function connect(role, events, existing) {
  const f = readForm();
  if (!f.deviceId) throw new Error("Device id is required");

  if (existing) {
    await existing.invoke("StartViewer", f.deviceId, f.cameraIndex);
    return { connection: existing, form: f };
  }

  const connection = new signalR.HubConnectionBuilder()
    .withUrl(`${f.api}/hubs/camera-stream`)
    .withAutomaticReconnect()
    .build();

  for (const name in events) connection.on(name, events[name]);

  const join = () => connection.invoke(
    role === "publisher" ? "StartPublisher" : "StartViewer",
    f.deviceId,
    f.cameraIndex
  );

  await connection.start();
  connection.onreconnected(join);
  await join();
  return { connection, form: f };
}

function signal(connection, f, viewerId, payload) {
  return connection.invoke("Signal", f.deviceId, f.cameraIndex, viewerId, payload);
}

async function stop(connection, f, close, disconnect) {
  close?.();
  try { await connection?.invoke("StopStream", f.deviceId); } catch { }
  if (disconnect) await connection?.stop();
}
