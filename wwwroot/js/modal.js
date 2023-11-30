"use strict";
var Modal = /** @class */ (function () {
    function Modal() {
        this.modal = document.querySelector('[data-modal]');
        this.openButton = document.querySelector('[data-open-modal]');
        this.closeButton = document.querySelector('[data-close-modal]');
    }
    Modal.prototype.init = function () {
        var _this = this;
        // open dialog when clicking open button
        this.openButton.addEventListener('click', function () {
            _this.modal.showModal();
            // close dialog when clicking close button
            _this.closeButton.addEventListener('click', function () {
                _this.modal.close();
            });
            // close dialog when clicking outside of it
            _this.modal.addEventListener('click', function (e) {
                if (e.target === _this.modal) {
                    _this.modal.close();
                }
            });
        });
    };
    return Modal;
}());
