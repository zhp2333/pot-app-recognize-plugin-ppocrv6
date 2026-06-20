async function recognize(_base64, _lang, options) {
    const { config, utils } = options;
    const { run, cacheDir, pluginDir } = utils;

    // Read model size from plugin config (default: small)
    const modelSize = config.model_size || "small";

    // Path to the screenshot that Pot captures
    const screenshot = `${cacheDir}/pot_screenshot_cut.png`;

    // Run the self-contained OCR engine
    // Usage: pot_ocr.exe <image_path> [tiny|small|medium]
    const result = await run(
        `${pluginDir}/pot_ocr.exe`,
        [screenshot, modelSize]
    );

    if (result.status !== 0) {
        throw new Error(result.stderr || `Exit code: ${result.status}`);
    }

    const stdout = result.stdout.trim();
    if (!stdout) {
        throw new Error("Empty output from OCR engine");
    }

    let json;
    try {
        json = JSON.parse(stdout);
    } catch (e) {
        throw new Error(`Invalid JSON output: ${stdout.substring(0, 200)}`);
    }

    if (json.code !== 100 || !json.data || json.data.length === 0) {
        throw new Error(json.error || "No text recognized");
    }

    // Join all detected text lines
    return json.data
        .map(item => item.text)
        .filter(t => t && t.trim())
        .join("\n")
        .trim();
}
