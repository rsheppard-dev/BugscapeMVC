"use strict";
var Modal = /** @class */ (function () {
    function Modal() {
        this.modal = document.querySelector('[data-modal]');
        this.openButton = document.querySelector('[data-open-modal]');
    }
    Modal.prototype.init = function () {
        var _this = this;
        this.openButton.addEventListener('click', function (e) {
            e.preventDefault();
            _this.modal.showModal();
            // close dialog when clicking outside of it
            _this.modal.addEventListener('click', function (e) {
                console.log(e.target);
                console.log(_this.modal);
                if (e.target === _this.modal) {
                    _this.modal.close();
                }
            });
        });
    };
    return Modal;
}());
