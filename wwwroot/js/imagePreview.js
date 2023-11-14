"use strict";
var ImagePreview = /** @class */ (function () {
    function ImagePreview(input, output) {
        this.input = input;
        this.output = output;
    }
    ImagePreview.prototype.init = function () {
        var _this = this;
        this.input.addEventListener('change', function () {
            var _a;
            var file = (_a = _this.input.files) === null || _a === void 0 ? void 0 : _a[0];
            if (file) {
                var reader = new FileReader();
                reader.onload = function (event) {
                    var _a;
                    if (_this.output.src && ((_a = event.target) === null || _a === void 0 ? void 0 : _a.result)) {
                        _this.output.src = event.target.result.toString();
                    }
                };
                reader.readAsDataURL(file);
            }
        });
    };
    return ImagePreview;
}());
