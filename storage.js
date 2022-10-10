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
var __forAwait = (obj, it, method) => {
  it = obj[Symbol.asyncIterator];
  method = (key, fn) => (fn = obj[key]) && (it[key] = (arg) => new Promise((resolve, reject, done) => {
    arg = fn.call(obj, arg);
    done = arg.done;
    return Promise.resolve(arg.value).then((value) => resolve({ value, done }), reject);
  }));
  return it ? it.call(obj) : (obj = obj[Symbol.iterator](), it = {}, method("next"), method("return"), it);
};

// modules/storage/indexedDb.ts
var InnerDbConnection = class {
  constructor(database) {
    this.database = database;
  }
  openStore(store, mode) {
    const tx = this.database.transaction(store, mode);
    return tx.objectStore(store);
  }
  put(store, obj, key) {
    return __async(this, null, function* () {
      const os = this.openStore(store, "readwrite");
      return yield new Promise((resolve, reject) => {
        const response = os.put(obj, key);
        response.onsuccess = () => {
          resolve(response.result);
        };
        response.onerror = () => {
          reject(response.error);
        };
      });
    });
  }
  get(store, key) {
    const os = this.openStore(store, "readonly");
    return new Promise((resolve, reject) => {
      const response = os.get(key);
      response.onsuccess = () => {
        resolve(response.result);
      };
      response.onerror = () => {
        reject(response.error);
      };
    });
  }
  delete(store, key) {
    return __async(this, null, function* () {
      const os = this.openStore(store, "readwrite");
      return yield new Promise((resolve, reject) => {
        const response = os.delete(key);
        response.onsuccess = () => {
          resolve();
        };
        response.onerror = () => {
          reject(response.error);
        };
      });
    });
  }
  close() {
    this.database.close();
  }
};
var IndexedDbWrapper = class {
  constructor(databaseName, objectStores) {
    this.databaseName = databaseName;
    this.objectStores = objectStores;
  }
  connect() {
    return __async(this, null, function* () {
      const conn = window.indexedDB.open(this.databaseName, 1);
      conn.onupgradeneeded = (event) => {
        const db = event.target.result;
        this.objectStores.forEach((store) => {
          db.createObjectStore(store);
        });
      };
      return yield new Promise((resolve, reject) => {
        conn.onsuccess = (event) => {
          resolve(new InnerDbConnection(event.target.result));
        };
        conn.onerror = (event) => {
          reject(event.target.error);
        };
      });
    });
  }
};
var fileBookmarksStore = "fileBookmarks";
var avaloniaDb = new IndexedDbWrapper("AvaloniaDb", [
  fileBookmarksStore
]);

// modules/storage/storageItem.ts
var StorageItem = class {
  constructor(handle, bookmarkId) {
    this.handle = handle;
    this.bookmarkId = bookmarkId;
  }
  get name() {
    return this.handle.name;
  }
  get kind() {
    return this.handle.kind;
  }
  static openRead(item) {
    return __async(this, null, function* () {
      if (!(item.handle instanceof FileSystemFileHandle)) {
        throw new Error("StorageItem is not a file");
      }
      yield item.verityPermissions("read");
      const file = yield item.handle.getFile();
      return file;
    });
  }
  static openWrite(item) {
    return __async(this, null, function* () {
      if (!(item.handle instanceof FileSystemFileHandle)) {
        throw new Error("StorageItem is not a file");
      }
      yield item.verityPermissions("readwrite");
      return yield item.handle.createWritable({ keepExistingData: true });
    });
  }
  static getProperties(item) {
    return __async(this, null, function* () {
      const file = item.handle instanceof FileSystemFileHandle && (yield item.handle.getFile());
      if (!file) {
        return null;
      }
      return {
        Size: file.size,
        LastModified: file.lastModified,
        Type: file.type
      };
    });
  }
  static getItems(item) {
    return __async(this, null, function* () {
      if (item.handle.kind !== "directory") {
        return new StorageItems([]);
      }
      const items = [];
      try {
        for (var iter = __forAwait(item.handle.entries()), more, temp, error; more = !(temp = yield iter.next()).done; more = false) {
          const [, value] = temp.value;
          items.push(new StorageItem(value));
        }
      } catch (temp) {
        error = [temp];
      } finally {
        try {
          more && (temp = iter.return) && (yield temp.call(iter));
        } finally {
          if (error)
            throw error[0];
        }
      }
      return new StorageItems(items);
    });
  }
  verityPermissions(mode) {
    return __async(this, null, function* () {
      if ((yield this.handle.queryPermission({ mode })) === "granted") {
        return;
      }
      if ((yield this.handle.requestPermission({ mode })) === "denied") {
        throw new Error("Permissions denied");
      }
    });
  }
  static saveBookmark(item) {
    return __async(this, null, function* () {
      if (item.bookmarkId) {
        return item.bookmarkId;
      }
      const connection = yield avaloniaDb.connect();
      try {
        const key = yield connection.put(fileBookmarksStore, item.handle, item.generateBookmarkId());
        return key;
      } finally {
        connection.close();
      }
    });
  }
  static deleteBookmark(item) {
    return __async(this, null, function* () {
      if (!item.bookmarkId) {
        return;
      }
      const connection = yield avaloniaDb.connect();
      try {
        yield connection.delete(fileBookmarksStore, item.bookmarkId);
      } finally {
        connection.close();
      }
    });
  }
  generateBookmarkId() {
    return Date.now().toString(36) + Math.random().toString(36).substring(2);
  }
};
var StorageItems = class {
  constructor(items) {
    this.items = items;
  }
  static itemsArray(instance) {
    return instance.items;
  }
};

// modules/storage/storageProvider.ts
var StorageProvider = class {
  static selectFolderDialog(startIn) {
    return __async(this, null, function* () {
      var _a;
      const options = {
        startIn: (_a = startIn == null ? void 0 : startIn.handle) != null ? _a : void 0
      };
      const handle = yield window.showDirectoryPicker(options);
      return new StorageItem(handle);
    });
  }
  static openFileDialog(startIn, multiple, types, excludeAcceptAllOption) {
    return __async(this, null, function* () {
      var _a;
      const options = {
        startIn: (_a = startIn == null ? void 0 : startIn.handle) != null ? _a : void 0,
        multiple,
        excludeAcceptAllOption,
        types: types != null ? types : void 0
      };
      const handles = yield window.showOpenFilePicker(options);
      return new StorageItems(handles.map((handle) => new StorageItem(handle)));
    });
  }
  static saveFileDialog(startIn, suggestedName, types, excludeAcceptAllOption) {
    return __async(this, null, function* () {
      var _a;
      const options = {
        startIn: (_a = startIn == null ? void 0 : startIn.handle) != null ? _a : void 0,
        suggestedName: suggestedName != null ? suggestedName : void 0,
        excludeAcceptAllOption,
        types: types != null ? types : void 0
      };
      const handle = yield window.showSaveFilePicker(options);
      return new StorageItem(handle);
    });
  }
  static openBookmark(key) {
    return __async(this, null, function* () {
      const connection = yield avaloniaDb.connect();
      try {
        const handle = yield connection.get(fileBookmarksStore, key);
        return handle && new StorageItem(handle, key);
      } finally {
        connection.close();
      }
    });
  }
  static createAcceptType(description, mimeTypes) {
    const accept = {};
    mimeTypes.forEach((a) => {
      accept[a] = [];
    });
    return { description, accept };
  }
};
export {
  StorageItem,
  StorageItems,
  StorageProvider
};
//# sourceMappingURL=storage.js.map
