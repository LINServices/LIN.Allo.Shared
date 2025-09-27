/** Hace scroll suave al final de un elemento */
export function scrollToBottom(elementId: string) {
    const element = document.getElementById(elementId);
    if (element) {
        const h = element.scrollHeight;
        element.scroll({
            top: h + 500,
            left: 0,
            behavior: "smooth",
        });
    }
}

/** Simula un click en un control por ID */
export function forceClick(id: string) {
    const control = document.getElementById(id) as HTMLElement | null;
    control?.click();
}

/** Abre una URL en una nueva pestaña de forma segura */
export function GoLaunch(url: string) {
    window.open(url, "_blank", "noopener,noreferrer");
}

/** Devuelve un código numérico según el sistema operativo detectado */
export function getOperativeSystem(): number {
    const ua = navigator.userAgent.toLowerCase();

    if (/iphone|ipad|ipod/.test(ua)) return 5; // iOS
    if (/windows/.test(ua)) return 1;
    if (/macintosh|mac os x/.test(ua)) return 3;
    if (/android/.test(ua)) return 2;
    if (/linux/.test(ua)) return 4;

    return 0; // desconocido
}

/** Devuelve un código numérico según el navegador detectado */
export function getBrowserName(): number {
    const ua = navigator.userAgent.toLowerCase();

    if (/edg\//.test(ua)) return 2; // Edge
    if (/chrome/.test(ua) && !/edg\//.test(ua) && !/opr\//.test(ua)) return 1;
    if (/firefox/.test(ua)) return 3;
    if (/safari/.test(ua) && !/chrome/.test(ua)) return 4;

    return 0; // desconocido
}

/**
 * Ejecuta de forma segura un callback.
 * Si arroja error, devuelve null.
 *
 * @param callback Función a ejecutar
 * @returns El resultado del callback o null si ocurre error
 */
export function safeExecution<T>(callback: () => T): T | null {
    try {
        return callback();
    } catch (e) {
        console.log("Falling")
        return null;
    }
}


// ==== Exponer globales para Blazor/JS interop ====
declare global {
    interface Window {
        scrollToBottom: typeof scrollToBottom;
        forceClick: typeof forceClick;
        GoLaunch: typeof GoLaunch;
        getOperativeSystem: typeof getOperativeSystem;
        getBrowserName: typeof getBrowserName;
    }
}

Object.assign(window, {
    scrollToBottom,
    forceClick,
    GoLaunch,
    getOperativeSystem,
    getBrowserName,
});