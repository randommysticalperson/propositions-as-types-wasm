// ============================================================
//  main.js  —  .NET WASM bootstrap
//  Boots the .NET 8 WASM runtime, provides JSImport targets,
//  and wires up the interactive UI by calling JSExport methods
//  on PATBridge (defined in Program.cs).
// ============================================================

import { dotnet } from './_framework/dotnet.js';

// Boot the .NET runtime
const { setModuleImports, getAssemblyExports, getConfig } = await dotnet
    .withDiagnosticTracing(false)
    .create();

// Provide JSImport targets (JS functions callable from C#)
setModuleImports('main.js', {
    dom: {
        setInnerHTML: (selector, html) => {
            const el = document.querySelector(selector);
            if (el) el.innerHTML = html;
        }
    }
});

// Get all JSExport methods from the main assembly
const config  = getConfig();
const exports = await getAssemblyExports(config.mainAssemblyName);
const bridge  = exports.PATBridge;

// Populate static tables (Curry-Howard + Rules)
document.getElementById('ch-table').innerHTML    = bridge.GetCorrespondenceTableHTML();
document.getElementById('rules-table').innerHTML = bridge.GetRulesTableHTML();

// Wire up demo buttons
const output = document.getElementById('proof-output');

document.querySelectorAll('.demo-btn').forEach(btn => {
    btn.addEventListener('click', () => {
        document.querySelectorAll('.demo-btn').forEach(b => b.classList.remove('active'));
        btn.classList.add('active');
        const html = bridge.RunDemo(btn.dataset.demo);
        output.innerHTML = html;
    });
});

// Hide loading overlay
document.getElementById('loading').classList.add('hidden');

// Start the .NET runtime loop
await dotnet.run();
