"use strict";
var MobileMenu = /** @class */ (function () {
    function MobileMenu() {
        this.toggleButton = document.querySelector('[data-toggle-menu]');
        this.menu = document.querySelector('[data-menu]');
        this.isOpen = false;
    }
    MobileMenu.prototype.init = function () {
        var _this = this;
        this.toggleButton.addEventListener('click', function (event) {
            event.stopPropagation();
            if (_this.isOpen) {
                _this.closeMenu();
            }
            else {
                _this.openMenu();
            }
        });
        document.addEventListener('click', function (event) {
            if (!_this.menu.contains(event.target) && _this.isOpen) {
                _this.closeMenu();
            }
        });
    };
    MobileMenu.prototype.openMenu = function () {
        this.isOpen = true;
        this.menu.classList.remove('-translate-x-full');
        this.menu.classList.add('translate-x-0');
        this.toggleButton.innerHTML = "\n        <svg xmlns=\"http://www.w3.org/2000/svg\" aria-hidden fill=\"none\" viewBox=\"0 0 24 24\" stroke-width=\"1.5\" stroke=\"currentColor\" class=\"w-6 h-6 stroke-dark\">\n            <path stroke-linecap=\"round\" stroke-linejoin=\"round\" d=\"M6 18L18 6M6 6l12 12\" />\n        </svg>\n        ";
        this.toggleButton.title = 'Close Menu';
    };
    MobileMenu.prototype.closeMenu = function () {
        this.isOpen = false;
        this.menu.classList.remove('translate-x-0');
        this.menu.classList.add('-translate-x-full');
        this.toggleButton.innerHTML = "\n        <svg xmlns=\"http://www.w3.org/2000/svg\" aria-hidden fill=\"none\" viewBox=\"0 0 24 24\" stroke-width=\"1.5\"\n            stroke=\"currentColor\" class=\"w-6 h-6 stroke-dark\">\n            <path stroke-linecap=\"round\" stroke-linejoin=\"round\"\n                d=\"M3.75 6.75h16.5M3.75 12h16.5m-16.5 5.25h16.5\" />\n        </svg>\n        ";
        this.toggleButton.title = 'Open Menu';
    };
    return MobileMenu;
}());
var instance = new MobileMenu();
instance.init();
