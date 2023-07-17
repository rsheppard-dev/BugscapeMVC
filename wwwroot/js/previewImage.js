"use strict";
function previewImage(inputId, outputId) {
    const imageInput = document.getElementById(inputId);
    const imageOutput = document.getElementById(outputId);
    imageInput.addEventListener('change', function () {
        var _a;
        const file = (_a = this.files) === null || _a === void 0 ? void 0 : _a[0];
        if (file) {
            const reader = new FileReader();
            reader.onload = function (event) {
                var _a;
                if (imageOutput.src && ((_a = event.target) === null || _a === void 0 ? void 0 : _a.result)) {
                    imageOutput.src = event.target.result.toString();
                }
            };
            reader.readAsDataURL(file);
        }
    });
}
