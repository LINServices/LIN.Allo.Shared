import { Drawer, Popover, Modal, Dropdown, Collapse } from "flowbite";
import { safeExecution } from "../extensions/globals";

/** Blazor interop helper mínimo */
export interface DotNetHelper {
    invokeMethodAsync<T = any>(method: string, ...args: any[]): Promise<T>;
}

/** Helpers DOM tipados */
function byId<T extends HTMLElement = HTMLElement>(id: string): T {
    const el = document.getElementById(id);
    if (!el) throw new Error(`Elemento con id '${id}' no encontrado`);
    return el as T;
}
function tryById<T extends HTMLElement = HTMLElement>(id?: string | null): T | null {
    if (!id) return null;
    return (document.getElementById(id) as T | null);
}

/**
 * Abre un Drawer con configuración flexible
 * @param id              ID del elemento del Drawer
 * @param dotnetHelper    DotNetHelper para invocar métodos .NET
 * @param placement       "right" | "bottom" | "left" | "top"
 * @param idCloseBtn      IDs de botones que cierran el Drawer
 */
export function showCustomDrawer(
    id: string,
    dotnetHelper?: DotNetHelper | null,
    placement: "right" | "bottom" | "left" | "top" = "right",
    ...idCloseBtn: string[]
) {
    let control: HTMLElement;
    try {
        control = byId(id);
    } catch (e) {
        console.error(e);
        return;
    }

    const options = {
        placement,
        backdropClasses: "bg-zinc-900 bg-opacity-50 fixed inset-0 z-30",
        onHide: () => safeExecution(() => dotnetHelper?.invokeMethodAsync("OnHide")),
        onShow: () => safeExecution(() => dotnetHelper?.invokeMethodAsync("OnShow"))
    };

    const drawer = new Drawer(control, options);
    drawer.show();

    idCloseBtn.forEach((closeBtnId) => {
        const closeButton = tryById<HTMLButtonElement>(closeBtnId);
        if (closeButton) {
            closeButton.addEventListener("click", () => drawer.hide(), { passive: true });
        } else {
            console.warn(`Botón con id '${closeBtnId}' no encontrado.`);
        }
    });
}

export function showDrawer(id: string, dotnetHelper?: DotNetHelper | null, ...idCloseBtn: string[]) {
    showCustomDrawer(id, dotnetHelper, "right", ...idCloseBtn);
}

export function showBottomDrawer(id: string, dotnetHelper?: DotNetHelper | null, ...idCloseBtn: string[]) {
    showCustomDrawer(id, dotnetHelper, "bottom", ...idCloseBtn);
}

/**
 * Muestra un Popover
 * @param id            ID del elemento del Popover (contenido)
 * @param dotnetHelper  DotNetHelper opcional
 * @param btn           ID del trigger
 */
export function showPopover(id: string, dotnetHelper?: DotNetHelper | null, btn?: string) {
    const targetEl = byId<HTMLElement>(id);
    const triggerEl = btn ? tryById<HTMLElement>(btn) : targetEl; // permite que el mismo sea trigger

    if (!triggerEl) {
        console.error(`Trigger para Popover con id '${btn}' no encontrado.`);
        return;
    }

    const options = {
        placement: "bottom" as const,
        triggerType: "hover" as const,
        offset: 10,
        onHide: () => safeExecution(() => dotnetHelper?.invokeMethodAsync("OnHide")),
        onShow: () => safeExecution(() => dotnetHelper?.invokeMethodAsync("OnShow"))
    };

    new Popover(targetEl, triggerEl, options);
}

/**
 * Muestra un Modal
 * @param id            ID del modal (contenedor)
 * @param dotnetHelper  DotNetHelper
 * @param idCloseBtn    IDs de botones que cierran el modal
 */
export function showModal(id: string, dotnetHelper?: DotNetHelper | null, ...idCloseBtn: string[]) {
    const control = byId<HTMLElement>(id);

    const options = {
        placement: "center" as const,
        backdrop: "dynamic" as const,
        backdropClasses: "bg-zinc-900 bg-opacity-50 fixed inset-0 z-40",
        closable: true,
        onHide: () => safeExecution(() => dotnetHelper?.invokeMethodAsync("OnHide")),
        onShow: () => safeExecution(() => dotnetHelper?.invokeMethodAsync("OnShow"))
    };

    const modal = new Modal(control, options);
    modal.show();

    idCloseBtn.forEach((closeBtnId) => {
        const closeButton = tryById<HTMLButtonElement>(closeBtnId);
        if (closeButton) {
            closeButton.addEventListener("click", () => modal.hide(), { passive: true });
        }
    });
}

/**
 * Abre un Dropdown genérico
 * @param id            ID del menú
 * @param idOpen        ID del botón que abre
 * @param dotnetHelper  DotNetHelper
 * @param idCloseBtn    IDs de botones que cierran
 */
export function openDropDown(
    id: string,
    idOpen: string,
    dotnetHelper?: DotNetHelper | null,
    ...idCloseBtn: string[]
) {
    const targetEl = byId<HTMLElement>(id);
    const triggerEl = byId<HTMLElement>(idOpen);

    const options = {
        placement: "bottom" as const,
        triggerType: "click" as const,
        offsetSkidding: 0,
        offsetDistance: 10,
        delay: 100,
        onHide: () => safeExecution(() => dotnetHelper?.invokeMethodAsync("OnHide")),
        onShow: () => safeExecution(() => dotnetHelper?.invokeMethodAsync("OnShow"))
    };

    const dropdown = new Dropdown(targetEl, triggerEl, options);

    idCloseBtn.forEach((closeBtnId) => {
        const closeButton = tryById<HTMLButtonElement>(closeBtnId);
        if (closeButton) {
            closeButton.addEventListener("click", () => dropdown.hide(), { passive: true });
        }
    });

    dropdown.toggle();
}

/** Abre un Dropdown específico para el usuario */
export function showUserDropdown(dotnetHelper?: DotNetHelper | null) {
    const targetEl = byId<HTMLElement>("user-dropdown");
    const triggerEl = byId<HTMLElement>("user-menu-button");

    const options = {
        placement: "bottom" as const,
        triggerType: "click" as const,
        offsetSkidding: 0,
        offsetDistance: 10,
        delay: 100,
        onHide: () => safeExecution(() => dotnetHelper?.invokeMethodAsync("OnHide")),
        onShow: () => safeExecution(() => dotnetHelper?.invokeMethodAsync("OnShow")),
        onToggle: () => safeExecution(() => dotnetHelper?.invokeMethodAsync("OnToggle")),
    };

    const dropdown = new Dropdown(targetEl, triggerEl, options);
    dropdown.toggle();
}

/** Abre o cierra el menú móvil (Collapse) */
export function toggleMobileMenu() {
    const targetEl = byId<HTMLElement>("mobile-menu-2");
    const collapse = new Collapse(targetEl, targetEl);
    collapse.toggle();
}

// ==== “Sueltes” en window (globales) ====
declare global {
    interface Window {
        showCustomDrawer: typeof showCustomDrawer;
        showDrawer: typeof showDrawer;
        showBottomDrawer: typeof showBottomDrawer;
        showPopover: typeof showPopover;
        showModal: typeof showModal;
        openDropDown: typeof openDropDown;
        showUserDropdown: typeof showUserDropdown;
        toggleMobileMenu: typeof toggleMobileMenu;
    }
}
Object.assign(window, {
    showCustomDrawer,
    showDrawer,
    showBottomDrawer,
    showPopover,
    showModal,
    openDropDown,
    showUserDropdown,
    toggleMobileMenu,
});