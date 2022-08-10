export function showPrompt(message) {
    return prompt(message, 'Type anything here');
}


export function addClassName (element, className) {
    element.classList.add(className);
}

export function downloadFile(url, fileName) {
    const link = document.createElement('a');
    link.href = url;
    link.download = fileName ?? '';
    link.click();
    link.remove();
}

export async function getClipboard() {
    return await navigator.clipboard.readText();
}

export function getSelectionStart (element) {
    return element.selectionStart;
}

export function invokeClick(elementId) {
    document.getElementById(elementId).click();
}

export function invokeConfirm (message) {
    return confirm(message);
}

export function invokeAlert (message) {
    alert(message);
}

export function invokePrompt (message) {
    return prompt(message);
}

export function preventTabOut (element) {
    element.addEventListener("keydown", ev => {
        if (ev.key.toLowerCase() == "tab") {
            ev.preventDefault();
        }
    })
}
export function reload () {
    window.location.reload();
}

export function scrollToEnd (element) {
    if (!element) {
        return;
    }

    element.scrollTop(element.scrollHeight);
}

export function scrollToElement(element) {
    window.setTimeout(() => {
        window.scrollTo({ top: element.offsetTop, behavior: "smooth" });
    }, 200);

}

export function setClipboard(content) {
    navigator.clipboard.writeText(content);
}

export function setStyleProperty(element, propertyName, value) {
    element.style[propertyName] = value;
}

export function startDraggingY(element, clientY) {
    if (!element) {
        return;
    }

    var startTop = Number(clientY);

    function pointerMove(ev) {
        if (Math.abs(ev.clientY - startTop) > 10) {
            if (ev.clientY < 0 || ev.clientY > window.innerHeight - element.clientHeight) {
                return;
            }

            element.style.top = `${ev.clientY}px`;
        }
    }

    function pointerUpOrLeave(ev) {
        window.removeEventListener("pointermove", pointerMove);
        window.removeEventListener("pointerup", pointerUpOrLeave);
        window.removeEventListener("pointerleave", pointerUpOrLeave);
    }

    pointerUpOrLeave();

    window.addEventListener("pointermove", pointerMove);
    window.addEventListener("pointerup", pointerUpOrLeave);
    window.addEventListener("pointerleave", pointerUpOrLeave);
}

export function openWindow(url, target) {
    window.open(url, target);
}
