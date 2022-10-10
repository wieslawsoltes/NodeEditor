var __async = (__this, __arguments, generator) => {
  return new Promise((resolve, reject) => {
    var fulfilled = (value) => {
      try {
        step(generator.next(value));
      } catch (e) {
        reject(e);
      }
    };
    var rejected = (value) => {
      try {
        step(generator.throw(value));
      } catch (e) {
        reject(e);
      }
    };
    var step = (x) => x.done ? resolve(x.value) : Promise.resolve(x.value).then(fulfilled, rejected);
    step((generator = generator.apply(__this, __arguments)).next());
  });
};

// modules/avalonia/canvas.ts
var Canvas = class {
  constructor(useGL, element, renderFrameCallback) {
    this.renderLoopEnabled = false;
    this.renderLoopRequest = 0;
    this.htmlCanvas = element;
    this.renderFrameCallback = renderFrameCallback;
    if (useGL) {
      const ctx = Canvas.createWebGLContext(element);
      if (!ctx) {
        console.error("Failed to create WebGL context");
        return;
      }
      const GL = globalThis.AvaloniaGL;
      GL.makeContextCurrent(ctx);
      const GLctx = GL.currentContext.GLctx;
      const fbo = GLctx.getParameter(GLctx.FRAMEBUFFER_BINDING);
      this.glInfo = {
        context: ctx,
        fboId: fbo ? fbo.id : 0,
        stencil: GLctx.getParameter(GLctx.STENCIL_BITS),
        sample: 0,
        depth: GLctx.getParameter(GLctx.DEPTH_BITS)
      };
    }
  }
  static initGL(element, elementId, renderFrameCallback) {
    const view = Canvas.init(true, element, elementId, renderFrameCallback);
    if (!view || !view.glInfo) {
      return null;
    }
    return view.glInfo;
  }
  static init(useGL, element, elementId, renderFrameCallback) {
    const htmlCanvas = element;
    if (!htmlCanvas) {
      console.error("No canvas element was provided.");
      return null;
    }
    if (!Canvas.elements) {
      Canvas.elements = /* @__PURE__ */ new Map();
    }
    Canvas.elements.set(elementId, element);
    const view = new Canvas(useGL, element, renderFrameCallback);
    htmlCanvas.Canvas = view;
    return view;
  }
  setEnableRenderLoop(enable) {
    this.renderLoopEnabled = enable;
    if (enable) {
      this.requestAnimationFrame();
    } else if (this.renderLoopRequest !== 0) {
      window.cancelAnimationFrame(this.renderLoopRequest);
      this.renderLoopRequest = 0;
    }
  }
  requestAnimationFrame(renderLoop) {
    if (renderLoop !== void 0 && this.renderLoopEnabled !== renderLoop) {
      this.setEnableRenderLoop(renderLoop);
    }
    if (this.renderLoopRequest !== 0) {
      return;
    }
    this.renderLoopRequest = window.requestAnimationFrame(() => {
      var _a, _b;
      if (this.glInfo) {
        const GL = globalThis.AvaloniaGL;
        GL.makeContextCurrent(this.glInfo.context);
      }
      if (this.htmlCanvas.width !== this.newWidth) {
        this.htmlCanvas.width = (_a = this.newWidth) != null ? _a : 0;
      }
      if (this.htmlCanvas.height !== this.newHeight) {
        this.htmlCanvas.height = (_b = this.newHeight) != null ? _b : 0;
      }
      this.renderFrameCallback();
      this.renderLoopRequest = 0;
      if (this.renderLoopEnabled) {
        this.requestAnimationFrame();
      }
    });
  }
  setCanvasSize(width, height) {
    this.newWidth = width;
    this.newHeight = height;
    if (this.htmlCanvas.width !== this.newWidth) {
      this.htmlCanvas.width = this.newWidth;
    }
    if (this.htmlCanvas.height !== this.newHeight) {
      this.htmlCanvas.height = this.newHeight;
    }
    if (this.glInfo) {
      const GL = globalThis.AvaloniaGL;
      GL.makeContextCurrent(this.glInfo.context);
    }
  }
  static setCanvasSize(element, width, height) {
    const htmlCanvas = element;
    if (!htmlCanvas || !htmlCanvas.Canvas) {
      return;
    }
    htmlCanvas.Canvas.setCanvasSize(width, height);
  }
  static requestAnimationFrame(element, renderLoop) {
    const htmlCanvas = element;
    if (!htmlCanvas || !htmlCanvas.Canvas) {
      return;
    }
    htmlCanvas.Canvas.requestAnimationFrame(renderLoop);
  }
  static createWebGLContext(htmlCanvas) {
    const contextAttributes = {
      alpha: 1,
      depth: 1,
      stencil: 8,
      antialias: 0,
      premultipliedAlpha: 1,
      preserveDrawingBuffer: 0,
      preferLowPowerToHighPerformance: 0,
      failIfMajorPerformanceCaveat: 0,
      majorVersion: 2,
      minorVersion: 0,
      enableExtensionsByDefault: 1,
      explicitSwapControl: 0,
      renderViaOffscreenBackBuffer: 1
    };
    const GL = globalThis.AvaloniaGL;
    let ctx = GL.createContext(htmlCanvas, contextAttributes);
    if (!ctx && contextAttributes.majorVersion > 1) {
      console.warn("Falling back to WebGL 1.0");
      contextAttributes.majorVersion = 1;
      contextAttributes.minorVersion = 0;
      ctx = GL.createContext(htmlCanvas, contextAttributes);
    }
    return ctx;
  }
};
var SizeWatcher = class {
  static observe(element, elementId, callback) {
    if (!element || !callback) {
      return;
    }
    SizeWatcher.init();
    const watcherElement = element;
    watcherElement.SizeWatcher = {
      callback
    };
    SizeWatcher.elements.set(elementId, element);
    SizeWatcher.observer.observe(element);
    SizeWatcher.invoke(element);
  }
  static unobserve(elementId) {
    if (!elementId || !SizeWatcher.observer) {
      return;
    }
    const element = SizeWatcher.elements.get(elementId);
    if (element) {
      SizeWatcher.elements.delete(elementId);
      SizeWatcher.observer.unobserve(element);
    }
  }
  static init() {
    if (SizeWatcher.observer) {
      return;
    }
    SizeWatcher.elements = /* @__PURE__ */ new Map();
    SizeWatcher.observer = new ResizeObserver((entries) => {
      for (const entry of entries) {
        SizeWatcher.invoke(entry.target);
      }
    });
  }
  static invoke(element) {
    const watcherElement = element;
    const instance = watcherElement.SizeWatcher;
    if (!instance || !instance.callback) {
      return;
    }
    return instance.callback(element.clientWidth, element.clientHeight);
  }
};
var DpiWatcher = class {
  static getDpi() {
    return window.devicePixelRatio;
  }
  static start(callback) {
    DpiWatcher.lastDpi = window.devicePixelRatio;
    DpiWatcher.timerId = window.setInterval(DpiWatcher.update, 1e3);
    DpiWatcher.callback = callback;
    return DpiWatcher.lastDpi;
  }
  static stop() {
    window.clearInterval(DpiWatcher.timerId);
  }
  static update() {
    if (!DpiWatcher.callback) {
      return;
    }
    const currentDpi = window.devicePixelRatio;
    const lastDpi = DpiWatcher.lastDpi;
    DpiWatcher.lastDpi = currentDpi;
    if (Math.abs(lastDpi - currentDpi) > 1e-3) {
      DpiWatcher.callback(lastDpi, currentDpi);
    }
  }
};

// modules/avalonia/caretHelper.ts
var CaretHelper = class {
  static getCaretCoordinates(element, position, options) {
    var _a, _b;
    if (!isBrowser) {
      throw new Error(
        "textarea-caret-position#getCaretCoordinates should only be called in a browser"
      );
    }
    const debug = (_a = options == null ? void 0 : options.debug) != null ? _a : false;
    if (debug) {
      const el = document.querySelector(
        "#input-textarea-caret-position-mirror-div"
      );
      if (el)
        (_b = el.parentNode) == null ? void 0 : _b.removeChild(el);
    }
    const div = document.createElement("div");
    div.id = "input-textarea-caret-position-mirror-div";
    document.body.appendChild(div);
    const style = div.style;
    const computed = window.getComputedStyle ? window.getComputedStyle(element) : element.currentStyle;
    const isInput = element.nodeName === "INPUT";
    style.whiteSpace = "pre-wrap";
    if (!isInput)
      style.wordWrap = "break-word";
    style.position = "absolute";
    if (!debug)
      style.visibility = "hidden";
    properties.forEach((prop) => {
      if (isInput && prop === "lineHeight") {
        if (computed.boxSizing === "border-box") {
          const height = parseInt(computed.height);
          const outerHeight = parseInt(computed.paddingTop) + parseInt(computed.paddingBottom) + parseInt(computed.borderTopWidth) + parseInt(computed.borderBottomWidth);
          const targetHeight = outerHeight + parseInt(computed.lineHeight);
          if (height > targetHeight) {
            style.lineHeight = `${height - outerHeight}px`;
          } else if (height === targetHeight) {
            style.lineHeight = computed.lineHeight;
          } else {
            style.lineHeight = "0";
          }
        } else {
          style.lineHeight = computed.height;
        }
      } else {
        style[prop] = computed[prop];
      }
    });
    if (isFirefox) {
      if (element.scrollHeight > parseInt(computed.height)) {
        style.overflowY = "scroll";
      }
    } else {
      style.overflow = "hidden";
    }
    div.textContent = element.value.substring(0, position);
    if (isInput)
      div.textContent = div.textContent.replace(/\s/g, "\xA0");
    const span = document.createElement("span");
    span.textContent = element.value.substring(position) || ".";
    div.appendChild(span);
    const coordinates = {
      top: span.offsetTop + parseInt(computed.borderTopWidth),
      left: span.offsetLeft + parseInt(computed.borderLeftWidth),
      height: parseInt(computed.lineHeight)
    };
    if (debug) {
      span.style.backgroundColor = "#aaa";
    } else {
      document.body.removeChild(div);
    }
    return coordinates;
  }
};
var properties = [
  "direction",
  "boxSizing",
  "width",
  "height",
  "overflowX",
  "overflowY",
  "borderTopWidth",
  "borderRightWidth",
  "borderBottomWidth",
  "borderLeftWidth",
  "borderStyle",
  "paddingTop",
  "paddingRight",
  "paddingBottom",
  "paddingLeft",
  "fontStyle",
  "fontVariant",
  "fontWeight",
  "fontStretch",
  "fontSize",
  "fontSizeAdjust",
  "lineHeight",
  "fontFamily",
  "textAlign",
  "textTransform",
  "textIndent",
  "textDecoration",
  "letterSpacing",
  "wordSpacing",
  "tabSize",
  "MozTabSize"
];
var isBrowser = typeof window !== "undefined";
var isFirefox = isBrowser && window.mozInnerScreenX != null;

// modules/avalonia/input.ts
var InputHelper = class {
  static subscribeKeyEvents(element, keyDownCallback, keyUpCallback) {
    const keyDownHandler = (args) => {
      if (keyDownCallback(args.code, args.key, this.getModifiers(args))) {
        args.preventDefault();
      }
    };
    element.addEventListener("keydown", keyDownHandler);
    const keyUpHandler = (args) => {
      if (keyUpCallback(args.code, args.key, this.getModifiers(args))) {
        args.preventDefault();
      }
    };
    element.addEventListener("keyup", keyUpHandler);
    return () => {
      element.removeEventListener("keydown", keyDownHandler);
      element.removeEventListener("keyup", keyUpHandler);
    };
  }
  static subscribeTextEvents(element, inputCallback, compositionStartCallback, compositionUpdateCallback, compositionEndCallback) {
    const inputHandler = (args) => {
      const inputEvent = args;
      if (inputCallback(inputEvent.type, inputEvent.data)) {
        args.preventDefault();
      }
    };
    element.addEventListener("input", inputHandler);
    const compositionStartHandler = (args) => {
      if (compositionStartCallback(args)) {
        args.preventDefault();
      }
    };
    element.addEventListener("compositionstart", compositionStartHandler);
    const compositionUpdateHandler = (args) => {
      if (compositionUpdateCallback(args)) {
        args.preventDefault();
      }
    };
    element.addEventListener("compositionupdate", compositionUpdateHandler);
    const compositionEndHandler = (args) => {
      if (compositionEndCallback(args)) {
        args.preventDefault();
      }
    };
    element.addEventListener("compositionend", compositionEndHandler);
    return () => {
      element.removeEventListener("input", inputHandler);
      element.removeEventListener("compositionstart", compositionStartHandler);
      element.removeEventListener("compositionupdate", compositionUpdateHandler);
      element.removeEventListener("compositionend", compositionEndHandler);
    };
  }
  static subscribePointerEvents(element, pointerMoveCallback, pointerDownCallback, pointerUpCallback, wheelCallback) {
    const pointerMoveHandler = (args) => {
      if (pointerMoveCallback(args)) {
        args.preventDefault();
      }
    };
    const pointerDownHandler = (args) => {
      if (pointerDownCallback(args)) {
        args.preventDefault();
      }
    };
    const pointerUpHandler = (args) => {
      if (pointerUpCallback(args)) {
        args.preventDefault();
      }
    };
    const wheelHandler = (args) => {
      if (wheelCallback(args)) {
        args.preventDefault();
      }
    };
    element.addEventListener("pointermove", pointerMoveHandler);
    element.addEventListener("pointerdown", pointerDownHandler);
    element.addEventListener("pointerup", pointerUpHandler);
    element.addEventListener("wheel", wheelHandler);
    return () => {
      element.removeEventListener("pointerover", pointerMoveHandler);
      element.removeEventListener("pointerdown", pointerDownHandler);
      element.removeEventListener("pointerup", pointerUpHandler);
      element.removeEventListener("wheel", wheelHandler);
    };
  }
  static subscribeInputEvents(element, inputCallback) {
    const inputHandler = (args) => {
      if (inputCallback(args.value)) {
        args.preventDefault();
      }
    };
    element.addEventListener("input", inputHandler);
    return () => {
      element.removeEventListener("input", inputHandler);
    };
  }
  static clearInput(inputElement) {
    inputElement.value = "";
  }
  static focusElement(inputElement) {
    inputElement.focus();
  }
  static setCursor(inputElement, kind) {
    inputElement.style.cursor = kind;
  }
  static setBounds(inputElement, x, y, caretWidth, caretHeight, caret) {
    inputElement.style.left = x.toFixed(0) + "px";
    inputElement.style.top = y.toFixed(0) + "px";
    const { left, top } = CaretHelper.getCaretCoordinates(inputElement, caret);
    inputElement.style.left = (x - left).toFixed(0) + "px";
    inputElement.style.top = (y - top).toFixed(0) + "px";
  }
  static hide(inputElement) {
    inputElement.style.display = "none";
  }
  static show(inputElement) {
    inputElement.style.display = "block";
  }
  static setSurroundingText(inputElement, text, start, end) {
    if (!inputElement) {
      return;
    }
    inputElement.value = text;
    inputElement.setSelectionRange(start, end);
    inputElement.style.width = "20px";
    inputElement.style.width = `${inputElement.scrollWidth}px`;
  }
  static getModifiers(args) {
    let modifiers = 0 /* None */;
    if (args.ctrlKey) {
      modifiers |= 2 /* Control */;
    }
    if (args.altKey) {
      modifiers |= 1 /* Alt */;
    }
    if (args.shiftKey) {
      modifiers |= 4 /* Shift */;
    }
    if (args.metaKey) {
      modifiers |= 8 /* Meta */;
    }
    return modifiers;
  }
};

// modules/avalonia/dom.ts
var AvaloniaDOM = class {
  static addClass(element, className) {
    element.classList.add(className);
  }
  static createAvaloniaHost(host) {
    host.classList.add("avalonia-container");
    host.tabIndex = 0;
    host.oncontextmenu = function() {
      return false;
    };
    const canvas = document.createElement("canvas");
    canvas.classList.add("avalonia-canvas");
    canvas.style.backgroundColor = "#ccc";
    canvas.style.width = "100%";
    canvas.style.height = "100%";
    canvas.style.position = "absolute";
    const nativeHost = document.createElement("div");
    nativeHost.classList.add("avalonia-native-host");
    nativeHost.style.left = "0px";
    nativeHost.style.top = "0px";
    nativeHost.style.width = "100%";
    nativeHost.style.height = "100%";
    nativeHost.style.position = "absolute";
    const inputElement = document.createElement("input");
    inputElement.classList.add("avalonia-input-element");
    inputElement.autocapitalize = "none";
    inputElement.type = "text";
    inputElement.spellcheck = false;
    inputElement.style.padding = "0";
    inputElement.style.margin = "0";
    inputElement.style.position = "absolute";
    inputElement.style.overflow = "hidden";
    inputElement.style.borderStyle = "hidden";
    inputElement.style.outline = "none";
    inputElement.style.background = "transparent";
    inputElement.style.color = "transparent";
    inputElement.style.display = "none";
    inputElement.style.height = "20px";
    inputElement.onpaste = function() {
      return false;
    };
    inputElement.oncopy = function() {
      return false;
    };
    inputElement.oncut = function() {
      return false;
    };
    host.prepend(inputElement);
    host.prepend(nativeHost);
    host.prepend(canvas);
    return {
      host,
      canvas,
      nativeHost,
      inputElement
    };
  }
};

// modules/avalonia/caniuse.ts
var Caniuse = class {
  static canShowOpenFilePicker() {
    return typeof window.showOpenFilePicker !== "undefined";
  }
  static canShowSaveFilePicker() {
    return typeof window.showSaveFilePicker !== "undefined";
  }
  static canShowDirectoryPicker() {
    return typeof window.showDirectoryPicker !== "undefined";
  }
};

// modules/avalonia/stream.ts
var StreamHelper = class {
  static seek(stream, position) {
    return __async(this, null, function* () {
      return yield stream.seek(position);
    });
  }
  static truncate(stream, size) {
    return __async(this, null, function* () {
      return yield stream.truncate(size);
    });
  }
  static close(stream) {
    return __async(this, null, function* () {
      return yield stream.close();
    });
  }
  static write(stream, span) {
    return __async(this, null, function* () {
      const array = new Uint8Array(span.byteLength);
      span.copyTo(array);
      const data = {
        type: "write",
        data: array
      };
      return yield stream.write(data);
    });
  }
  static byteLength(stream) {
    return stream.size;
  }
  static sliceArrayBuffer(stream, offset, count) {
    return __async(this, null, function* () {
      const buffer = yield stream.slice(offset, offset + count).arrayBuffer();
      return new Uint8Array(buffer);
    });
  }
  static toMemoryView(buffer) {
    return buffer;
  }
};

// modules/avalonia/nativeControlHost.ts
var NativeControlHostTopLevelAttachment = class {
};
var NativeControlHost = class {
  static createDefaultChild(parent) {
    return document.createElement("div");
  }
  static createAttachment() {
    return new NativeControlHostTopLevelAttachment();
  }
  static initializeWithChildHandle(element, child) {
    element._child = child;
    element._child.style.position = "absolute";
  }
  static attachTo(element, host) {
    if (element._host && element._child) {
      element._host.removeChild(element._child);
    }
    element._host = host;
    if (element._host && element._child) {
      element._host.appendChild(element._child);
    }
  }
  static showInBounds(element, x, y, width, height) {
    if (element._child) {
      element._child.style.top = `${y}px`;
      element._child.style.left = `${x}px`;
      element._child.style.width = `${width}px`;
      element._child.style.height = `${height}px`;
      element._child.style.display = "block";
    }
  }
  static hideWithSize(element, width, height) {
    if (element._child) {
      element._child.style.width = `${width}px`;
      element._child.style.height = `${height}px`;
      element._child.style.display = "none";
    }
  }
  static releaseChild(element) {
    if (element._child) {
      element._child = void 0;
    }
  }
};

// modules/avalonia.ts
function createAvaloniaRuntime(api) {
  return __async(this, null, function* () {
    api.setModuleImports("avalonia.ts", {
      Caniuse,
      Canvas,
      InputHelper,
      SizeWatcher,
      DpiWatcher,
      AvaloniaDOM,
      StreamHelper,
      NativeControlHost
    });
  });
}
export {
  createAvaloniaRuntime
};
//# sourceMappingURL=avalonia.js.map
