"use strict";
var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
var __generator = (this && this.__generator) || function (thisArg, body) {
    var _ = { label: 0, sent: function() { if (t[0] & 1) throw t[1]; return t[1]; }, trys: [], ops: [] }, f, y, t, g;
    return g = { next: verb(0), "throw": verb(1), "return": verb(2) }, typeof Symbol === "function" && (g[Symbol.iterator] = function() { return this; }), g;
    function verb(n) { return function (v) { return step([n, v]); }; }
    function step(op) {
        if (f) throw new TypeError("Generator is already executing.");
        while (g && (g = 0, op[0] && (_ = 0)), _) try {
            if (f = 1, y && (t = op[0] & 2 ? y["return"] : op[0] ? y["throw"] || ((t = y["return"]) && t.call(y), 0) : y.next) && !(t = t.call(y, op[1])).done) return t;
            if (y = 0, t) op = [op[0] & 2, t.value];
            switch (op[0]) {
                case 0: case 1: t = op; break;
                case 4: _.label++; return { value: op[1], done: false };
                case 5: _.label++; y = op[1]; op = [0]; continue;
                case 7: op = _.ops.pop(); _.trys.pop(); continue;
                default:
                    if (!(t = _.trys, t = t.length > 0 && t[t.length - 1]) && (op[0] === 6 || op[0] === 2)) { _ = 0; continue; }
                    if (op[0] === 3 && (!t || (op[1] > t[0] && op[1] < t[3]))) { _.label = op[1]; break; }
                    if (op[0] === 6 && _.label < t[1]) { _.label = t[1]; t = op; break; }
                    if (t && _.label < t[2]) { _.label = t[2]; _.ops.push(op); break; }
                    if (t[2]) _.ops.pop();
                    _.trys.pop(); continue;
            }
            op = body.call(thisArg, _);
        } catch (e) { op = [6, e]; y = 0; } finally { f = t = 0; }
        if (op[0] & 5) throw op[1]; return { value: op[0] ? op[1] : void 0, done: true };
    }
};
var TicketsTable = /** @class */ (function () {
    function TicketsTable(query, options) {
        var _a, _b, _c, _d, _e, _f, _g;
        this.query = query !== null && query !== void 0 ? query : 'getAllTickets',
            this.container = (_a = options === null || options === void 0 ? void 0 : options.container) !== null && _a !== void 0 ? _a : document.querySelector('[data-container="tickets"]'),
            this.page = (_b = options === null || options === void 0 ? void 0 : options.page) !== null && _b !== void 0 ? _b : 1,
            this.limit = (_c = options === null || options === void 0 ? void 0 : options.limit) !== null && _c !== void 0 ? _c : 10,
            this.sortBy = (_d = options === null || options === void 0 ? void 0 : options.sortBy) !== null && _d !== void 0 ? _d : 'date',
            this.order = (_e = options === null || options === void 0 ? void 0 : options.order) !== null && _e !== void 0 ? _e : 'desc';
        this.memberId = (_f = options === null || options === void 0 ? void 0 : options.memberId) !== null && _f !== void 0 ? _f : undefined;
        this.projectId = (_g = options === null || options === void 0 ? void 0 : options.projectId) !== null && _g !== void 0 ? _g : undefined;
    }
    TicketsTable.prototype.init = function () {
        return __awaiter(this, void 0, void 0, function () {
            var _this = this;
            return __generator(this, function (_a) {
                this.getTickets();
                this.container.addEventListener('click', function (event) { return __awaiter(_this, void 0, void 0, function () {
                    var header;
                    var _a, _b, _c;
                    return __generator(this, function (_d) {
                        header = event.target.closest('[data-sortable]');
                        if (header) {
                            if (header.getAttribute('data-order') === this.order)
                                this.order =
                                    header.getAttribute('data-order') === 'asc' ? 'desc' : 'asc';
                            this.page =
                                (_a = parseInt(header.getAttribute('data-page'))) !== null && _a !== void 0 ? _a : this.page;
                            this.limit =
                                (_b = parseInt(header.getAttribute('data-limit'))) !== null && _b !== void 0 ? _b : this.limit;
                            this.sortBy = (_c = header.getAttribute('data-sort')) !== null && _c !== void 0 ? _c : this.sortBy;
                            this.getTickets();
                        }
                        return [2 /*return*/];
                    });
                }); });
                return [2 /*return*/];
            });
        });
    };
    TicketsTable.prototype.getTickets = function () {
        return __awaiter(this, void 0, void 0, function () {
            var url, response, html, header, arrow, error_1;
            return __generator(this, function (_a) {
                switch (_a.label) {
                    case 0:
                        _a.trys.push([0, 3, , 4]);
                        url = "/Tickets/GetTicketsByQuery?query=".concat(this.query, "&page=").concat(this.page, "&sortBy=").concat(this.sortBy, "&order=").concat(this.order, "&limit=").concat(this.limit, "&memberId=").concat(this.memberId, "&projectId=").concat(this.projectId);
                        return [4 /*yield*/, fetch(url)];
                    case 1:
                        response = _a.sent();
                        if (!response.ok) {
                            throw new Error("Error loading tickets table: ".concat(response.statusText));
                        }
                        return [4 /*yield*/, response.text()];
                    case 2:
                        html = _a.sent();
                        this.container.innerHTML = html;
                        header = this.container.querySelector("[data-sort=\"".concat(this.sortBy, "\"]"));
                        header.setAttribute('data-order', this.order);
                        header.classList.add('active');
                        arrow = header.querySelector('.order');
                        arrow === null || arrow === void 0 ? void 0 : arrow.classList.add(this.order);
                        return [3 /*break*/, 4];
                    case 3:
                        error_1 = _a.sent();
                        console.error(error_1);
                        return [3 /*break*/, 4];
                    case 4: return [2 /*return*/];
                }
            });
        });
    };
    return TicketsTable;
}());
