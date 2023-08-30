"use strict";
class ImagePreview {
    constructor(input, output) {
        this.input = input;
        this.output = output;
    }
    init() {
        this.input.addEventListener('change', () => {
            var _a;
            const file = (_a = this.input.files) === null || _a === void 0 ? void 0 : _a[0];
            if (file) {
                const reader = new FileReader();
                reader.onload = (event) => {
                    var _a;
                    if (this.output.src && ((_a = event.target) === null || _a === void 0 ? void 0 : _a.result)) {
                        this.output.src = event.target.result.toString();
                    }
                };
                reader.readAsDataURL(file);
            }
        });
    }
}
