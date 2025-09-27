/** Instancia global de audio para controlar reproducir/pausar */
let audioInstance: HTMLAudioElement | null = null;

/**
 * Reproduce un audio en bucle desde la URL indicada.
 * Si ya hay uno sonando, lo reinicia.
 */
export function playAudio(src: string) {
    try {
        if (!audioInstance) {
            audioInstance = new Audio();
            audioInstance.loop = true; // que suene en bucle mientras está abierto
        }

        const resolved = new URL(src, document.baseURI).href;
        if (audioInstance.src !== resolved) {
            audioInstance.src = resolved;
        }

        audioInstance.currentTime = 0;
        // Ignorar error de autoplay si el usuario aún no interactuó
        audioInstance.play().catch(() => { });
    } catch (e) {
        console.warn("playAudio error", e);
    }
}

/** Detiene y reinicia el audio global */
export function stopAudio() {
    try {
        if (audioInstance) {
            audioInstance.pause();
            audioInstance.currentTime = 0;
        }
    } catch (e) {
        console.warn("stopAudio error", e);
    }
}

// ==== Exponer globales para Blazor/JS interop ====
declare global {
    interface Window {
        playAudio: typeof playAudio;
        stopAudio: typeof stopAudio;
    }
}

Object.assign(window, {
    playAudio,
    stopAudio,
});
