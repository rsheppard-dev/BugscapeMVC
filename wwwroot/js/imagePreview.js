"use strict";
var ImagePreview = /** @class */ (function () {
    function ImagePreview(imageSource) {
        this.inputs = document.querySelectorAll('[data-image-input]');
        this.outputs = document.querySelectorAll('[data-image-output]');
        this.saveButton = document.querySelector('[data-save-button]');
        this.imageSource = this.decodeHtml(imageSource);
    }
    ImagePreview.prototype.init = function () {
        var _this = this;
        if (this.saveButton)
            this.saveButton.disabled = true;
        this.inputs.forEach(function (input, index) {
            input.addEventListener('click', function () {
                if (_this.imageSource)
                    _this.outputs[index].src = _this.imageSource;
                if (_this.saveButton)
                    _this.saveButton.disabled = true;
            });
            input.addEventListener('change', function (event) {
                var _a, _b;
                var file = (_b = (_a = event.target) === null || _a === void 0 ? void 0 : _a.files) === null || _b === void 0 ? void 0 : _b[0];
                if (file) {
                    var reader_1 = new FileReader();
                    reader_1.onload = function () {
                        if (_this.outputs[index]) {
                            _this.outputs[index].src = reader_1.result;
                        }
                    };
                    reader_1.readAsDataURL(file);
                    if (_this.saveButton)
                        _this.saveButton.disabled = false;
                }
            });
        });
    };
    ImagePreview.prototype.decodeHtml = function (html) {
        if (html === void 0) { html = ''; }
        var txt = document.createElement("textarea");
        txt.innerHTML = html;
        return txt.value;
    };
    return ImagePreview;
}());
