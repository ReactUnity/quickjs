"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const file_watcher_1 = require("./file_watcher");
const jsb_1 = require("jsb");
if (typeof globalThis["__fw"] !== "undefined") {
    globalThis["__fw"].dispose();
    delete globalThis["__fw"];
}
let fw = new file_watcher_1.FileWatcher("Scripts", "*.js");
function collect_reload(mod, dirtylist) {
    if (dirtylist.indexOf(mod) < 0) {
        dirtylist.push(mod);
        let parent = mod.parent;
        if (typeof parent === "object") {
            collect_reload(parent, dirtylist);
            parent = parent.parent;
        }
    }
}
fw.on(file_watcher_1.FileWatcher.CHANGED, this, function (filestates) {
    let cache = require.main["cache"];
    let dirtylist = [];
    for (let name in filestates) {
        let filestate = filestates[name];
        // console.log("file changed:", filestate.name, filestate.fullPath, filestate.state);
        for (let moduleId in cache) {
            let mod = cache[moduleId];
            // console.warn(mod.filename, mod.filename == filestate.fullPath)
            if (mod.filename == filestate.fullPath) {
                collect_reload(mod, dirtylist);
                // delete cache[moduleId];
                break;
            }
        }
    }
    if (dirtylist.length > 0) {
        jsb_1.ModuleManager.BeginReload();
        for (let i = 0; i < dirtylist.length; i++) {
            let mod = dirtylist[i];
            console.warn("reloading", mod.id);
            jsb_1.ModuleManager.MarkReload(mod.id);
        }
        jsb_1.ModuleManager.EndReload();
    }
});
globalThis["__fw"] = fw;
console.log("i am here");
//# sourceMappingURL=js_reload.js.map