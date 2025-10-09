export function afterWebAssemblyStarted(blazor) {
    console.log("afterWebAssemblyStarted");
    console.log(blazor);
}

export function onRuntimeInitialized(args) {

    console.log("onRuntimeInitialized");
    console.log(args);
}