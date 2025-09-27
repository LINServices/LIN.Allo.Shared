import { byId } from "../utils/dom";

type SpotlightState = {
    on: boolean;
    selectedId: string | null;
    overlay: HTMLDivElement | null;
    main: HTMLDivElement | null;
    rail: HTMLDivElement | null;
    gridId: string;
};

const state: SpotlightState = {
    on: false, selectedId: null, overlay: null, main: null, rail: null, gridId: "grid-llamada"
};

export function setSpotlightGridId(gridId: string) { state.gridId = gridId; }

export function buildSpotlightOverlay() {
    const overlay = document.createElement("div");
    overlay.id = "spotlight-overlay";
    overlay.className = "fixed inset-0 z-[70] bg-black/95";
    (overlay.style as any).height = "100dvh";

    const container = document.createElement("div");
    container.id = "spotlight-container";
    container.className = "relative w-full h-full";

    const main = document.createElement("div");
    main.id = "spotlight-main";

    const rail = document.createElement("div");
    rail.id = "spotlight-rail";

    const close = document.createElement("button");
    close.className = "absolute top-3 right-3 z-20 grid h-10 w-10 place-items-center rounded-full bg-zinc-800/70 hover:bg-zinc-700/80 border border-white/10";
    close.innerHTML = '<svg class="h-5 w-5 fill-white" viewBox="0 0 24 24"><path d="M18.3 5.71a1 1 0 0 0-1.41 0L12 10.59 7.11 5.7A1 1 0 1 0 5.7 7.11L10.59 12l-4.9 4.89a1 1 0 1 0 1.41 1.42L12 13.41l4.89 4.9a1 1 0 0 0 1.42-1.41L13.41 12l4.9-4.89Z"/></svg>';
    close.addEventListener("click", clearSpotlight);

    container.append(main, rail, close);
    overlay.appendChild(container);
    document.body.appendChild(overlay);

    state.overlay = overlay; state.main = main; state.rail = rail;

    applyResponsiveSpotlightLayout();
    window.addEventListener("resize", applyResponsiveSpotlightLayout, { passive: true });
    overlay.addEventListener("keydown", (e) => { if ((e as KeyboardEvent).key === "Escape") clearSpotlight(); });
    (overlay as any).tabIndex = -1; (overlay as any).focus();
}

export function applyResponsiveSpotlightLayout() {
    if (!state.overlay || !state.main || !state.rail) return;
    const isMobile = window.matchMedia("(max-width: 639px)").matches;

    state.main.className = isMobile
        ? "absolute inset-x-0 top-0 bottom-[120px] rounded-none sm:rounded-2xl overflow-hidden"
        : "absolute inset-y-0 left-0 right-[260px] rounded-2xl overflow-hidden border border-white/10";

    state.rail.className = isMobile
        ? "absolute left-0 right-0 bottom-0 h-[120px] flex gap-2 overflow-x-auto p-2 bg-black/30 backdrop-blur-sm"
        : "absolute top-0 right-0 w-[260px] h-full flex flex-col gap-3 overflow-y-auto p-2";

    const closeBtn = state.overlay.querySelector("button");
    if (closeBtn) {
        closeBtn.className = isMobile
            ? "absolute top-3 right-3 z-20 grid h-10 w-10 place-items-center rounded-full bg-zinc-800/70 hover:bg-zinc-700/80 border border-white/10"
            : "absolute top-3 right-[280px] z-20 grid h-10 w-10 place-items-center rounded-full bg-zinc-800/70 hover:bg-zinc-700/80 border border-white/10";
    }
}

function moveTileTo(targetWrapper: HTMLElement, container: HTMLElement, thumb = false) {
    targetWrapper.className = "rounded-xl overflow-hidden bg-black border border-white/10";
    const vid = targetWrapper.querySelector("video") as HTMLVideoElement | null;
    if (vid) vid.className = "w-full h-full object-cover";

    if (thumb) {
        targetWrapper.style.height = "120px";
        targetWrapper.style.minHeight = "120px";
        (targetWrapper.style as any).cursor = "pointer";
        targetWrapper.classList.add("opacity-90", "hover:opacity-100");
    } else {
        targetWrapper.style.height = "100%";
        targetWrapper.style.minHeight = "";
        (targetWrapper.style as any).cursor = "default";
        targetWrapper.classList.add("ring-2", "ring-offset-2", "ring-offset-black");
    }
    container.appendChild(targetWrapper);
}

export function spotlightPeer(id: string) {
    const grid = byId<HTMLDivElement>(state.gridId);
    const sel = byId<HTMLDivElement>(`ct-${id}`);
    if (!grid || !sel) return;

    if (!state.on) buildSpotlightOverlay();
    state.on = true; state.selectedId = id;

    moveTileTo(sel, state.main!, false);

    Array.from(grid.children).forEach(ch => {
        const el = ch as HTMLElement;
        if (el.id?.startsWith("ct-")) moveTileTo(el, state.rail!, true);
    });

    Array.from(state.rail!.children).forEach(ch => {
        (ch as HTMLElement).onclick = () => spotlightPeer(ch.id.replace("ct-", ""));
    });
}

export function clearSpotlight() {
    if (!state.on) return;
    const grid = byId<HTMLDivElement>(state.gridId);
    if (!grid || !state.overlay || !state.main || !state.rail) return;

    const all = [
        ...Array.from(state.main.children),
        ...Array.from(state.rail.children),
    ] as HTMLElement[];

    all.forEach(w => {
        w.className = "relative aspect-video bg-gray-800 rounded-2xl overflow-hidden shadow-lg";
        w.style.height = ""; w.style.minHeight = ""; (w.style as any).cursor = "";
        const vid = w.querySelector("video") as HTMLVideoElement | null;
        if (vid) vid.className = "w-full h-full object-cover";
        grid.appendChild(w);
        (w as any).onclick = () => spotlightPeer(w.id.replace("ct-", ""));
    });

    state.overlay.remove();
    state.on = false; state.selectedId = null; state.overlay = state.main = state.rail = null;
}
