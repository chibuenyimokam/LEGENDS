
document.addEventListener("DOMContentLoaded", function () {
    const inputs = document.querySelectorAll(".otp-input");
    const hiddenOtpInput = document.getElementById("hiddenOtpCode");
    const form = document.getElementById("otpForm");

    inputs.forEach((input, index) => {
        input.addEventListener("input", (e) => {
            const value = e.target.value;
            if (!/^\d*$/.test(value)) {
                e.target.value = "";
                return;
            }
            if (value.length === 1 && index < inputs.length - 1) {
                inputs[index + 1].focus();
            }
            updateHiddenInput();
        });

        input.addEventListener("keydown", (e) => {
            if (e.key === "Backspace" && !e.target.value && index > 0) {
                inputs[index - 1].focus();
            }
        });

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

    function updateHiddenInput() {
        let otpValue = "";
        inputs.forEach(input => {
            otpValue += input.value;
        });
        hiddenOtpInput.value = otpValue;
    }

    form.addEventListener("submit", function (e) {
        updateHiddenInput();
        if (hiddenOtpInput.value.length !== 6) {
            e.preventDefault();
            alert("Please enter all 6 digits of your verification code.");
        }
    });

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

            resendLink.classList.remove("disabled-link");
            resendLink.style.color = "#0052cc";
            resendLink.style.cursor = "pointer";
        }
    }, 1000);
});