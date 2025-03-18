// sourced from https://github.com/dotnet/docfx/blob/main/templates/modern/src/docfx.ts


function setTheme(theme) {
    localStorage.setItem("theme", theme)
    if (theme === "auto") {
        document.documentElement.setAttribute("data-bs-theme", window.matchMedia("(prefers-color-scheme: dark)").matches ? "dark" : "light")
        $("#theme").removeClass("bi-sun bi-moon").addClass("bi-circle-half")
    } else {
        document.documentElement.setAttribute("data-bs-theme", theme)
        if (theme == "light"){
            $("#theme").removeClass("bi-moon bi-circle-half").addClass("bi-sun")
        }
        else {
            $("#theme").removeClass("bi-sun bi-circle-half").addClass("bi-moon")
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
