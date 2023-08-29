"use strict";
const multiStepForm = document.querySelector("[data-multi-step]");
const formSteps = Array.from(multiStepForm.querySelectorAll("[data-step]"));
let currentStep = formSteps.findIndex((step) => step.classList.contains('active'));
if (currentStep < 0) {
    currentStep = 0;
    showCurrentStep();
}
multiStepForm.addEventListener("click", e => {
    const target = e.target;
    let incrementor;
    if (target.matches("[data-next]"))
        incrementor = 1;
    else if (target.matches("[data-previous]"))
        incrementor = -1;
    else
        return;
    const inputs = Array.from(formSteps[currentStep].querySelectorAll("input"));
    const isValid = inputs.every(input => {
        return input.reportValidity();
    });
    if (isValid) {
        currentStep += incrementor;
        showCurrentStep();
    }
    showCurrentStep();
});
function showCurrentStep() {
    formSteps.forEach((step, index) => {
        step.classList.toggle("active", index === currentStep);
    });
}
