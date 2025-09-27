import type { Els, PeerId, PeerItem, PeersMeta, SignalClient, IceServersConfig } from "../types";
import { byId, safePlay, initialsFrom } from "../utils/dom";
import { requestWakeLock } from "../utils/wakeLock";
import { spotlightPeer, clearSpotlight, setSpotlightGridId } from "../ui/spotlight";

import { HubConnectionBuilder, HubConnection, LogLevel } from "@microsoft/signalr";

const servers: IceServersConfig = {
    iceServers: [{ urls: "stun:stun.l.google.com:19302" }]
};

const els: Els = { grid: "videoGrid", localVideo: "localVideo" };

let connection: SignalClient | null = null;
let localStream: MediaStream | null = null;
const pcs = new Map<PeerId, PeerItem>();         // targetId -> { pc, remoteStream }
const peersMeta = new Map<PeerId, PeersMeta>();  // targetId -> { name }
let roomId: string | null = null;
let myName = "";

// Estado extra para compartir pantalla
let cameraStream: MediaStream | null = null;
let screenStream: MediaStream | null = null;
let usingScreen = false;

export function setEls(map: Partial<Els>) {
    Object.assign(els, map);
    setSpotlightGridId(map.grid ?? els.grid);
}

export async function init(hubUrl: string, userName: string) {
    myName = userName;

    connection = new HubConnectionBuilder()
        .withUrl(hubUrl)
        .withAutomaticReconnect()
        .configureLogging(LogLevel.Information)
        .build();

    connection.on("PeersInRoom", async (ids: PeerId[]) => {
        for (const id of ids)
            await ensurePeerAndOffer(id);
    });

    connection.on("PeerJoined", async (id: PeerId) => {
        console.log("PeerJoined", id);
    });

    connection.on("Disconnet", async () => {
        await hangup();
    });

    connection.on("PeerLeft", (id: PeerId) => {
        const it = pcs.get(id);
        console.log("PeerLeft", id);
        if (it) {
            try { it.pc.close(); } catch { }
            pcs.delete(id);
            removeVideoEl(id);
        }
    });

    connection.on("Sdp", async (fromId: PeerId, type: RTCSdpType, sdp: string, name?: string) => {
        if (name) peersMeta.set(fromId, { name });
        updateNameTag(fromId);

        const it = await ensurePeer(fromId);
        await it.pc.setRemoteDescription({ type, sdp });

        if (type === "offer") {
            const answer = await it.pc.createAnswer();
            await it.pc.setLocalDescription(answer);
            await connection!.invoke("SendSdp", fromId, "answer", answer.sdp, myName);
        }
    });

    connection.on("Ice", async (fromId: PeerId, candidate?: string, sdpMid?: string, sdpMLineIndex?: number) => {
        const it = pcs.get(fromId);
        if (it && candidate) {
            try {
                await it.pc.addIceCandidate({ candidate, sdpMid, sdpMLineIndex: sdpMLineIndex ?? null as any });
            } catch (e) {
                console.warn(e);
            }
        }
    });

    await connection.start();
    console.log("SignalR connected");
}

export async function join(rid: string, token: string) {
    await requestWakeLock();
    roomId = rid;
    await getMedia();
    console.log("Joining");
    await connection!.invoke("Join", rid, token);
    console.log("Joined");
}

async function getMedia() {
    if (localStream) return localStream;

    cameraStream = await navigator.mediaDevices.getUserMedia({
        audio: { echoCancellation: true, noiseSuppression: true, autoGainControl: true } as any,
        video: { width: { ideal: 1280 }, height: { ideal: 720 } }
    });
    localStream = cameraStream;

    const lv = byId<HTMLVideoElement>(els.localVideo);
    if (lv) { lv.srcObject = localStream; lv.muted = true; lv.playsInline = true; await safePlay(lv); }
    return localStream;
}

function makePc(targetId: PeerId): PeerItem {
    const pc = new RTCPeerConnection(servers);
    const remoteStream = new MediaStream();

    pc.onicecandidate = async (ev) => {
        if (ev.candidate) {
            await connection!.invoke("SendIce", targetId, ev.candidate.candidate, ev.candidate.sdpMid, ev.candidate.sdpMLineIndex);
        }
    };

    pc.ontrack = (ev) => {
        ev.streams[0].getTracks().forEach(t => {
            if (!remoteStream.getTracks().some(x => x.id === t.id)) remoteStream.addTrack(t);
        });
        attachRemote(targetId, remoteStream);
    };

    const stream = localStream!;
    stream.getTracks().forEach(t => pc.addTrack(t, stream));

    return { pc, remoteStream };
}

async function ensurePeer(targetId: PeerId) {
    if (pcs.has(targetId)) return pcs.get(targetId)!;
    const it = makePc(targetId);
    pcs.set(targetId, it);
    createVideoEl(targetId);
    return it;
}

async function ensurePeerAndOffer(targetId: PeerId) {
    const it = await ensurePeer(targetId);
    const offer = await it.pc.createOffer({ offerToReceiveAudio: true, offerToReceiveVideo: true } as any);
    await it.pc.setLocalDescription(offer);
    await connection!.invoke("SendSdp", targetId, "offer", offer.sdp, myName);
}

function createVideoEl(id: PeerId) {
    const grid = byId<HTMLDivElement>(els.grid) ?? byId<HTMLDivElement>("grid-llamada");
    if (!grid) return;

    const wrapper = document.createElement("div");
    wrapper.id = `ct-${id}`;
    wrapper.className = "relative aspect-video bg-zinc-800 rounded-2xl overflow-hidden shadow-lg";

    const v = document.createElement("video");
    v.id = `peer-${id}`;
    v.autoplay = true;
    v.playsInline = true;
    v.setAttribute("data-peer", id);
    v.className = "w-full h-full object-cover";

    const label = document.createElement("div");
    label.id = `label-${id}`;
    label.className = "absolute left-2 bottom-2 max-w-[85%] truncate rounded-md bg-black/60 px-2 py-1 text-xs font-medium text-white";
    label.textContent = peersMeta.get(id)?.name || "Conectando…";

    // avatar iniciales (fallback opcional)
    const avatar = document.createElement("div");
    avatar.id = `avatar-${id}`;
    avatar.className = "absolute left-2 top-2 size-7 rounded-full bg-white/10 text-white text-[10px] grid place-items-center";
    avatar.textContent = initialsFrom(peersMeta.get(id)?.name);

    wrapper.append(v, label, avatar);
    grid.appendChild(wrapper);
    (wrapper as any).onclick = () => spotlightPeer(id);
}

function removeVideoEl(id: PeerId) {
    let v = byId<HTMLElement>(`peer-${id}`);
    v?.parentNode?.removeChild(v);

    v = byId<HTMLElement>(`ct-${id}`);
    v?.parentNode?.removeChild(v);
}

async function attachRemote(id: PeerId, stream: MediaStream) {
    const v = byId<HTMLVideoElement>(`peer-${id}`);
    if (!v) return;
    v.srcObject = stream;
    await safePlay(v);
}

export async function hangup() {
    for (const [, it] of pcs) {
        try {
            it.pc.getSenders().forEach(s => s.track?.stop());
            it.pc.close();
        } catch { }
    }
    pcs.clear();
    if (localStream) { localStream.getTracks().forEach(t => t.stop()); localStream = null; }
    const grid = byId<HTMLDivElement>(els.grid);
    if (grid) grid.innerHTML = "";
    clearSpotlight();
    await connection?.invoke("Leave");
}

async function replaceVideoTrackForAllPeers(track: MediaStreamTrack) {
    for (const [, it] of pcs) {
        const sender = it.pc.getSenders().find(s => s.track && s.track.kind === "video");
        if (sender) await sender.replaceTrack(track);
    }
}

export async function startScreenShare() {
    try {
        if (usingScreen) return true;
        await getMedia();
        screenStream = await navigator.mediaDevices.getDisplayMedia({ video: true, audio: false });
        const screenTrack = screenStream.getVideoTracks()[0];
        screenTrack.addEventListener("ended", () => stopScreenShare());
        await replaceVideoTrackForAllPeers(screenTrack);

        const lv = byId<HTMLVideoElement>(els.localVideo);
        if (lv) { lv.srcObject = screenStream; await safePlay(lv); }

        usingScreen = true;
    } catch (ex) {
        alert(ex);
    }
    return usingScreen;
}

export async function stopScreenShare() {
    if (!usingScreen) return false;
    if (screenStream) { screenStream.getTracks().forEach(t => t.stop()); screenStream = null; }
    await getMedia();
    const camTrack = cameraStream!.getVideoTracks()[0];
    await replaceVideoTrackForAllPeers(camTrack);

    const lv = byId<HTMLVideoElement>(els.localVideo);
    if (lv) { lv.srcObject = cameraStream; await safePlay(lv); }

    usingScreen = false;
    return usingScreen;
}

export function toggleMute() {
    if (!localStream) return true;
    localStream.getAudioTracks().forEach(t => (t.enabled = !t.enabled));
    const muted = !localStream.getAudioTracks().every(t => t.enabled);
    return muted;
}

export function toggleCamera() {
    if (!localStream) return true;
    localStream.getVideoTracks().forEach(t => (t.enabled = !t.enabled));
    const off = !localStream.getVideoTracks().every(t => t.enabled);
    return off;
}

function updateNameTag(id: PeerId) {
    const meta = peersMeta.get(id);
    if (!meta) return;
    const label = byId<HTMLDivElement>(`label-${id}`);
    if (label) label.textContent = `${meta.name}`;
    const av = byId<HTMLDivElement>(`avatar-${id}`);
    if (av) av.textContent = initialsFrom(meta.name);
}

// API pública agrupada.
export const webrtc = {
    init, join, hangup, setEls,
    startScreenShare, stopScreenShare,
    toggleMute, toggleCamera,
    spotlightPeer, clearSpotlight
};