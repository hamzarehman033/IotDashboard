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

async function connect(role, events, existing, ready) {
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

  try {
    await connection.start();
    ready?.(connection, f);
    connection.onreconnected(() => join().catch((error) => log(error.message || String(error))));
    await join();
    return { connection, form: f };
  } catch (error) {
    await connection.stop();
    throw error;
  }
}

function signal(connection, f, viewerId, payload) {
  return connection.invoke("Signal", f.deviceId, f.cameraIndex, viewerId, payload);
}

async function stop(connection, f, close, disconnect) {
  close?.();
  try {
    if (connection && f) await connection.invoke("StopStream", f.deviceId);
  } finally {
    if (disconnect) await connection?.stop();
  }
}

async function attachMicrophone(peer, track) {
  const audio = peer.getTransceivers().find((transceiver) =>
    transceiver.receiver.track.kind === "audio");
  if (!audio) throw new Error("Audio channel is not ready. Wait for the stream, then try again.");
  await audio.sender.replaceTrack(track);
}

function setMicrophone(stream) {
  const tracks = stream?.getAudioTracks() || [];
  $("muteMic").disabled = tracks.length === 0;
  $("muteMic").textContent = "Mute microphone";
}

function toggleMicrophone(stream) {
  const tracks = stream?.getAudioTracks() || [];
  const enabled = !tracks[0]?.enabled;
  tracks.forEach((track) => { track.enabled = enabled; });
  $("muteMic").textContent = enabled ? "Mute microphone" : "Unmute microphone";
}

async function playMedia(element) {
  try {
    await element.play();
  } catch (error) {
    log("Playback blocked: " + error.message + ". Use the media play control to hear remote audio.");
  }
}

const signalQueues = new WeakMap();

function receiveSignal(peer, payload, answer) {
  let state = signalQueues.get(peer);
  if (!state) {
    state = { pendingIce: [], queue: Promise.resolve() };
    signalQueues.set(peer, state);
  }
  state.queue = state.queue.then(async () => {
    if (peer.signalingState === "closed") return;
    if (payload.type === "ice" && payload.candidate) {
      if (peer.remoteDescription) await peer.addIceCandidate(payload.candidate);
      else state.pendingIce.push(payload.candidate);
    } else if (payload.type === "offer" || payload.type === "answer") {
      await peer.setRemoteDescription({ type: payload.type, sdp: payload.sdp });
      for (const candidate of state.pendingIce.splice(0)) await peer.addIceCandidate(candidate);
      if (payload.type === "offer") await answer();
    }
  }).catch((error) => log("Signaling error: " + error.message));
  return state.queue;
}
