import { dotnet } from './_framework/dotnet.js';

try {
    const runtime = await dotnet
        .withDiagnosticTracing(false)
        .withApplicationArgumentsFromQuery()
        .create();

    const config = runtime.getConfig();
    await runtime.runMain(config.mainAssemblyName, [globalThis.location.href]);
} catch (error) {
    console.error('NodeEditor could not start.', error);
    const splash = document.querySelector('.avalonia-splash');
    if (splash) {
        splash.querySelector('p').textContent = 'Unable to load NodeEditor. Reload the page or check the browser console.';
        splash.setAttribute('role', 'alert');
    }
}
