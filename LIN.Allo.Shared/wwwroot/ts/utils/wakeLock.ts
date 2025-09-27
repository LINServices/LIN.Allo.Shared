let wakeLock: any = null;

export async function requestWakeLock() {
    try {
        if ("wakeLock" in navigator) {
            wakeLock = await (navigator as any).wakeLock.request("screen");
            wakeLock.addEventListener?.("release", () => console.log("Wake Lock released"));
            console.log("Wake Lock active");
        }
    } catch (err: any) {
        console.error(`${err?.name}, ${err?.message}`);
    }
}

export async function releaseWakeLock() {
    if (wakeLock) {
        await wakeLock.release?.();
        wakeLock = null;
    }
}