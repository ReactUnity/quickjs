

console.log("in worker");

setInterval(function () {
    // console.log("worker log");
    postMessage("message form worker");
}, 30000)

onmessage = function (data) {
    console.log("worker get message from master:", data);
}
