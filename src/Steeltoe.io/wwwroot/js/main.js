// modified copy of https://github.com/dotnet/docfx/blob/main/templates/modern/src/docfx.ts

function setTheme(theme) {
    localStorage.setItem("theme", theme)
    if (theme === "auto") {
        document.documentElement.setAttribute("data-bs-theme", window.matchMedia("(prefers-color-scheme: dark)").matches ? "dark" : "light")
        document.getElementById("theme").className = "bi bi-circle-half";
    } else {
        document.documentElement.setAttribute("data-bs-theme", theme)
        if (theme === "light"){
          document.getElementById("theme").className = "bi bi-sun";
        }
        else {
          document.getElementById("theme").className ="bi bi-moon";
        }
    }
}

async function getDefaultTheme() {
    return localStorage.getItem("theme") || "auto"
}

async function initTheme() {
    setTheme(await getDefaultTheme())
}

function onThemeChange(callback) {
    return new MutationObserver(() => callback(getTheme()))
        .observe(document.documentElement, { attributes: true, attributeFilter: ["data-bs-theme"] })
}

function getTheme() {
    return document.documentElement.getAttribute("data-bs-theme")
}

function changeTheme(e, theme) {
    e.preventDefault()
    setTheme(theme)
}
