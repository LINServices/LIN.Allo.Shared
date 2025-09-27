export const byId = <T extends HTMLElement = HTMLElement>(id: string) =>
    document.getElementById(id) as T | null;

export async function safePlay(video: HTMLVideoElement) {
    try {
        await video.play();
    } catch {}
}
export function initialsFrom(name?: string) {
    if (!name) return "??";
    const parts = name.trim().split(/\s+/).slice(0, 2);
    return parts.map(p => p.charAt(0).toUpperCase()).join("");
}