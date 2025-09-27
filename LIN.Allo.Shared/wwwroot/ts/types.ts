export type PeerId = string;

export interface PeerItem {
    pc: RTCPeerConnection;
    remoteStream: MediaStream;
}

export interface Els {
    grid: string;       // id del contenedor (grid-llamada)
    localVideo: string; // id del video local
}

export interface PeersMeta {
    name?: string;
}

export interface IceServersConfig {
    iceServers: RTCIceServer[];
}

export interface SignalClient {
    start(): Promise<void>;
    invoke(method: string, ...args: any[]): Promise<any>;
    on(method: string, cb: (...args: any[]) => void): void;
}