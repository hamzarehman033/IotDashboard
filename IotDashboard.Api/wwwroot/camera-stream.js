function iceServersFromForm() {
  const servers = [{ urls: "stun:stun.l.google.com:19302" }];
  const turnUrl = document.getElementById("turnUrl")?.value.trim();
  if (turnUrl) {
    servers.push({
      urls: turnUrl,
      username: document.getElementById("turnUser")?.value.trim() || undefined,
      credential: document.getElementById("turnPass")?.value.trim() || undefined
    });
  }
  return servers;
}

function hubUrl(apiUrl) {
  const origin = (apiUrl || location.origin).replace(/\/$/, "");
  return origin.endsWith("/hubs/camera-stream")
    ? origin
    : `${origin}/hubs/camera-stream`;
}

function readForm() {
  return {
    apiUrl: document.getElementById("apiUrl")?.value.trim() || location.origin,
    deviceId: Number(document.getElementById("deviceId").value),
    cameraIndex: Number(document.getElementById("cameraIndex").value || 0)
  };
}

function log(message) {
  const el = document.getElementById("log");
  if (!el) return;
  el.textContent += message + "\n";
  el.scrollTop = el.scrollHeight;
}

async function connectHub(onSignal) {
  const form = readForm();
  if (!form.apiUrl || !form.deviceId) {
    throw new Error("API URL and device id are required");
  }

  const connection = new signalR.HubConnectionBuilder()
    .withUrl(hubUrl(form.apiUrl))
    .withAutomaticReconnect()
    .build();

  connection.on("CameraSignal", onSignal);
  await connection.start();
  await connection.invoke("StartStream", form.deviceId, form.cameraIndex);
  return { connection, form };
}

function sendSignal(connection, form, sessionId, payload) {
  return connection.invoke("Signal", form.deviceId, form.cameraIndex, sessionId, payload);
}

async function stopHub(connection, form, stream, peer) {
  stream?.getTracks().forEach((track) => track.stop());
  peer?.close();
  if (connection) {
    await connection.invoke("StopStream", form.deviceId);
    await connection.stop();
  }
}
