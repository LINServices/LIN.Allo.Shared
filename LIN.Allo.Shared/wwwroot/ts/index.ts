export * from "./types";
export * from "./utils/dom";
export * from "./utils/wakeLock";
export * from "./ui/spotlight";
export * from "./rtc/webrtc";
export * from "./extensions/flowbite";
export * from "./extensions/audio";
export * from "./extensions/globals";

import { webrtc } from "./rtc/webrtc";
declare global { interface Window { webrtc?: any; } }
window.webrtc = webrtc;