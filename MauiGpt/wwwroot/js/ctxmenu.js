"use strict";
var _createClass = function () {
    function e(e, n) {
        for (var t = 0; t < n.length; t++) {
            var s = n[t];
            s.enumerable = s.enumerable || !1, s.configurable = !0, "value" in s && (s.writable = !0), Object.defineProperty(e, s.key, s)
        }
    }
    return function (n, t, s) {
        return t && e(n.prototype, t), s && e(n, s), n
    }
}();

function _classCallCheck(e, n) {
    if (!(e instanceof n)) throw new TypeError("Cannot call a class as a function")
}
var ECtxMenuNames = {
    menu: "ctx-menu-wrapper",
    item: "ctx-menu-item",
    separator: "ctx-menu-separator",
    hasIcon: "ctx-menu-hasIcon",
    darkInvert: "ctx-menu-darkInvert"
},
    CtxMenuManagerClass = function () {
        function e() {
            _classCallCheck(this, e), this._currentMenuVisible = null, this._ctxMenus = new Map, document.addEventListener("contextmenu", this._eventOpenMenu.bind(this));
            var n = document.getElementsByTagName("script"),
                t = n[n.length - 1].src.split("?")[0].split("/").slice(0, -1).join("/") + "/",
                s = document.createElement("link");
            s.href = t + "ctxmenu.css", s.type = "text/css", s.rel = "stylesheet", document.getElementsByTagName("head")[0].appendChild(s)
        }
        return _createClass(e, [{
            key: "_eventOpenMenu",
            value: function (e) {
                if (null != e.path) var n = this._traceCtxMenu(e.path);
                else n = this._msEdgeTraceCtxMenu(e.target);
                if (this.closeCurrentlyOpenedMenu(), null != n) {
                    var t = n[0],
                        s = n[1];
                    0 != t ? 1 != t && (t._elementClicked = s, t.openMenu(e.pageX, e.pageY), this._currentMenuVisible = t, document.addEventListener("click", CtxCloseCurrentlyOpenedMenus), window.addEventListener("resize", CtxCloseCurrentlyOpenedMenus), e.preventDefault(), null != t._openEventListener && t._openEventListener()) : e.preventDefault()
                }
            }
        }, {
            key: "closeMenu",
            value: function (e) {
                e.closeMenu(), this._currentMenuVisible = null, document.removeEventListener("click", CtxCloseCurrentlyOpenedMenus), window.removeEventListener("resize", CtxCloseCurrentlyOpenedMenus)
            }
        }, {
            key: "closeCurrentlyOpenedMenu",
            value: function () {
                null != this._currentMenuVisible && this.closeMenu(this._currentMenuVisible)
            }
        }, {
            key: "_traceCtxMenu",
            value: function (e) {
                for (var n = 0; n < e.length; ++n) {
                    var t = this._ctxMenusHas(e[n]);
                    if (null != t) return [t, e[n]]
                }
                return null
            }
        }, {
            key: "_msEdgeTraceCtxMenu",
            value: function (e) {
                for (; null != e;) {
                    var n = this._ctxMenusHas(e);
                    if (null != n) return [n, e];
                    e = e.parentNode
                }
                return null
            }
        }, {
            key: "_ctxMenusHas",
            value: function (e) {
                if (this._ctxMenus.has(e)) return this._ctxMenus.get(e);
                if (this._ctxMenus.has("#" + e.id)) return this._ctxMenus.get("#" + e.id);
                if (null != e.className)
                    for (var n = e.className.split(" "), t = 0; t < n.length; t++)
                        if (this._ctxMenus.has("." + n[t])) return this._ctxMenus.get("." + n[t]);
                return this._ctxMenus.has(e.nodeName) ? this._ctxMenus.get(e.nodeName) : null
            }
        }, {
            key: "getMenuFromElement",
            value: function (e) {
                return this._ctxMenus.get(e)
            }
        }, {
            key: "createNewMenu",
            value: function (e) {
                var n = new CtxMenuClass;
                return this._ctxMenus.set(e, n), n
            }
        }, {
            key: "setCustomContexMenuValue",
            value: function (e, n) {
                this._ctxMenus.set(e, n)
            }
        }]), e
    }(),
    CtxMenuClass = function () {
        function e() {
            _classCallCheck(this, e), this.menuContainer = document.createElement("div"), this.menuContainer.className = ECtxMenuNames.menu, document.body.appendChild(this.menuContainer), this.closeMenu(), this._elementClicked = void 0, this._openEventListener = void 0, this._closeEventListener = void 0, this._clickEventListener = void 0
        }
        return _createClass(e, [{
            key: "addItem",
            value: function (e, n) {
                var t = arguments.length <= 2 || void 0 === arguments[2] ? void 0 : arguments[2],
                    s = arguments.length <= 3 || void 0 === arguments[3] ? void 0 : arguments[3],
                    u = !(arguments.length <= 4 || void 0 === arguments[4]) && arguments[4],
                    i = document.createElement("div");
                i.className = ECtxMenuNames.item;
                var l = document.createElement("img");
                if (null != t && null != t) {
                    l.src = t;
                    var r = !0;
                    console.log(u), u && (l.className = ECtxMenuNames.darkInvert)
                } else r = !1;
                return i.appendChild(l), i.innerHTML += e, i.addEventListener("click", function () {
                    this._callItem(n)
                }.bind(this)), r && this.menuContainer.classList.add(ECtxMenuNames.hasIcon), this.menuContainer.insertBefore(i, this.menuContainer.childNodes[s]), i
            }
        }, {
            key: "addSeparator",
            value: function () {
                var e = arguments.length <= 0 || void 0 === arguments[0] ? void 0 : arguments[0],
                    n = document.createElement("div");
                return n.className = ECtxMenuNames.separator, this.menuContainer.insertBefore(n, this.menuContainer.childNodes[e]), n
            }
        }, {
            key: "getItems",
            value: function () {
                return this.menuContainer.childNodes
            }
        }, {
            key: "getItemAtIndex",
            value: function (e) {
                return this.menuContainer.childNodes[e]
            }
        }, {
            key: "addEventListener",
            value: function (e, n) {
                switch (e) {
                    case "open":
                        this._openEventListener = n;
                    case "close":
                        this._closeEventListener = n;
                    case "click":
                        this._clickEventListener = n
                }
            }
        }, {
            key: "openMenu",
            value: function (e, n) {
                this.menuContainer.style.display = "block";
                var t = document.documentElement.clientWidth + document.documentElement.scrollLeft,
                    s = document.documentElement.clientHeight + document.documentElement.scrollTop;
                e + this.menuContainer.offsetWidth > t && (e = t - this.menuContainer.offsetWidth - 1), n + this.menuContainer.offsetHeight > s && (n = s - this.menuContainer.offsetHeight - 1), this.menuContainer.style.left = e + "px", this.menuContainer.style.top = n + "px"
            }
        }, {
            key: "closeMenu",
            value: function () {
                this.menuContainer.style.left = "0px", this.menuContainer.style.top = "0px", this.menuContainer.style.display = "none", null != this._closeEventListener && this._closeEventListener()
            }
        }, {
            key: "_callItem",
            value: function (e) {
                this.closeMenu(), setTimeout(function () {
                    e(this._elementClicked), null != this._clickEventListener && this._clickEventListener(item)
                }.bind(this), 1)
            }
        }]), e
    }();

function CtxMenu(e) {
    return null == e && (e = document), null != ctxMenuManager.getMenuFromElement(e) ? ctxMenuManager.getMenuFromElement(e) : ctxMenuManager.createNewMenu(e)
}

function CtxMenuBlock(e) {
    ctxMenuManager.setCustomContexMenuValue(e, !1)
}

function CtxMenuDefault(e) {
    ctxMenuManager.setCustomContexMenuValue(e, !0)
}

function CtxCloseCurrentlyOpenedMenus() {
    ctxMenuManager.closeCurrentlyOpenedMenu()
}
var ctxMenuManager = new CtxMenuManagerClass;
