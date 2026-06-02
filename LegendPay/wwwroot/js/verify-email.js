
document.addEventListener("DOMContentLoaded", function () {
    const inputs = document.querySelectorAll(".otp-input");
    const hiddenOtpInput = document.getElementById("hiddenOtpCode");
    const form = document.getElementById("otpForm");

    // Auto-focus management and shifting selection right/left
    inputs.forEach((input, index) => {
        input.addEventListener("input", (e) => {
            const value = e.target.value;
            // Prevent non-numeric key entries
            if (!/^\d*$/.test(value)) {
                e.target.value = "";
                return;
            }
            if (value.length === 1 && index < inputs.length - 1) {
                inputs[index + 1].focus();
            }
            updateHiddenInput();
        });

        // Shift focus back on backspace keypress
        input.addEventListener("keydown", (e) => {
            if (e.key === "Backspace" && !e.target.value && index > 0) {
                inputs[index - 1].focus();
            }
        });

        // Intercept standard full 6-digit paste event strings
        input.addEventListener("paste", (e) => {
            if (index === 0) {
                const pasteData = (e.clipboardData || window.clipboardData).getData("text").trim();
                if (/^\d{6}$/.test(pasteData)) {
                    inputs.forEach((inp, idx) => {
                        inp.value = pasteData[idx];
                    });
                    inputs[inputs.length - 1].focus();
                    updateHiddenInput();
                    e.preventDefault();
                }
            }
        });
    });

    // Combines segmented box values into the model input property
    function updateHiddenInput() {
        let otpValue = "";
        inputs.forEach(input => {
            otpValue += input.value;
        });
        hiddenOtpInput.value = otpValue;
    }

    // Block submission fires if form digits are incomplete
    form.addEventListener("submit", function (e) {
        updateHiddenInput();
        if (hiddenOtpInput.value.length !== 6) {
            e.preventDefault();
            alert("Please enter all 6 digits of your verification code.");
        }
    });

    // 2. Countdown Timer functionality matching image layout (00:58)
    let totalSeconds = 58;
    const countdownElement = document.getElementById("countdown");
    const resendLink = document.getElementById("resendLink");

    const interval = setInterval(() => {
        totalSeconds--;
        let minutes = Math.floor(totalSeconds / 60);
        let seconds = totalSeconds % 60;

        let displayMinutes = minutes < 10 ? "0" + minutes : minutes;
        let displaySeconds = seconds < 10 ? "0" + seconds : seconds;

        countdownElement.textContent = `${displayMinutes}:${displaySeconds}`;

        if (totalSeconds <= 0) {
            clearInterval(interval);
            const badgeContainer = document.querySelector(".timer-badge-container");
            if (badgeContainer) badgeContainer.style.display = "none";

            // Remove the CSS blockades so the anchor tag works normally
            resendLink.classList.remove("disabled-link");
            resendLink.style.color = "#0052cc";
            resendLink.style.cursor = "pointer";
        }
    }, 1000);
});