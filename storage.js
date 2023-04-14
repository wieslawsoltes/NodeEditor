var oe=Object.defineProperty;var pe=(n,e,t)=>e in n?oe(n,e,{enumerable:!0,configurable:!0,writable:!0,value:t}):n[e]=t;var p=(n,e)=>()=>(n&&(e=n(n=0)),e);var T=(n,e)=>{for(var t in e)oe(n,t,{get:e[t],enumerable:!0})};var m=(n,e,t)=>(pe(n,typeof e!="symbol"?e+"":e,t),t);var he,z,X=p(()=>{he={ReadableStream:globalThis.ReadableStream,WritableStream:globalThis.WritableStream,TransformStream:globalThis.TransformStream,DOMException:globalThis.DOMException,Blob:globalThis.Blob,File:globalThis.File},z=he});var W={};T(W,{FileHandle:()=>x,FolderHandle:()=>F,Sink:()=>q,default:()=>ve});var S,fe,s,ye,h,ae,be,Y,wt,we,q,x,F,ge,ve,N=p(()=>{D();X();({File:S,Blob:fe,DOMException:s}=z),{INVALID:ye,GONE:h,MISMATCH:ae,MOD_ERR:be,SYNTAX:Y,SECURITY:wt,DISALLOWED:we}=C,q=class{constructor(e,t){this.fileHandle=e,this.file=t,this.size=t.size,this.position=0}write(e){let t=this.file;if(typeof e=="object"){if(e.type==="write"){if(Number.isInteger(e.position)&&e.position>=0&&(this.position=e.position,this.size<e.position&&(this.file=new S([this.file,new ArrayBuffer(e.position-this.size)],this.file.name,this.file))),!("data"in e))throw new s(...Y("write requires a data argument"));e=e.data}else if(e.type==="seek")if(Number.isInteger(e.position)&&e.position>=0){if(this.size<e.position)throw new s(...ye);this.position=e.position;return}else throw new s(...Y("seek requires a position argument"));else if(e.type==="truncate")if(Number.isInteger(e.size)&&e.size>=0){t=e.size<this.size?new S([t.slice(0,e.size)],t.name,t):new S([t,new Uint8Array(e.size-this.size)],t.name),this.size=t.size,this.position>t.size&&(this.position=t.size),this.file=t;return}else throw new s(...Y("truncate requires a size argument"))}e=new fe([e]);let i=this.file,r=i.slice(0,this.position),o=i.slice(this.position+e.size),a=this.position-r.size;a<0&&(a=0),i=new S([r,new Uint8Array(a),e,o],i.name),this.size=i.size,this.position+=e.size,this.file=i}close(){if(this.fileHandle._deleted)throw new s(...h);this.fileHandle._file=this.file,this.file=this.position=this.size=null,this.fileHandle.onclose&&this.fileHandle.onclose(this.fileHandle)}},x=class{constructor(e="",t=new S([],e),i=!0){this._file=t,this.name=e,this.kind="file",this._deleted=!1,this.writable=i,this.readable=!0}async getFile(){if(this._deleted)throw new s(...h);return this._file}async createWritable(e){if(!this.writable)throw new s(...we);if(this._deleted)throw new s(...h);let t=e.keepExistingData?await this.getFile():new S([],this.name);return new q(this,t)}async isSameEntry(e){return this===e}async _destroy(){this._deleted=!0,this._file=null}},F=class{constructor(e,t=!0){this.name=e,this.kind="directory",this._deleted=!1,this._entries={},this.writable=t,this.readable=!0}async*entries(){if(this._deleted)throw new s(...h);yield*Object.entries(this._entries)}async isSameEntry(e){return this===e}async getDirectoryHandle(e,t){if(this._deleted)throw new s(...h);let i=this._entries[e];if(i){if(i instanceof x)throw new s(...ae);return i}else{if(t.create)return this._entries[e]=new F(e);throw new s(...h)}}async getFileHandle(e,t){let i=this._entries[e],r=i instanceof x;if(i&&r)return i;if(i&&!r)throw new s(...ae);if(!i&&!t.create)throw new s(...h);if(!i&&t.create)return this._entries[e]=new x(e)}async removeEntry(e,t){let i=this._entries[e];if(!i)throw new s(...h);await i._destroy(t.recursive),delete this._entries[e]}async _destroy(e){for(let t of Object.values(this._entries)){if(!e)throw new s(...be);await t._destroy(e)}this._entries={},this._deleted=!0}},ge=new F(""),ve=()=>ge});var Q={};T(Q,{FileHandle:()=>I,FolderHandle:()=>f,default:()=>Se});var Ee,O,I,f,Se,J=p(()=>{D();({DISALLOWED:Ee}=C),O=class{constructor(e,t){this.writer=e,this.fileEntry=t}async write(e){if(typeof e=="object"){if(e.type==="write"){if(Number.isInteger(e.position)&&e.position>=0&&(this.writer.seek(e.position),this.writer.position!==e.position&&(await new Promise((t,i)=>{this.writer.onwriteend=t,this.writer.onerror=i,this.writer.truncate(e.position)}),this.writer.seek(e.position))),!("data"in e))throw new DOMException("Failed to execute 'write' on 'UnderlyingSinkBase': Invalid params passed. write requires a data argument","SyntaxError");e=e.data}else if(e.type==="seek")if(Number.isInteger(e.position)&&e.position>=0){if(this.writer.seek(e.position),this.writer.position!==e.position)throw new DOMException("seeking position failed","InvalidStateError");return}else throw new DOMException("Failed to execute 'write' on 'UnderlyingSinkBase': Invalid params passed. seek requires a position argument","SyntaxError");else if(e.type==="truncate")return new Promise(t=>{if(Number.isInteger(e.size)&&e.size>=0)this.writer.onwriteend=i=>t(),this.writer.truncate(e.size);else throw new DOMException("Failed to execute 'write' on 'UnderlyingSinkBase': Invalid params passed. truncate requires a size argument","SyntaxError")})}await new Promise((t,i)=>{this.writer.onwriteend=t,this.writer.onerror=i,this.writer.write(new Blob([e]))})}close(){return new Promise(this.fileEntry.file.bind(this.fileEntry))}},I=class{constructor(e,t=!0){this.file=e,this.kind="file",this.writable=t,this.readable=!0}get name(){return this.file.name}isSameEntry(e){return this.file.toURL()===e.file.toURL()}getFile(){return new Promise(this.file.file.bind(this.file))}createWritable(e){if(!this.writable)throw new DOMException(...Ee);return new Promise((t,i)=>this.file.createWriter(r=>{e.keepExistingData===!1?(r.onwriteend=o=>t(new O(r,this.file)),r.truncate(0)):t(new O(r,this.file))},i))}},f=class{constructor(e,t=!0){this.dir=e,this.writable=t,this.readable=!0,this.kind="directory",this.name=e.name}isSameEntry(e){return this.dir.fullPath===e.dir.fullPath}async*entries(){let e=this.dir.createReader(),t=await new Promise(e.readEntries.bind(e));for(let i of t)yield[i.name,i.isFile?new I(i,this.writable):new f(i,this.writable)]}getDirectoryHandle(e,t){return new Promise((i,r)=>{this.dir.getDirectory(e,t,o=>{i(new f(o))},r)})}getFileHandle(e,t){return new Promise((i,r)=>this.dir.getFile(e,t,o=>i(new I(o)),r))}async removeEntry(e,t){let i=await this.getDirectoryHandle(e,{create:!1}).catch(r=>r.name==="TypeMismatchError"?this.getFileHandle(e,{create:!1}):r);if(i instanceof Error)throw i;return new Promise((r,o)=>{i instanceof f?t.recursive?i.dir.removeRecursively(()=>r(),o):i.dir.remove(()=>r(),o):i.file&&i.file.remove(()=>r(),o)})}},Se=(n={})=>new Promise((e,t)=>window.webkitRequestFileSystem(n._persistent,0,i=>e(new f(i.root)),t))});var y,xe,_,M,R=p(()=>{y=Symbol("adapter"),_=class{constructor(e){m(this,xe);m(this,"name");m(this,"kind");this.kind=e.kind,this.name=e.name,this[y]=e}async queryPermission({mode:e="read"}={}){let t=this[y];if(t.queryPermission)return t.queryPermission({mode:e});if(e==="read")return"granted";if(e==="readwrite")return t.writable?"granted":"denied";throw new TypeError(`Mode ${e} must be 'read' or 'readwrite'`)}async requestPermission({mode:e="read"}={}){let t=this[y];if(t.requestPermission)return t.requestPermission({mode:e});if(e==="read")return"granted";if(e==="readwrite")return t.writable?"granted":"denied";throw new TypeError(`Mode ${e} must be 'read' or 'readwrite'`)}async remove(e={}){await this[y].remove(e)}async isSameEntry(e){return this===e?!0:!e||typeof e!="object"||this.kind!==e.kind||!e[y]?!1:this[y].isSameEntry(e[y])}};xe=y;Object.defineProperty(_.prototype,Symbol.toStringTag,{value:"FileSystemHandle",writable:!1,enumerable:!1,configurable:!0});M=_});var Fe,g,Z,ee=p(()=>{X();({WritableStream:Fe}=z),g=class extends Fe{constructor(...e){super(...e),Object.setPrototypeOf(this,g.prototype),this._closed=!1}close(){this._closed=!0;let e=this.getWriter(),t=e.close();return e.releaseLock(),t}seek(e){return this.write({type:"seek",position:e})}truncate(e){return this.write({type:"truncate",size:e})}write(e){if(this._closed)return Promise.reject(new TypeError("Cannot write to a CLOSED writable stream"));let t=this.getWriter(),i=t.write(e);return t.releaseLock(),i}};Object.defineProperty(g.prototype,Symbol.toStringTag,{value:"FileSystemWritableFileStream",writable:!1,enumerable:!1,configurable:!0});Object.defineProperties(g.prototype,{close:{enumerable:!0},seek:{enumerable:!0},truncate:{enumerable:!0},write:{enumerable:!0}});Z=g});var H={};T(H,{FileSystemFileHandle:()=>L,default:()=>se});var j,Le,L,se,v=p(()=>{R();ee();j=Symbol("adapter"),L=class extends M{constructor(t){super(t);m(this,Le);this[j]=t}async createWritable(t={}){return new Z(await this[j].createWritable(t))}async getFile(){return this[j].getFile()}};Le=j;Object.defineProperty(L.prototype,Symbol.toStringTag,{value:"FileSystemFileHandle",writable:!1,enumerable:!1,configurable:!0});Object.defineProperties(L.prototype,{createWritable:{enumerable:!0},getFile:{enumerable:!0}});se=L});var K={};T(K,{FileSystemDirectoryHandle:()=>d,default:()=>le});var b,He,d,le,A=p(()=>{R();b=Symbol("adapter"),d=class extends M{constructor(t){super(t);m(this,He);this[b]=t}async getDirectoryHandle(t,i={}){if(t==="")throw new TypeError("Name can't be an empty string.");if(t==="."||t===".."||t.includes("/"))throw new TypeError("Name contains invalid characters.");i.create=!!i.create;let r=await this[b].getDirectoryHandle(t,i);return new d(r)}async*entries(){let{FileSystemFileHandle:t}=await Promise.resolve().then(()=>(v(),H));for await(let[i,r]of this[b].entries())yield[r.name,r.kind==="file"?new t(r):new d(r)]}async*getEntries(){let{FileSystemFileHandle:t}=await Promise.resolve().then(()=>(v(),H));console.warn("deprecated, use .entries() instead");for await(let i of this[b].entries())yield i.kind==="file"?new t(i):new d(i)}async getFileHandle(t,i={}){let{FileSystemFileHandle:r}=await Promise.resolve().then(()=>(v(),H));if(t==="")throw new TypeError("Name can't be an empty string.");if(t==="."||t===".."||t.includes("/"))throw new TypeError("Name contains invalid characters.");i.create=!!i.create;let o=await this[b].getFileHandle(t,i);return new r(o)}async removeEntry(t,i={}){if(t==="")throw new TypeError("Name can't be an empty string.");if(t==="."||t===".."||t.includes("/"))throw new TypeError("Name contains invalid characters.");return i.recursive=!!i.recursive,this[b].removeEntry(t,i)}async resolve(t){if(await t.isSameEntry(this))return[];let i=[{handle:this,path:[]}];for(;i.length;){let{handle:r,path:o}=i.pop();for await(let a of r.values()){if(await a.isSameEntry(t))return[...o,a.name];a.kind==="directory"&&i.push({handle:a,path:[...o,a.name]})}}return null}async*keys(){for await(let[t]of this[b].entries())yield t}async*values(){for await(let[t,i]of this)yield i}[(He=b,Symbol.asyncIterator)](){return this.entries()}};Object.defineProperty(d.prototype,Symbol.toStringTag,{value:"FileSystemDirectoryHandle",writable:!1,enumerable:!1,configurable:!0});Object.defineProperties(d.prototype,{getDirectoryHandle:{enumerable:!0},entries:{enumerable:!0},getFileHandle:{enumerable:!0},removeEntry:{enumerable:!0}});le=d});var te={};T(te,{config:()=>Te,errors:()=>C,fromDataTransfer:()=>Pe,getDirHandlesFromInput:()=>ke,getFileHandlesFromInput:()=>Ce});async function Pe(n){console.warn("deprecated fromDataTransfer - use `dt.items[0].getAsFileSystemHandle()` instead");let[e,t,i]=await Promise.all([Promise.resolve().then(()=>(N(),W)),Promise.resolve().then(()=>(J(),Q)),Promise.resolve().then(()=>(A(),K))]),r=new e.FolderHandle("",!1);return r._entries=n.map(o=>o.isFile?new t.FileHandle(o,!1):new t.FolderHandle(o,!1)),new i.FileSystemDirectoryHandle(r)}async function ke(n){let{FolderHandle:e,FileHandle:t}=await Promise.resolve().then(()=>(N(),W)),{FileSystemDirectoryHandle:i}=await Promise.resolve().then(()=>(A(),K)),r=Array.from(n.files),o=r[0].webkitRelativePath.split("/",1)[0],a=new e(o,!1);return r.forEach(l=>{let u=l.webkitRelativePath.split("/");u.shift();let G=u.pop(),ue=u.reduce((U,B)=>(U._entries[B]||(U._entries[B]=new e(B,!1)),U._entries[B]),a);ue._entries[G]=new t(l.name,l,!1)}),new i(a)}async function Ce(n){let{FileHandle:e}=await Promise.resolve().then(()=>(N(),W)),{FileSystemFileHandle:t}=await Promise.resolve().then(()=>(v(),H));return Array.from(n.files).map(i=>new t(new e(i.name,i,!1)))}var C,Te,D=p(()=>{C={INVALID:["seeking position failed.","InvalidStateError"],GONE:["A requested file or directory could not be found at the time an operation was processed.","NotFoundError"],MISMATCH:["The path supplied exists, but was not an entry of requested type.","TypeMismatchError"],MOD_ERR:["The object can not be modified in this way.","InvalidModificationError"],SYNTAX:n=>[`Failed to execute 'write' on 'UnderlyingSinkBase': Invalid params passed. ${n}`,"SyntaxError"],SECURITY:["It was determined that certain files are unsafe for access within a Web application, or that too many calls are being made on file resources.","SecurityError"],DISALLOWED:["The request is not allowed by the user agent or the platform in the current context.","NotAllowedError"]},Te={writable:globalThis.WritableStream}});var V=class{constructor(e){this.database=e}openStore(e,t){return this.database.transaction(e,t).objectStore(e)}async put(e,t,i){let r=this.openStore(e,"readwrite");return await new Promise((o,a)=>{let l=r.put(t,i);l.onsuccess=()=>{o(l.result)},l.onerror=()=>{a(l.error)}})}get(e,t){let i=this.openStore(e,"readonly");return new Promise((r,o)=>{let a=i.get(t);a.onsuccess=()=>{r(a.result)},a.onerror=()=>{o(a.error)}})}async delete(e,t){let i=this.openStore(e,"readwrite");return await new Promise((r,o)=>{let a=i.delete(t);a.onsuccess=()=>{r()},a.onerror=()=>{o(a.error)}})}close(){this.database.close()}},$=class{constructor(e,t){this.databaseName=e;this.objectStores=t}async connect(){let e=window.indexedDB.open(this.databaseName,1);return e.onupgradeneeded=t=>{let i=t.target.result;this.objectStores.forEach(r=>{i.createObjectStore(r)})},await new Promise((t,i)=>{e.onsuccess=r=>{t(new V(r.target.result))},e.onerror=r=>{i(r.target.error)}})}},E="fileBookmarks",P=new $("AvaloniaDb",[E]);var me=typeof window!="undefined",qe=me&&window.mozInnerScreenX!=null;var w=class{static hasNativeFilePicker(){return"showSaveFilePicker"in globalThis}static isMobile(){var o;let e=(o=globalThis.navigator)==null?void 0:o.userAgentData;if(e)return e.mobile;let t=navigator.userAgent,i=/(android|bb\d+|meego).+mobile|avantgo|bada\/|blackberry|blazer|compal|elaine|fennec|hiptop|iemobile|ip(hone|od)|iris|kindle|lge |maemo|midp|mmp|mobile.+firefox|netfront|opera m(ob|in)i|palm( os)?|phone|p(ixi|re)\/|plucker|pocket|psp|series(4|6)0|symbian|treo|up\.(browser|link)|vodafone|wap|windows ce|xda|xiino/i,r=/1207|6310|6590|3gso|4thp|50[1-6]i|770s|802s|a wa|abac|ac(er|oo|s)|ai(ko|rn)|al(av|ca|co)|amoi|an(ex|ny|yw)|aptu|ar(ch|go)|as(te|us)|attw|au(di|m|r |s )|avan|be(ck|ll|nq)|bi(lb|rd)|bl(ac|az)|br(e|v)w|bumb|bw(n|u)|c55\/|capi|ccwa|cdm|cell|chtm|cldc|cmd|co(mp|nd)|craw|da(it|ll|ng)|dbte|dcs|devi|dica|dmob|do(c|p)o|ds(12|d)|el(49|ai)|em(l2|ul)|er(ic|k0)|esl8|ez([4-7]0|os|wa|ze)|fetc|fly(|_)|g1 u|g560|gene|gf5|gmo|go(\.w|od)|gr(ad|un)|haie|hcit|hd(m|p|t)|hei|hi(pt|ta)|hp( i|ip)|hsc|ht(c(| |_|a|g|p|s|t)|tp)|hu(aw|tc)|i(20|go|ma)|i230|iac( ||\/)|ibro|idea|ig01|ikom|im1k|inno|ipaq|iris|ja(t|v)a|jbro|jemu|jigs|kddi|keji|kgt( |\/)|klon|kpt |kwc|kyo(c|k)|le(no|xi)|lg( g|\/(k|l|u)|50|54|[a-w])|libw|lynx|m1w|m3ga|m50\/|ma(te|ui|xo)|mc(01|21|ca)|mcr|me(rc|ri)|mi(o8|oa|ts)|mmef|mo(01|02|bi|de|do|t(| |o|v)|zz)|mt(50|p1|v )|mwbp|mywa|n10[0-2]|n20[2-3]|n30(0|2)|n50(0|2|5)|n7(0(0|1)|10)|ne((c|m)|on|tf|wf|wg|wt)|nok(6|i)|nzph|o2im|op(ti|wv)|oran|owg1|p800|pan(a|d|t)|pdxg|pg(13|([1-8]|c))|phil|pire|pl(ay|uc)|pn2|po(ck|rt|se)|prox|psio|ptg|qaa|qc(07|12|21|32|60|[2-7]|i)|qtek|r380|r600|raks|rim9|ro(ve|zo)|s55\/|sa(ge|ma|mm|ms|ny|va)|sc(01|h|oo|p)|sdk\/|se(c(|0|1)|47|mc|nd|ri)|sgh|shar|sie(|m)|sk0|sl(45|id)|sm(al|ar|b3|it|t5)|so(ft|ny)|sp(01|h|v|v )|sy(01|mb)|t2(18|50)|t6(00|10|18)|ta(gt|lk)|tcl|tdg|tel(i|m)|tim|tmo|to(pl|sh)|ts(70|m|m3|m5)|tx9|up(\.b|g1|si)|utst|v400|v750|veri|vi(rg|te)|vk(40|5[0-3]|v)|vm40|voda|vulc|vx(52|53|60|61|70|80|81|83|85|98)|w3c(| )|webc|whit|wi(g |nc|nw)|wmlb|wonu|x700|yas|your|zeto|zte/i;return i.test(t)||r.test(t.substr(0,4))}};var c=class{constructor(e,t,i,r){this.handle=e;this.file=t;this.bookmarkId=i;this.wellKnownType=r}get name(){var e;return this.handle?this.handle.name:this.file?this.file.name:(e=this.wellKnownType)!=null?e:""}get kind(){return this.handle?this.handle.kind:this.file?"file":"directory"}static createFromHandle(e,t){return new c(e,void 0,t,void 0)}static createFromFile(e){return new c(void 0,e,void 0,void 0)}static createWellKnownDirectory(e){return new c(void 0,void 0,void 0,e)}static async openRead(e){if(e.file)return e.file;if(!e.handle||e.kind!=="file")throw new Error("StorageItem is not a file");return await e.verityPermissions("read"),await e.handle.getFile()}static async openWrite(e){if(!e.handle||e.kind!=="file")throw new Error("StorageItem is not a writeable file");return await e.verityPermissions("readwrite"),await e.handle.createWritable({keepExistingData:!0})}static async getProperties(e){try{let t=e.handle&&"getFile"in e.handle?await e.handle.getFile():e.file;return t?{Size:t.size,LastModified:t.lastModified,Type:t.type}:null}catch(t){return null}}static getItemsIterator(e){return e.kind!=="directory"||!e.handle?null:e.handle.entries()}static async createFile(e,t){if(e.kind!=="directory"||!e.handle)throw new TypeError("Unable to create item in the requested directory");return await e.verityPermissions("readwrite"),await e.handle.getFileHandle(t,{create:!0})}static async createFolder(e,t){if(e.kind!=="directory"||!e.handle)throw new TypeError("Unable to create item in the requested directory");return await e.verityPermissions("readwrite"),await e.handle.getDirectoryHandle(t,{create:!0})}static async deleteAsync(e){return e.handle?(await e.verityPermissions("readwrite"),await e.handle.remove({recursive:!0})):null}static async moveAsync(e,t){if(!e.handle)return null;if(t.kind!=="directory"||!t.handle)throw new TypeError("Unable to move item to the requested directory");return await e.verityPermissions("readwrite"),await e.handle.move(t)}async verityPermissions(e){if(!!this.handle&&!!w.hasNativeFilePicker()&&await this.handle.queryPermission({mode:e})!=="granted"&&await this.handle.requestPermission({mode:e})==="denied")throw new Error("Permissions denied")}static async saveBookmark(e){if(e.bookmarkId)return e.bookmarkId;if(!e.handle||!w.hasNativeFilePicker())return null;let t=await P.connect();try{return await t.put(E,e.handle,e.generateBookmarkId())}finally{t.close()}}static async deleteBookmark(e){if(!e.bookmarkId||!w.hasNativeFilePicker())return;let t=await P.connect();try{await t.delete(E,e.bookmarkId)}finally{t.close()}}generateBookmarkId(){return Date.now().toString(36)+Math.random().toString(36).substring(2)}},k=class{constructor(e){this.items=e}static itemsArray(e){return e.items}static filesToItemsArray(e){if(!e)return[];let t=[];for(let i=0;i<e.length;i++)t[i]=c.createFromFile(e[i]);return t}};var ce=globalThis.showDirectoryPicker;async function De(n={}){if(ce&&!n._preferPolyfill)return ce(n);let e=document.createElement("input");if(e.type="file",!("webkitdirectory"in e))throw new Error("HTMLInputElement.webkitdirectory is not supported");e.style.position="fixed",e.style.top="-100000px",e.style.left="-100000px",document.body.appendChild(e),e.webkitdirectory=!0;let t=Promise.resolve().then(()=>(D(),te));return await new Promise(i=>{e.addEventListener("change",i),e.click()}),t.then(i=>i.getDirHandlesFromInput(e))}var ie=De;var Ie={accepts:[]},de=globalThis.showOpenFilePicker;async function Me(n={}){let e={...Ie,...n};if(de&&!n._preferPolyfill)return de(e);let t=document.createElement("input");t.type="file",t.multiple=e.multiple,t.accept=(e.accepts||[]).map(r=>[...(r.extensions||[]).map(o=>"."+o),...r.mimeTypes||[]]).flat().join(","),t.style.position="fixed",t.style.top="-100000px",t.style.left="-100000px",document.body.appendChild(t);let i=Promise.resolve().then(()=>(D(),te));return await new Promise(r=>{t.addEventListener("change",r),t.click()}),i.then(r=>r.getFileHandlesFromInput(t))}var re=Me;var Dt=globalThis.showSaveFilePicker;globalThis.DataTransferItem&&!DataTransferItem.prototype.getAsFileSystemHandle&&(DataTransferItem.prototype.getAsFileSystemHandle=async function(){let n=this.webkitGetAsEntry(),[{FileHandle:e,FolderHandle:t},{FileSystemDirectoryHandle:i},{FileSystemFileHandle:r}]=await Promise.all([Promise.resolve().then(()=>(J(),Q)),Promise.resolve().then(()=>(A(),K)),Promise.resolve().then(()=>(v(),H))]);return n.isFile?new r(new e(n,!1)):new i(new t(n,!1))});A();v();R();ee();var ne=class{static async selectFolderDialog(e){var r,o;let t={startIn:(o=(r=e==null?void 0:e.wellKnownType)!=null?r:e==null?void 0:e.handle)!=null?o:void 0},i=await ie(t);return c.createFromHandle(i)}static async openFileDialog(e,t,i,r){var l,u;let o={startIn:(u=(l=e==null?void 0:e.wellKnownType)!=null?l:e==null?void 0:e.handle)!=null?u:void 0,multiple:t,excludeAcceptAllOption:r,types:i!=null?i:void 0},a=await re(o);return new k(a.map(G=>c.createFromHandle(G)))}static async saveFileDialog(e,t,i,r){var l,u;let o={startIn:(u=(l=e==null?void 0:e.wellKnownType)!=null?l:e==null?void 0:e.handle)!=null?u:void 0,suggestedName:t!=null?t:void 0,excludeAcceptAllOption:r,types:i!=null?i:void 0},a=await globalThis.showSaveFilePicker(o);return c.createFromHandle(a)}static async openBookmark(e){let t=await P.connect();try{let i=await t.get(E,e);return i&&c.createFromHandle(i,e)}finally{t.close()}}static createAcceptType(e,t,i){let r={};return t.forEach(o=>{r[o]=i!=null?i:[]}),{description:e,accept:r}}};export{c as StorageItem,k as StorageItems,ne as StorageProvider};
//# sourceMappingURL=storage.js.map