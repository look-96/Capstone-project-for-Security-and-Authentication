// Add password equality validation while leaving required, type, length, and pattern checks to HTML5.
document.addEventListener("input", (event) => {
    if (!(event.target instanceof HTMLInputElement)) return;

    const form = event.target.closest("[data-registration-form]");
    if (!form) return;

    const password = form.querySelector("#password");
    const confirmation = form.querySelector("#confirmPassword");
    if (!password || !confirmation) return;

    confirmation.setCustomValidity(
        confirmation.value && confirmation.value !== password.value
            ? "Passwords must match."
            : ""
    );
});
